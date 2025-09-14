using Mirror;
using UnityEngine;

public class FormManager : NetworkBehaviour
{

    public GameObject geometry;
    public GameObject camera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartAuthority()
    {
        geometry.layer = 7;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
