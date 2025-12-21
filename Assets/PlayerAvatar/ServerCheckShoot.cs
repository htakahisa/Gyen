using Mirror;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ServerCheckShoot : NetworkBehaviour
{

    public LayerMask hitMask;

    public float maxDistance = 100f;
    public float lineDuration = 0.05f;

    public LineRenderer lineRenderer;

    public static float headShot;

    public static float bodyShot;

    public GameObject Blood;
    public GameObject HeadBlood;
    public GameObject Fragment;
    [SyncVar]
    public bool isDarkness;

    public GameObject darkOrb;
    public float darkRadius = 2f;        // 上部からの半径（周囲の広がり）
    public LayerMask ground;   // 壁や障害物のLayerを指定

    public SyncList<GameObject> darkOrbPrefabs = new SyncList<GameObject>();
    Coroutine drawCoroutine;

    // Start is called before the first frame update
    void Awake()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }


    [Command(requiresAuthority = false)]
    public void CmdGetShoot(
      GameObject playerObject,
      Vector3 cameraPos,          // ← クライアントのカメラ位置
      Vector3 cameraForward,      // ← クライアントのカメラ forward（方向）
      Vector3 weaponPos,          // ← クライアント側の銃口位置
      int damage,
      int headDamage
  )
    {
        Debug.Log("shoot");

        ThirdPersonController tpc = playerObject.GetComponentInChildren<ThirdPersonController>();

        // ====== ここが最重要：サーバーの Transform を使わず、クライアントの送信値を使う ======
        Vector3 dir = cameraForward.normalized;
        Ray ray = new Ray(cameraPos, dir);

        Vector3 targetPoint;
        RaycastHit hit;

        // 命中点を取得
        if (Physics.Raycast(ray, out hit, 100, hitMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = cameraPos + dir * 100;
        }

        // ② 銃口位置 → 命中点への方向で弾を可視化（元の挙動100%維持）
        Vector3 shootDir = (targetPoint - weaponPos).normalized;
        DrawBulletLine(weaponPos, dir, playerObject);

        // 移動速度による射撃可否（元の挙動そのまま）
        if (tpc.GetSpeed() <= 1f && tpc.Grounded)
        {
            float originalDamage = damage;
            float originalHeadDamage = headDamage;
            float currentDamageRate = 1f;
            List<GameObject> hitList = new List<GameObject>();

            // RaycastAll による貫通処理（完全維持）
            RaycastHit[] results = Physics.RaycastAll(ray, 100f, hitMask);
            Array.Sort(results, (a, b) => a.distance.CompareTo(b.distance));

            for (int i = 0; i < results.Length; i++)
            {
                RaycastHit hitpoint = results[i];
                GameObject hitObject = hitpoint.collider.gameObject;
                int hitLayer = hitObject.layer;

                if (hitObject.GetComponentInParent<SpawnOwner>() != null && !hitObject.GetComponentInParent<SpawnOwner>().friendlyFire && playerObject.GetComponent<SpawnOwner>().ownerNetId == hitObject.GetComponentInParent<SpawnOwner>().ownerNetId) continue;

                Debug.Log($"HitOrder: [{i}] {hitpoint.collider.name} (Dist: {hitpoint.distance:F3}) Layer:{hitLayer}");

                // ■ 壁貫通の減衰（機能完全維持）
                HpMaster hpMaster = hitObject.GetComponentInParent<HpMaster>();
                PenetrationRate penetration = hitObject.GetComponentInParent<PenetrationRate>();

                float k;
                if (penetration != null)
                {
                    k = penetration.penetrationRate;
                }
                else
                {
                    k = 999f;
                }
                float thickness = GetWallThickness(hitpoint, dir, 100f);
                

                // ■ プレイヤーへのダメージ処理（完全維持）
                if (hitLayer == 6)
                {
                    if (hpMaster != null &&
                        playerObject.GetComponent<NetworkIdentity>().netId
                        != hitObject.GetComponentInParent<NetworkIdentity>().netId)
                    {
                        bool isHeadShot = hitpoint.collider.CompareTag("Head");

                        if (!hitList.Contains(hpMaster.gameObject))
                        {
                            // 血のエフェクト（完全維持）
                            if (isHeadShot)
                            {
                                AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.HEADBLOOD, transform.TransformPoint(GetComponentInParent<CharacterController>().center), 0.1f, 15);
                                GameObject bloodObj = Instantiate(HeadBlood, hitpoint.point, Quaternion.identity);
                                NetworkServer.Spawn(bloodObj);
                            }
                            else
                            {
                                AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.HITBLOOD, transform.TransformPoint(GetComponentInParent<CharacterController>().center), 0.1f, 15);
                                GameObject bloodObj = Instantiate(Blood, hitpoint.point, Quaternion.identity);
                                NetworkServer.Spawn(bloodObj);
                            }

                            int finalDamage = (int)((isHeadShot ? originalHeadDamage : originalDamage) * currentDamageRate);

                            bool headshot = false;
                            if (GetComponentInParent<BotManager>() == null)
                            {
                                headshot = isHeadShot;
                                if (isHeadShot) headShot++; else bodyShot++;
                            }

                            var targetTpc = hpMaster.GetComponentInChildren<ThirdPersonController>();
                            if (targetTpc != null)
                            {
                                targetTpc.ApplyPenalty();
                            }

                            hpMaster.TakeDamage(finalDamage, headshot);
                            hitList.Add(hpMaster.gameObject);

                            Debug.Log($"ヒット: {hitpoint.collider.tag}, ダメージ {finalDamage}");

                            if (isDarkness)
                            {
                                Darkness(hpMaster.transform, finalDamage, playerObject);
                            }
                        }
                    }
                }else
                if (hitLayer == 15)
                {
                    if (hpMaster != null &&
                        playerObject.GetComponent<NetworkIdentity>().netId
                        != hitObject.GetComponentInParent<NetworkIdentity>().netId)
                    {
                        bool isHeadShot = hitpoint.collider.CompareTag("Head");

                        if (!hitList.Contains(hpMaster.gameObject))
                        {
                            // 血のエフェクト（完全維持）
                            if (isHeadShot)
                            {
                                AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.HEADBLOOD, transform.TransformPoint(GetComponentInParent<CharacterController>().center), 0.1f, 15);
                            }
                            else
                            {
                                AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.HITBLOOD, transform.TransformPoint(GetComponentInParent<CharacterController>().center), 0.1f, 15);
                            }

                            int finalDamage = (int)((isHeadShot ? originalHeadDamage : originalDamage) * currentDamageRate);

                            bool headshot = false;
                            if (GetComponentInParent<BotManager>() == null)
                            {
                                headshot = isHeadShot;
                                if (isHeadShot) headShot++; else bodyShot++;
                            }

                            hpMaster.TakeDamage(finalDamage, headshot);
                            hitList.Add(hpMaster.gameObject);

                            Debug.Log($"ヒット: {hitpoint.collider.tag}, ダメージ {finalDamage}");
                        }
                    }
                }
                else
                {
                    if (k >= 999f)
                    {
                        GameObject fragment = Instantiate(Fragment, hitpoint.point, Quaternion.identity);
                        NetworkServer.Spawn(fragment);

                        currentDamageRate = 0f;
                        Debug.Log($"Layer {hitLayer} is impassable. Stopping bullet.");
                        break;
                    }
                    else if (thickness > 0f)
                    {
                        GameObject fragment = Instantiate(Fragment, hitpoint.point, Quaternion.identity);
                        NetworkServer.Spawn(fragment);

                        currentDamageRate *= Mathf.Exp(-k * thickness);
                        Debug.Log($"{hitObject.gameObject.name} thickness {thickness:F3}m, k={k:F3} => damageRate={currentDamageRate:F4}");
                    }
                }
            } // for
        } // if shootable
    }


    // --- 入口から同じ方向にわずかに inside して出口を探す関数 ---
    float GetWallThickness(RaycastHit entryHit, Vector3 dir, float maxDistance)
    {
        Collider col = entryHit.collider;
        dir = dir.normalized;
        const float eps = 0.001f;

        // 入口点のほんの内側から、同じ方向へレイを飛ばす（出口検出）
        Vector3 insideOrigin = entryHit.point + dir * eps;
        Ray insideRay = new Ray(insideOrigin, dir);

        // 残り射程（元のレイの距離を使う）
        float remaining = Mathf.Max(0f, maxDistance - entryHit.distance - eps);

        RaycastHit exitHit;
        if (col.Raycast(insideRay, out exitHit, remaining))
        {
            // exitHit.point は insideOrigin + dir * exitHit.distance
            float thickness = Vector3.Distance(entryHit.point, exitHit.point);
            return thickness;
        }

        // 失敗した場合は境界の大きさから近似（安全策）
        Vector3 absDir = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z));
        float approxThickness = Vector3.Dot(col.bounds.size, absDir); // bounds.size は全長
        return approxThickness;
    }

    public void Darkness(Transform target, int damage, GameObject playerObject)
    {
        // 対象のBounds（サイズ情報）を取得
        Renderer rend = target.GetComponentInChildren<SkinnedMeshRenderer>();
        if (rend == null) return;

        Bounds bounds = rend.bounds;

        float scale = 0.5f + (damage / 200);

        for (int i = 0; i < 1000; i++) // 最大1000回試行
        {
            // XZ平面でランダムな位置
            Vector2 randomCircle = Random.insideUnitCircle * darkRadius;

            Vector3 spawnPos = new Vector3(
                bounds.center.x + randomCircle.x,
                bounds.center.y + 3,
                bounds.center.z + randomCircle.y
            );

            // その位置に障害物がないか確認
            bool blocked = Physics.CheckSphere(spawnPos, scale / 2, ground);
            if (!blocked)
            {
                GameObject darkOrbPrefab = Instantiate(darkOrb, spawnPos, Quaternion.identity);                
                darkOrbPrefab.transform.localScale = new Vector3(scale, scale, scale);
                // 誰が生成依頼したかを記録
                var ownerTag = darkOrbPrefab.GetComponent<SpawnOwner>();
                // 依頼者のnetIdを記録
                ownerTag.ownerNetId = playerObject.GetComponent<NetworkIdentity>().netId;
                ownerTag.friendlyFire = true;
                NetworkServer.Spawn(darkOrbPrefab);
                darkOrbPrefab.GetComponent<Darkness>().SetSize(damage);
                darkOrbPrefabs.Add(darkOrbPrefab);
                RoundManager.spawns.Add(darkOrbPrefab);
                return; // 成功したら終了
            }
        }

        
    }

    public GameObject GetDarkOrb()
    {
        var orbs = FindObjectsOfType<Darkness>();

        if (orbs.Length == 0) return null;

        Darkness smallest = orbs[0];
        float smallestScale = orbs[0].transform.localScale.magnitude;

        foreach (var orb in orbs)
        {
            float orbScale = orb.transform.localScale.magnitude;
            if (orbScale < smallestScale)
            {
                smallest = orb;
                smallestScale = orbScale;
            }
        }

        return smallest.gameObject;
    }


    public void DestroyOrb(GameObject orb)
    {
        if (darkOrbPrefabs.Contains(orb))
        {
            darkOrbPrefabs.Remove(orb);
        }
        RoundManager.spawns.Remove(orb);
        NetworkServer.Destroy(orb);

    }

    public float GetHeadShotRate()
    {
        if(headShot + bodyShot == 0)
        {
            return 0f;
        }
        int a = (int)(headShot / (headShot + bodyShot) * 10000);

        return (float)a / 100;
    }


    [ClientRpc]
    public void DrawBulletLine(Vector3 origin, Vector3 direction, GameObject playerObject)
    {

        if (playerObject.GetComponent<NetworkIdentity>().isLocalPlayer) return; // 自分自身は無視

        Vector3 endPoint = origin + direction * maxDistance;
        Transform self = transform; // 自分の Transform。Awake 等でセットしておく。

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance, hitMask);

        // 距離順にソート（RaycastAll は順不同）
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            // 自分自身または子オブジェクトならスルー
            if (hit.transform == self || hit.transform.IsChildOf(self))
                continue;

            // ★ 自分以外のオブジェクトに当たった！
            endPoint = hit.point;
            break;
        }

        if(drawCoroutine != null)
        {
            StopCoroutine(drawCoroutine);
            drawCoroutine = null;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }
        drawCoroutine = StartCoroutine(ShowLine(origin, endPoint));
    }

    private IEnumerator ShowLine(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(lineDuration);

        drawCoroutine = null;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
        lineRenderer.enabled = false;
    }

}
public class RaycastHitDistanceComparer : IComparer<RaycastHit>
{
    public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
}
