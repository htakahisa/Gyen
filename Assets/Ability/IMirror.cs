using UnityEngine;
using Mirror;
using System.Collections;

public class IMirror : NetworkBehaviour
{
    public float speed = 10f; // 移動速度
    public float radius = 1f; // 自分の半径
    private Vector3 direction; // 移動方向
    private LayerMask ground;
    private bool isMoving = true;
    public GameObject flashEffect;
    public float flashDuration;

    void Start()
    {
        ground = LayerMask.GetMask("Ground");
        // カメラの方向に向けて初期ベクトルを決定
        Transform target = Camera.main.transform;
        direction = target.forward;
        Invoke("CmdFlash", 1f);
    }

    void Update()
    {
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

    [Command]
    public void CmdFlash()
    {
        StartCoroutine(FlashCoroutine());
    }

    public IEnumerator FlashCoroutine()
    {
        HideScreenWhenVisible.instance.AddTarget(GetComponent<Renderer>(), flashDuration);
        GameObject instance = Instantiate(flashEffect, transform.position, transform.rotation);
        NetworkServer.Spawn(instance);

        yield return new WaitForSeconds(0.2f);
        instance.GetComponent<DestroyTimer>().enabled = false;
        isMoving = false;
        yield return new WaitForSeconds(0.3f);

        AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.IMIRRORDESTROYED, transform.position, 1);
        NetworkServer.Destroy(gameObject);
    }

}
