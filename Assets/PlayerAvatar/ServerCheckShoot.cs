using Mirror;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCheckShoot : NetworkBehaviour
{

    public LayerMask hitMask;

    public float maxDistance = 100f;
    public float lineDuration = 0.05f;

    private LineRenderer lineRenderer;

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

        ThirdPersonController tpc = playerObject.GetComponent<ThirdPersonController>();

        RaycastHit[] results = new RaycastHit[10]; // ���o�\�ȍő吔

        Ray ray = new Ray(position, direction);

        DrawBulletLine(weaponPos, direction, playerObject);


        if (tpc.GetSpeed() == 0f && tpc.Grounded)
        {
            int hitCount = Physics.RaycastNonAlloc(ray, results, 100, hitMask);
            // �������Ƀ\�[�g�i���������������Ă���ꍇ�̕ی��j
            Array.Sort(results, 0, hitCount, new RaycastHitDistanceComparer());

 
            float originalDamage = damage;
            float originalHeadDamage = headDamage;
            float currentDamageRate = 1f; // ���񃊃Z�b�g
            List<GameObject> hitList = new List<GameObject>();

            for (int i = 0; i < hitCount; i++)
            {
                Debug.Log($"HitOrder: [{i}] {results[i].collider.name} (Dist: {results[i].distance}m)");
                GameObject hit = results[i].collider.gameObject;


                // �n�ʃ`�F�b�N�i��Ɍ��������v�Z�j
                if (hit.layer == 9)
                {
                    currentDamageRate *= 0f;
                    Debug.Log($"�n�ʒʉ�: ������ {currentDamageRate}");
                    continue; // �n�ʎ��̂ɂ̓_���[�W��^���Ȃ�
                }


                // �n�ʃ`�F�b�N�i��Ɍ��������v�Z�j
                if (hit.layer == 10)
                {
                    currentDamageRate *= 0.9f;
                    Debug.Log($"�n�ʒʉ�: ������ {currentDamageRate}");
                    continue; // �n�ʎ��̂ɂ̓_���[�W��^���Ȃ�
                }


                // �n�ʃ`�F�b�N�i��Ɍ��������v�Z�j
                if (hit.layer == 3)
                {
                    currentDamageRate *= 0.5f;
                    Debug.Log($"�n�ʒʉ�: ������ {currentDamageRate}");
                    continue; // �n�ʎ��̂ɂ̓_���[�W��^���Ȃ�
                }



                // �G�ւ̃_���[�W����
                if (hit.layer == 6)
                {
                    HpMaster hpMaster = hit.GetComponentInParent<HpMaster>();
                    if (hpMaster != null && playerObject.GetComponent<NetworkIdentity>().netId != hit.GetComponentInParent<NetworkIdentity>().netId)
                    {
                        if (!hitList.Contains(hpMaster.gameObject))
                        {
                            int finalDamage = (int)((hit.tag == "Head" ? originalHeadDamage : originalDamage) * currentDamageRate);
                            hpMaster.TakeDamage(finalDamage);
                            hitList.Add(hpMaster.gameObject);
                            Debug.Log($"�q�b�g: {hit.tag}, �_���[�W {finalDamage}");
                        }
                    }
                }
          
            }
        }
    }

    [ClientRpc]
    public void DrawBulletLine(Vector3 origin, Vector3 direction, GameObject playerObject)
    {

        if (playerObject.GetComponent<NetworkIdentity>().isLocalPlayer) return; // �������g�͖���

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
