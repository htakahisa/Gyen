using Mirror;
using UnityEngine;

public class TriggerSpawner : NetworkBehaviour
{
    [Header("Prefab to spawn (registered in NetworkManager)")]
    public GameObject Lime; // Inspector で割り当て済み

    public float speed = 6f;

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        Debug.Log(transform.forward);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return; // サーバーのみ処理

        if (other.gameObject.layer == 3)
        {
            SpawnAndDestroy();
        }
    }

    [Server] 
    private void SpawnAndDestroy()
    {
        if (Lime != null)
        {
            Vector3 spawnPos = transform.position -transform.forward;
            Quaternion spawnRot = Quaternion.identity;

            GameObject limeInstance = Instantiate(Lime, spawnPos, spawnRot);
            NetworkServer.Spawn(limeInstance);

            NetworkServer.Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Lime prefab is not assigned in TriggerSpawner!");
        }
    }
}
