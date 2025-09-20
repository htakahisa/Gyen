using Mirror;
using StarterAssets;
using System.Collections;
using UnityEngine;

public class Ejah : CharacterSkills
{
    public AbilityController abilityController;
    public CharacterSkills currentCharacter;
    public ShootManager shootManager;
    public HpMaster hpMaster;


    [Tooltip("最大到達距離 (m)")]
    public float maxDistance = 5f;

    [Tooltip("壁に当たったとき内側に押し戻すオフセット")]
    public float terraSurfaceOffset = 0.5f;

    [Tooltip("地面に置くとき上に浮かせるオフセット")]
    public float terraUpOffset = 0.5f;

    [Tooltip("壁に当たったとき内側に押し戻すオフセット")]
    public float horusSurfaceOffset = 0.5f;

    [Tooltip("地面に置くとき上に浮かせるオフセット")]
    public float horusUpOffset = 0.5f;

    public enum RotationMode { Identity, AlignWithSurfaceNormal, FaceViewDirection }
    [Tooltip("生成時の回転方法")]
    public RotationMode rotationMode = RotationMode.Identity;

    public GameObject terraPre;
    public GameObject terra;

    public Transform terraPreInstance;

    public LayerMask ground;

    public bool isTerra;

    public GameObject dustPuff;

    public GameObject horus;
    public GameObject horusPre;

    public bool isHorus;

    public Transform horusPreInstance;


    [TextArea]
    public string memo = "Skill1 = Lime, Skill2 = Yellow, Skill3 = Singing";

    private void Awake()
    {
        Skill1 = Terra;
        Skill2 = Horus;
        Skill3 = Horus;
    }

    private void Update()
    {
        if (isTerra)
        {

         
            
            Transform t = Camera.main.transform;

            if (terraPre == null)
            {
                Debug.LogWarning("prefabToSpawn が設定されていません。");
                return;
            }

            Vector3 origin = t.position;
            Vector3 direction = t.forward;

            RaycastHit hit;
            Vector3 spawnPos;
            Quaternion spawnRot = Quaternion.identity;

            // ---- 1. 前方レイキャスト（壁チェック） ----
            if (Physics.Raycast(origin, direction, out hit, maxDistance, ground, QueryTriggerInteraction.Ignore))
            {
                spawnPos = hit.point + hit.normal * terraSurfaceOffset;
                spawnRot = GetRotation(t, hit.normal, direction);
            }
            else
            {
                spawnPos = origin + direction.normalized * maxDistance;
                spawnRot = GetRotation(t, Vector3.up, direction);
            }

            // ---- 2. 下方向に地面チェック ----
            RaycastHit groundHit;
            if (Physics.Raycast(spawnPos + Vector3.up * 1f, Vector3.down, out groundHit, 2f, ground, QueryTriggerInteraction.Ignore))
            {
                // 地面が近くにあったら、めり込み防止で地面の上に配置
                spawnPos = groundHit.point + Vector3.up * terraUpOffset;
            }

            TerraPre(spawnPos, spawnRot);
            if (Input.GetMouseButtonDown(0))
            {
                currentCharacter.skill1Energy--;
                spawnPos = terraPreInstance.position;
                CmdTerra(spawnPos);
                

            }
        }
        else
        {
            DestroyTerraPre();          
        }

        if (isHorus)
        {



            Transform t = Camera.main.transform;

            if (horusPre == null)
            {
                Debug.LogWarning("prefabToSpawn が設定されていません。");
                return;
            }
            Vector3 origin = t.position;
            Vector3 direction = t.forward;

            RaycastHit hit;
            Vector3 spawnPos;
            Quaternion spawnRot = Quaternion.identity;

            // ---- 1. 前方レイキャスト（壁チェック） ----
            if (Physics.Raycast(origin, direction, out hit, maxDistance, ground, QueryTriggerInteraction.Ignore))
            {
                spawnPos = hit.point + hit.normal * horusSurfaceOffset;
                spawnRot = GetRotation(t, hit.normal, direction);
            }
            else
            {
                spawnPos = origin + direction.normalized * maxDistance;
                spawnRot = GetRotation(t, Vector3.up, direction);
            }

            // ---- 2. 下方向に地面チェック ----
            RaycastHit groundHit;
            if (Physics.Raycast(spawnPos + Vector3.up * 1f, Vector3.down, out groundHit, 2f, ground, QueryTriggerInteraction.Ignore))
            {
                // 地面が近くにあったら、めり込み防止で地面の上に配置
                spawnPos = groundHit.point + Vector3.up * horusUpOffset;
            }

            HorusPre(spawnPos, spawnRot);

            if (Input.GetMouseButtonDown(0))
            {
                currentCharacter.skill2Energy--;
                CmdHorus(spawnPos);


            }
        }
        else
        {
            DestroyHorusPre();
        }
    }

