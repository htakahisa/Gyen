using UnityEngine;
using System.Collections;
using Mirror;   // Mirrorを使う場合追加

public class AutoLaserTurret : NetworkBehaviour
{
    [Header("レーザー設定")]
    public float detectionRange = 10f;    // 検知範囲
    public int damage = 10;           // ダメージ量
    public float laserDuration = 0.2f;   // 光線表示時間
    public float fireInterval = 1f;      // 発射間隔

    [Header("参照")]
    public Transform firePoint;          // 発射位置

    private LineRenderer lineRenderer;
    private int bodyLayer;
    private float lastFireTime;

    private Coroutine laserCoroutine;


    void Start()
    {
        bodyLayer = LayerMask.NameToLayer("Body");

        // LineRenderer初期化
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = Color.red;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (!isServer) return;
        if (Time.time - lastFireTime < fireInterval) return;

        // Bodyレイヤーを範囲内から探す
        Collider[] targets = Physics.OverlapSphere(transform.position, detectionRange, 1 << bodyLayer);
        if (targets.Length > 0)
        {
            Transform nearest = GetNearestTarget(targets);

            if (nearest != null)
            {


                // ターゲット方向にRayを飛ばす
                Vector3 dir = (nearest.position - firePoint.position).normalized;
                if (Physics.Raycast(firePoint.position, dir, out RaycastHit hit, detectionRange))
                {
                    if (hit.collider.tag == "Body" || hit.collider.tag == "Head")
                    {

                        if (laserCoroutine == null)
                        {
                            lastFireTime = Time.time;
                            RpcShowLaser(hit.point);
                            var networkIdentity = hit.collider.GetComponentInParent<NetworkIdentity>();
                            ServerFireLaser(networkIdentity);
                        }
                    }


                }
            }
        }
    }



    [Server]
    public void ServerFireLaser(NetworkIdentity networkIdentity)
    {
        // ダメージ処理

        var hp = networkIdentity.GetComponentInParent<HpMaster>();

        if (hp != null)
        {
            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.LASER, transform.position, 1, 30);
            hp.TakeDamage(damage, false);
        }

        
    }

    Transform GetNearestTarget(Collider[] targets)
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var t in targets)
        {
            // 自分自身ならスキップ
            var netIdentity = t.GetComponentInParent<NetworkIdentity>();

            //NetworkIdentityがあることを確認、対象のオブジェクトのオーナーが自分だった場合かつ、これがプラクティスで相手がボットであるというわけでもない場合にそれを無効のターゲットとしてやり直す
            if (netIdentity != null && (CheckAuthority(netIdentity, GetComponent<NetworkIdentity>()) && !(RoundManager.rm.currentMode == RoundManager.Mode.PRACTICE && netIdentity.GetComponent<BotManager>() != null)))
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = t.transform;
            }
        }

        return nearest;
    }

    public bool CheckAuthority(NetworkIdentity objA, NetworkIdentity objB)
    {
        var ownerA = objA.GetComponent<NetworkIdentity>().connectionToClient;
        var ownerB = objB.GetComponent<NetworkIdentity>().connectionToClient;

        return(ownerA == ownerB);
    }

    [ClientRpc]
    private void RpcShowLaser(Vector3 hitPoint)
    {
        laserCoroutine = StartCoroutine(ShowLaser(hitPoint));
    }

   
    private IEnumerator ShowLaser(Vector3 hitPoint)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, hitPoint);

        yield return new WaitForSeconds(laserDuration);

        lineRenderer.enabled = false;
        laserCoroutine = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
