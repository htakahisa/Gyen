using Mirror;
using UnityEngine;

public class DestroyTimer : NetworkBehaviour
{
    public float time;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Invoke("RpcDestroy", time);
    }

    [ClientRpc]
    public void RpcDestroy()
    {
        NetworkServer.Destroy(gameObject);
    }

}
