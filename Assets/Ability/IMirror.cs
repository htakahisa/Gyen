using UnityEngine;
using Mirror;
using System.Collections;
using StarterAssets;

public class IMirror : NetworkBehaviour
{
    public float speed; // 移動速度
    public float radius; // 自分の半径
    public float fuseTime; //投擲から爆発までの時間
    public float delayTime; //爆発開始から中身が飛び散るまでの時間
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
        
        direction = transform.forward;
        Invoke("CmdFlash", fuseTime);
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
        GameObject instance = Instantiate(flashEffect, transform.position, transform.rotation);
        NetworkServer.Spawn(instance);
        isMoving = false;

        yield return new WaitForSeconds(delayTime);
        HideScreenWhenVisible.instance.AddTarget(gameObject, flashDuration);
        instance.GetComponent<DestroyTimer>().enabled = false;
        HideScreenWhenVisible.instance.RemoveTarget(gameObject, flashDuration);
        yield return new WaitForSeconds(0.1f);

        AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.IMIRRORDESTROYED, transform.position, 1, 30);
        NetworkServer.Destroy(gameObject);
    }

}
