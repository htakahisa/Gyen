using Mirror;
using UnityEngine;

public class DarknessSize : NetworkBehaviour
{
    [SyncVar]
    public int darknessSize;

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

}
