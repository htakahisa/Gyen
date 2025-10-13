using Mirror;
using System.Collections;
using UnityEngine;

public class ScanCamera : NetworkBehaviour
{

    public Camera scanCamera;
    public Coroutine timerCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CameraTimeOn(float time)
    {
        if(timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(OnTimer(time));
    }

    public IEnumerator OnTimer(float time)
    {
        CmdCameraOn();
        yield return new WaitForSeconds(time);
        CmdCameraOff();
    }

    [Command(requiresAuthority = false)]
    public void CmdCameraOn()
    {
        RpcCameraOn();
    }

    [Command(requiresAuthority = false)]
    public void CmdCameraOff()
    {
        RpcCameraOff();
    }

    [ClientRpc]
    public void RpcCameraOn()
    {
        scanCamera.enabled = true;
    }

    [ClientRpc]
    public void RpcCameraOff()
    {
        scanCamera.enabled = false;
    }

}
