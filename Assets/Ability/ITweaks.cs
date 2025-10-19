using UnityEngine;
using Mirror;

public class Tweaks : NetworkBehaviour
{
    [Header("Laser Settings")]
    public float attackRange = 10f;          // 攻撃距離
    public int damage = 10;              // ダメージ量
    public float fireInterval = 3.0f;       // 発射間隔（秒）
    public LineRenderer lineRenderer;       // レーザー用
    public Transform firePoint;             // 発射位置
    private int bodyLayer;

    private float nextFireTime = 0f;

    private void Start()
    {
        bodyLayer = LayerMask.NameToLayer("Body");
        
    }


    void Update()
    {


        if (!isServer) return; // 攻撃判定はサーバーで行う

        // Bodyレイヤーを範囲内から探す
        Collider[] targets = Physics.OverlapSphere(transform.position, attackRange, 1 << bodyLayer);
        if (targets.Length > 0)
        {
            GameObject target = GetNearestTarget(targets);
            if (target != null && Time.time >= nextFireTime)
            {
                Vector3 dir = (target.transform.position - firePoint.position).normalized;

                if (Physics.Raycast(firePoint.position, dir, out RaycastHit hit, attackRange))
                {
                    if (hit.collider.CompareTag("Body") || hit.collider.CompareTag("Head"))
                    {
                        // 射線が通っているなら発射
                        nextFireTime = Time.time + fireInterval;
                        RpcFireLaser(firePoint.position, hit.point);

                        // ダメージ処理（サーバー側で実行）
                        HpMaster hp = hit.collider.GetComponentInParent<HpMaster>();
                        if (hp != null)
                        {
                            hp.TakeDamage(damage, false);
                            hp.TakeStun(0.1f, 1);
                        }
                    }
                }
            }
        }
    }

    // 一番近いプレイヤーを探す
    GameObject GetNearestTarget(Collider[] targets)
    {
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var t in targets)
        {
            // 自分自身ならスキップ
            var netIdentity = t.GetComponentInParent<NetworkIdentity>();
            if (netIdentity != null && CheckAuthority(netIdentity, GetComponent<NetworkIdentity>()))
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = t.gameObject;
            }
        }

        return nearest;
    }

    public bool CheckAuthority(NetworkIdentity objA, NetworkIdentity objB)
    {
        var ownerA = objA.GetComponent<NetworkIdentity>().connectionToClient;
        var ownerB = objB.GetComponent<NetworkIdentity>().connectionToClient;

        return (ownerA == ownerB);
    }

    // クライアントにレーザー描画を同期
    [ClientRpc]
    void RpcFireLaser(Vector3 start, Vector3 end)
    {
        StartCoroutine(ShowLaser(start, end));
    }

    System.Collections.IEnumerator ShowLaser(Vector3 start, Vector3 end)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            yield return new WaitForSeconds(0.1f);
            lineRenderer.enabled = false;
        }
    }
}
