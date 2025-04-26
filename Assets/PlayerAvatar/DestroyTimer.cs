using Mirror;
using UnityEngine;

public class DestroyTimer : NetworkBehaviour
{
    public float time;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Invoke("Destroy", time);
    }

    public void Destroy()
    {
        NetworkServer.Destroy(gameObject);
    }

}
