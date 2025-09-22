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



    [Command]
    public void CmdGetShoot(GameObject playerObject, Vector3 position, Vector3 direction, int damage, int headDamage, Vector3 weaponPos)
    {
        Debug.Log("shoot");

        ThirdPersonController tpc = playerObject.GetComponentInChildren<ThirdPersonController>();

        RaycastHit[] results = new RaycastHit[10]; // 検出可能な最大数

        Ray ray = new Ray(position, direction);

        DrawBulletLine(weaponPos, direction, playerObject);

        if (tpc.GetSpeed() == 0 && tpc.Grounded)
        {
            
            int hitCount = Physics.RaycastNonAlloc(ray, results, 100, hitMask);
            // 距離順にソート（もし順序が狂っている場合の保険）
            Array.Sort(results, 0, hitCount, new RaycastHitDistanceComparer());

 
            float originalDamage = damage;
            float originalHeadDamage = headDamage;
            float currentDamageRate = 1f; // 毎回リセット
            List<GameObject> hitList = new List<GameObject>();

            for (int i = 0; i < hitCount; i++)
            {
                Debug.Log($"HitOrder: [{i}] {results[i].collider.name} (Dist: {results[i].distance}m)");
                GameObject hit = results[i].collider.gameObject;
                RaycastHit hitpoint = results[i];

                // フェーズウォールチェック（先に減衰率を計算）
                if (hit.layer == 9)
                {
                    GameObject fragment = Instantiate(Fragment, hitpoint.point, Quaternion.identity);
                    NetworkServer.Spawn(fragment);
                    currentDamageRate *= 0f;
                    Debug.Log($"地面通過: 減衰率 {currentDamageRate}");
                    continue; // 地面自体にはダメージを与えない
                }


                // スモークチェック（先に減衰率を計算）
                if (hit.layer == 10)
                {
                    currentDamageRate *= 0.9f;
                    Debug.Log($"地面通過: 減衰率 {currentDamageRate}");
                    continue; // 地面自体にはダメージを与えない
                }


                // 地面チェック（先に減衰率を計算）
                if (hit.layer == 3)
                {
                    GameObject fragment = Instantiate(Fragment, hitpoint.point, Quaternion.identity);
                    NetworkServer.Spawn(fragment);
                    currentDamageRate *= 0.5f;
                    Debug.Log($"地面通過: 減衰率 {currentDamageRate}");
                    continue; // 地面自体にはダメージを与えない
                }



                // 敵へのダメージ処理
                if (hit.layer == 6)
                {
                    HpMaster hpMaster = hit.GetComponentInParent<HpMaster>();
                    if (hpMaster != null && playerObject.GetComponent<NetworkIdentity>().netId != hit.GetComponentInParent<NetworkIdentity>().netId)
                    {
                        
                        GameObject blood = Instantiate(Blood, hitpoint.point, Quaternion.identity);
                        NetworkServer.Spawn(blood);
                        if (!hitList.Contains(hpMaster.gameObject))
                        {
                            playerObject.GetComponent<AudioManager>().CmdPlaySoundAtPoint(AudioManager.Sounds.HITBLOOD, transform.TransformPoint(GetComponentInParent<CharacterController>().center), 0.1f);
                            int finalDamage = (int)((hit.tag == "Head" ? originalHeadDamage : originalDamage) * currentDamageRate);

                            if(hit.tag == "Head")
                            {
                                headShot++;
                            }
                            else
                            {
                                bodyShot++;
                            }

                            hpMaster.TakeDamage(finalDamage);
                            hitList.Add(hpMaster.gameObject);
                            Debug.Log($"ヒット: {hit.tag}, ダメージ {finalDamage}");

                            if (isDarkness)
                            {
                                Darkness(hpMaster.transform, finalDamage, playerObject);                              
                            }

                        }
                    }
                }
          
            }
        }
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
                darkOrbPrefab.GetComponent<DarknessSize>().SetSize(damage);
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
