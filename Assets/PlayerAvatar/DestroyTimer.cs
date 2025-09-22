using Mirror;
using UnityEngine;

public class DestroyTimer : NetworkBehaviour
{
    public float time;
    public OnTimeOut onTimeOut;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Invoke("CmdDestroy", time);
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroy()
    {
        if (onTimeOut == OnTimeOut.DESTROY)
        {
            NetworkServer.Destroy(gameObject);
        }
        if(onTimeOut == OnTimeOut.CALLSERVERCHECKSHOOTANDDESTROY)
        {
            NetworkIdentity owner = GetComponent<SpawnOwner>().WhoseThis();
            owner.GetComponentInChildren<ServerCheckShoot>().DestroyOrb(gameObject);
        }
    }

    public enum OnTimeOut
    {
        DESTROY,
        CALLSERVERCHECKSHOOTANDDESTROY,
    }




}