    private Quaternion GetRotation(Transform t, Vector3 normal, Vector3 forward)
    {
        switch (rotationMode)
        {
            case RotationMode.AlignWithSurfaceNormal:
                return Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, normal), normal);
            case RotationMode.FaceViewDirection:
                return Quaternion.LookRotation(forward, Vector3.up);
            case RotationMode.Identity:
            default:
                return Quaternion.identity;
        }
    }

    public void Terra()
    {

        if (currentCharacter.skill1Energy <= 0)
        {
            return;
        }      
        
        ChangeTerra();

        

    }








    [Command]
    public void CmdTerra(Vector3 spawnPos, NetworkConnectionToClient conn = null)
    {


        StartCoroutine(TerraSpawn(spawnPos, conn));

    }

    public IEnumerator TerraSpawn(Vector3 spawnPos, NetworkConnectionToClient conn = null)
    {
        

        GameObject dustPre = Instantiate(dustPuff, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(dustPre);
        RoundManager.spawns.Add(dustPre);
        if (terraPreInstance != null)
        {
            terraPreInstance.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);
        NetworkServer.Destroy(dustPre);

        GameObject terraPre = Instantiate(terra, spawnPos, Quaternion.identity);
        // 誰が生成依頼したかを記録
        var ownerTag = terraPre.AddComponent<SpawnOwner>();
        ownerTag.ownerNetId = conn.identity.netId; // 依頼者のnetIdを記録
        NetworkServer.Spawn(terraPre);
        RoundManager.spawns.Add(terraPre);

        RpcChangeTerra(connectionToClient);
    }

    [TargetRpc]
    public void RpcChangeTerra(NetworkConnection target)
    {
        ChangeTerra();
    }


    public void TerraPre(Vector3 pos, Quaternion dir)
    {
        if (terraPreInstance == null)
        {
            terraPreInstance = Instantiate(terraPre, pos, dir).transform;
        }
        else
        {
            terraPreInstance.transform.position = pos;
            terraPreInstance.transform.rotation = dir;
        }
    }

    public void DestroyTerraPre()
    {
        if (terraPreInstance != null)
        {
            Destroy(terraPreInstance.gameObject);
        }
        
    }

    public void ChangeTerra()
    {

        if (!isTerra)
        {

            shootManager.canShoot = false;
            isTerra = true;
            isHorus = false;

        }
        else
        {
            shootManager.canShoot = true;
            isTerra = false;
            shootManager.shootInputAhead = true;
        }

    }

    public void Horus()
    {

        if (currentCharacter.skill2Energy <= 0)
        {
            return;
        }

        ChangeHorus();



    }

    [Command]
    public void CmdHorus(Vector3 spawnPos, NetworkConnectionToClient conn = null)
    {


        HorusSpawn(spawnPos, conn);

    }

    public void ChangeHorus()
    {
        if (!isHorus)
        {

            shootManager.canShoot = false;
            isHorus = true;
            isTerra = false;

        }
        else
        {
            shootManager.canShoot = true;
            isHorus = false;
            shootManager.shootInputAhead = true;
        }
    }

    public void HorusPre(Vector3 pos, Quaternion dir)
    {
        if (horusPreInstance == null)
        {
            horusPreInstance = Instantiate(horusPre, pos, dir).transform;
        }
        else
        {
            horusPreInstance.transform.position = pos;
            horusPreInstance.transform.rotation = dir;
        }
    }

    public void HorusSpawn(Vector3 spawnPos, NetworkConnectionToClient conn = null) 
    {

        
        GameObject horusPrefab = Instantiate(horus, spawnPos, Quaternion.identity);

        // 誰が生成依頼したかを記録
        var ownerTag = horusPrefab.AddComponent<SpawnOwner>();
        ownerTag.ownerNetId = conn.identity.netId; // 依頼者のnetIdを記録

        NetworkServer.Spawn(horusPrefab, conn);
        RoundManager.spawns.Add(horusPrefab);

        RpcChangeHorus(connectionToClient);
    }

    [TargetRpc]
    public void RpcChangeHorus(NetworkConnection target)
    {
        ChangeHorus();
    }

    public void DestroyHorusPre()
    {
        if (horusPreInstance != null)
        {
            Destroy(horusPreInstance.gameObject);
        }

    }


}





