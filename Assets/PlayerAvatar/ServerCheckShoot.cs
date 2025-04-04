using Mirror;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCheckShoot : NetworkBehaviour
{

    public LayerMask hitMask;

    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }



    [Command]
    public void CmdGetShoot(GameObject playerObject, Vector3 position, Vector3 direction, int damage, int headDamage)
    {
        Debug.Log("shoot");

        ThirdPersonController tpc = playerObject.GetComponent<ThirdPersonController>();

        RaycastHit[] results = new RaycastHit[10]; // ���o�\�ȍő吔

        Ray ray = new Ray(position, direction);
        if (tpc.GetSpeed() == 0f)
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

}
public class RaycastHitDistanceComparer : IComparer<RaycastHit>
{
    public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
}
