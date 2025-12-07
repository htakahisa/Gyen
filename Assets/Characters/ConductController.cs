using Mirror;
using UnityEngine;

public class ConductController : NetworkBehaviour
{

    public GameObject conductor;
    public GameObject conductorInstance; 
    public LayerMask groundMask;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetMouseButtonDown(2))
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 100, groundMask))
            {
                if(conductorInstance != null)
                {
                    Destroy(conductorInstance);
                }

                conductorInstance = Instantiate(conductor, hit.point, Quaternion.identity);

            }
        }
    }
}
