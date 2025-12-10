using Mirror;
using System.Collections;
using UnityEngine;

public class ScanCamera : NetworkBehaviour
{
    public Camera scanCamera;

    private Coroutine timerCoroutine;

    [SyncVar(hook = nameof(OnCameraStateChanged))]
    private bool isCameraOn = false;

    // Camera.enabled をフレーム末に反映する
    private bool pendingStateChange = false;
    private bool targetState = false;

    public void CameraTimeOn(float time)
    {
        // コルーチン管理はクライアント側だけでOK
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        timerCoroutine = StartCoroutine(OnTimer(time));
    }

    private IEnumerator OnTimer(float time)
    {
        // 状態が同じなら送らない（無駄なCmdを防止）
        CmdSetCameraState(true);

        yield return new WaitForSeconds(time);

        CmdSetCameraState(false);
    }

    [Command(requiresAuthority = false)]
    private void CmdSetCameraState(bool state)
    {
        if (isCameraOn == state) return; // 無駄な更新を防ぐ
        isCameraOn = state;
    }

    private void OnCameraStateChanged(bool oldValue, bool newValue)
    {
        // ここで長い処理をしない
        targetState = newValue;
        pendingStateChange = true;
    }

    private void LateUpdate()
    {
        // Camera.enabled 切替はフレーム末に実行 → 固まり防止
        if (pendingStateChange)
        {
            pendingStateChange = false;

            if (scanCamera != null)
            {
                scanCamera.enabled = targetState;
            }
        }
    }
}
