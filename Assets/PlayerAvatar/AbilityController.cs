using Mirror;
using UnityEngine;

public class AbilityController : NetworkBehaviour
{

    public LayerMask wallLayer;

    public GameObject lime;

    public int energy = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(RoundManager.rm.Mode == "Practice")
        {
            energy = 10000;
        }
    }


    public void Lime()
    {
        //�K�v��1
        if (energy >= 1)
        {
            Transform mainCamera = GetComponentInChildren<Camera>().transform;
            CmdLime(mainCamera.position, mainCamera.forward);
            energy--;
        }
    }

    [Command]
    public void CmdLime(Vector3 pos, Vector3 dir)
    {
        GameObject instance = Instantiate(lime, GetHitInForward(pos, dir), Quaternion.identity);
        NetworkServer.Spawn(instance);
        RoundManager.spawns.Add(instance);
    }

    public Vector3 GetHitInForward(Vector3 pos, Vector3 dir)
    {

        Physics.Raycast(pos, dir, out RaycastHit hit, 100, wallLayer);

        Vector3 offsetDirection = -dir;
        float offsetDistance = 0.3f; // ������O�̋����i�K�v�ɉ����Ē����j

        return hit.point + offsetDirection * offsetDistance;

    }
}
