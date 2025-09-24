using Mirror;
using UnityEngine;

public class Darkness : NetworkBehaviour
{
    [SyncVar]
    public int darknessSize;

    public GameObject sparksLightLoad;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSize(int darknessSizeValue)
    {
        darknessSize = darknessSizeValue;
    }

    public void Collected()
    {
        GameObject instance = Instantiate(sparksLightLoad, transform.position, Quaternion.identity);
        NetworkServer.Spawn(instance);
    }

}
