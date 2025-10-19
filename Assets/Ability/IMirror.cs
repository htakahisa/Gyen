using UnityEngine;
using Mirror;
using System.Collections;

public class IMirror : NetworkBehaviour
{
    public float speed = 10f; // 移動速度
    public float radius = 0.5f; // 自分の半径
    private Vector3 direction; // 移動方向
    private LayerMask ground;
    [SyncVar]
    private bool isMoving = true;
    public GameObject flashEffect;
    public float flashDuration;

    void Start()
    {

        if (!isServer) return;
        ground = LayerMask.GetMask("Ground");
        // サーバーが動かすので、サーバー

        Transform target = null;

        if (GetComponent<SpawnOwner>().IsMine())
        {
            target = RoundManager.rm.GetMyPlayer().GetComponentInChildren<Camera>().transform;
        }
        else
        {
            target = RoundManager.rm.GetOtherPlayer().GetComponentInChildren<Camera>().transform;
        }
        direction = target.forward;
        Invoke("CmdFlash", 1f);
    }

    void Update()
    {
        if (!isServer) return;
        if (!isMoving) return;

        float moveDistance = speed * Time.deltaTime;

        if (Physics.SphereCast(transform.position, radius, direction, out RaycastHit hit, moveDistance + radius, ground))
        {
            // 衝突までの距離だけ移動
            transform.position += direction * hit.distance;

            // 法線で反射
            direction = Vector3.Reflect(direction, hit.normal);
        }
        else
        {
            transform.position += direction * moveDistance;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdFlash()
    {
        StartCoroutine(FlashCoroutine());
    }

    public IEnumerator FlashCoroutine()
    {
        HideScreenWhenVisible.instance.AddTarget(gameObject, flashDuration);
        GameObject instance = Instantiate(flashEffect, transform.position, transform.rotation);
        NetworkServer.Spawn(instance);
        isMoving = false;

        yield return new WaitForSeconds(0.3f);
        instance.GetComponent<DestroyTimer>().enabled = false;
        yield return new WaitForSeconds(0.2f);

        AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.IMIRRORDESTROYED, transform.position, 1, 30);
        NetworkServer.Destroy(gameObject);
    }

}
