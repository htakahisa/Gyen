using Mirror;
using UnityEngine;

public class Lime : NetworkBehaviour
{

    public GameObject effect;
    public GameObject effectInstance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        ServerSpawnEffect();
        Invoke("ServerDestroyEffect", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Server]
    public void ServerSpawnEffect()
    {
        effectInstance = Instantiate(effect, transform.position, Quaternion.identity);
        NetworkServer.Spawn(effectInstance);
        RoundManager.spawns.Add(effectInstance);
    }
    [Server]
    public void ServerDestroyEffect()
    {
        NetworkServer.Destroy(effectInstance);
    }
}
