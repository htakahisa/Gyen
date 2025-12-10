using Mirror;
using System.Collections;
using UnityEngine;

public class ScanCamera : NetworkBehaviour
{
    public Camera scanCamera;

    [SyncVar(hook = nameof(OnCameraStateChanged))]
    private bool isCameraOn = false;

    private Coroutine serverTimer;

    // クライアントが呼ぶ
    public void CameraTimeOn(float time)
    {
        if (!isLocalPlayer) return;

        CmdRequestCameraOn(time);
    }

    // サーバーだけが状態を管理する
    [Command(requiresAuthority = false)]
    private void CmdRequestCameraOn(float time)
    {
        if (serverTimer != null)
            StopCoroutine(serverTimer);

        serverTimer = StartCoroutine(ServerTimer(time));
    }

    private IEnumerator ServerTimer(float time)
    {
        isCameraOn = true;
        yield return new WaitForSeconds(time);
        isCameraOn = false;
    }

    private void OnCameraStateChanged(bool oldValue, bool newValue)
    {
        scanCamera.enabled = newValue;
    }
}
