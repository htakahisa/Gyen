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

    private LineRenderer lineRenderer;

    public static float headShot;

    public static float bodyShot;

    public GameObject Blood;
    public GameObject Fragment;
    [SyncVar]
    public bool isDarkness;

    public GameObject darkOrb;
    public float darkRadius = 2f;        // 上部からの半径（周囲の広がり）
    public LayerMask ground;   // 壁や障害物のLayerを指定

    public SyncList<GameObject> darkOrbPrefabs = new SyncList<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }



    // レイヤーごとの減衰率設定（1mあたりの減衰率）
    private Dictionary<int, float> layerAttenuation = new Dictionary<int, float>()
{
    { 3, 0.5f },  // Ground: 1mごとに exp(-0.2*thickness) 減衰
    { 9, 999f },  // PhaseWall: 通過不可（ほぼ即死）
    { 10, 0.1f }, // Smoke: ほぼ影響なし（厚さ依存で微減衰）
};

    [Command]
    public void CmdGetShoot(GameObject playerObject, Vector3 position, Vector3 direction, int damage, int headDamage, Vector3 weaponPos)
    {
        Debug.Log("shoot");

        ThirdPersonController tpc = playerObject.GetComponentInChildren<ThirdPersonController>();
        Vector3 dir = direction.normalized; // ← 正規化する
        Ray ray = new Ray(position, dir);
        DrawBulletLine(weaponPos, direction, playerObject);

        if (tpc.GetSpeed() == 0 && tpc.Grounded)
        {
            float originalDamage = damage;
            float originalHeadDamage = headDamage;
            float currentDamageRate = 1f;
            List<GameObject> hitList = new List<GameObject>();

            // --- RaycastAll のみを使う（RaycastNonAlloc は使わない） ---
            RaycastHit[] results = Physics.RaycastAll(ray, 100f, hitMask);
            Array.Sort(results, (a, b) => a.distance.CompareTo(b.distance));

            for (int i = 0; i < results.Length; i++)
            {
                RaycastHit hitpoint = results[i];
                GameObject hit = hitpoint.collider.gameObject;
                int hitLayer = hit.layer;

                Debug.Log($"HitOrder: [{i}] {hitpoint.collider.name} (Dist: {hitpoint.distance:F3}) Layer:{hitLayer}");

                // ★「レイヤーごとの減衰」扱いがあるならここで処理
                if (layerAttenuation != null && layerAttenuation.ContainsKey(hitLayer))
                {
                    float k = layerAttenuation[hitLayer]; // 1mあたりの減衰係数と仮定

                    // 入力: entryHit, 発射方向 dir, 射程 100f
                    float thickness = GetWallThickness(hitpoint, dir, 100f);

                    if (k >= 999f) // 貫通不可フラグの扱い（例）
                    {
                        // 元コードでやっていた Fragment 生成を再現
                        GameObject fragment = Instantiate(Fragment, hitpoint.point, Quaternion.identity);
                        NetworkServer.Spawn(fragment);

                        currentDamageRate = 0f;
                        Debug.Log($"Layer {hitLayer} is impassable. Stopping bullet.");
                        break;
                    }
                    else if (thickness > 0f)
                    {
                        // 元コードでやっていた Fragment 生成を再現
                        GameObject fragment = Instantiate(Fragment, hitpoint.point, Quaternion.identity);
                        NetworkServer.Spawn(fragment);
                        currentDamageRate *= Mathf.Exp(-k * thickness);
                        Debug.Log($"{hit.gameObject.name} thickness {thickness:F3}m, k={k:F3} => damageRate={currentDamageRate:F4}");
                    }

                    continue; // 壁自体にはダメージを与えない
                }

                // ★ 敵へのダメージ処理（既存のロジックを維持）
                if (hitLayer == 6)
                {
                    HpMaster hpMaster = hit.GetComponentInParent<HpMaster>();
                    if (hpMaster != null && playerObject.GetComponent<NetworkIdentity>().netId != hit.GetComponentInParent<NetworkIdentity>().netId)
                    {
                        GameObject blood = Instantiate(Blood, hitpoint.point, Quaternion.identity);
                        NetworkServer.Spawn(blood);
                        if (!hitList.Contains(hpMaster.gameObject))
                        {
                            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.HITBLOOD, transform.TransformPoint(GetComponentInParent<CharacterController>().center), 0.1f);
                            int finalDamage = (int)((hitpoint.collider.tag == "Head" ? originalHeadDamage : originalDamage) * currentDamageRate);

                            if (hitpoint.collider.tag == "Head")
                            {
                                headShot++;
                            }
                            else
                            {
                                bodyShot++;
                            }

                            hpMaster.TakeDamage(finalDamage);
                            hitList.Add(hpMaster.gameObject);
                            Debug.Log($"ヒット: {hitpoint.collider.tag}, ダメージ {finalDamage}");

                            if (isDarkness)
                            {
                                Darkness(hpMaster.transform, finalDamage, playerObject);
                            }
                        }
                    }
                }
            } // for results
        } // if can shoot
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
                var ownerTag = darkOrbPrefab.AddComponent<SpawnOwner>();
                // 依頼者のnetIdを記録
                ownerTag.ownerNetId = playerObject.GetComponent<NetworkIdentity>().netId; 
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
        if (darkOrbPrefabs.Count != 0)
        {
            GameObject smallest = darkOrbPrefabs[0];
            float smallestScale = darkOrbPrefabs[0].transform.localScale.magnitude;

            foreach (var orb in darkOrbPrefabs)
            {
                float orbScale = orb.transform.localScale.magnitude;
                if (orbScale < smallestScale)
                {
                    smallest = orb;
                    smallestScale = orbScale;
                }
            }

            return smallest;
        }
        else
        {
            return null;
        }
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
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, hitMask))
        {
            endPoint = hit.point;
        }

        StartCoroutine(ShowLine(origin, endPoint));
    }

    private System.Collections.IEnumerator ShowLine(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(lineDuration);

        lineRenderer.enabled = false;
    }

}
public class RaycastHitDistanceComparer : IComparer<RaycastHit>
{
    public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
}
