using Mirror;
using UnityEngine;

public class AbilityController : NetworkBehaviour
{

    public LayerMask wallLayer;

    public GameObject lime;

    private Transform mainCamera;

    public static AbilityController ac;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        ac = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(RoundManager.rm.hasLoaded && mainCamera == null)
        {
            mainCamera = RoundManager.rm.GetMyPlayer().GetComponentInChildren<Camera>().transform;
        }
    }


    public void Lime()
    {
        GameObject instance = Instantiate(lime, GetHitInForward(), Quaternion.identity);
        NetworkServer.Spawn(instance);
        RoundManager.spawns.Add(instance);
    }

    public Vector3 GetHitInForward()
    {
        
        Physics.Raycast(mainCamera.position, mainCamera.forward, out RaycastHit hit, 100, wallLayer);

        Vector3 offsetDirection = -mainCamera.forward;
        float offsetDistance = 0.1f; // 少し手前の距離（必要に応じて調整）

        return hit.point + offsetDirection * offsetDistance;

    }
}
