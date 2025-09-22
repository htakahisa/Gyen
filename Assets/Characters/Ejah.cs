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
    public enum RotationMode { Identity, AlignWithSurfaceNormal, FaceViewDirection }
    [Tooltip("生成時の回転方法")]
    public RotationMode rotationMode = RotationMode.Identity;

    public LayerMask ground;

    [Tooltip("最大到達距離 (m)")]
    public float terraMaxDistance = 5f;

    [Tooltip("壁に当たったとき内側に押し戻すオフセット")]
    public float terraSurfaceOffset = 0.5f;

    [Tooltip("地面に置くとき上に浮かせるオフセット")]
    public float terraUpOffset = 0.5f;


    public GameObject terraPre;
    public GameObject terra;

    public Transform terraPreInstance;

    public bool isTerra;

    [Tooltip("最大到達距離 (m)")]
    public float horusMaxDistance = 3f;

    [Tooltip("壁に当たったとき内側に押し戻すオフセット")]
    public float horusSurfaceOffset = 0.5f;

    [Tooltip("地面に置くとき上に浮かせるオフセット")]
    public float horusUpOffset = 0.5f;

    public GameObject dustPuff;

    public GameObject horus;
    public GameObject horusPre;

    public bool isHorus;

    public Transform horusPreInstance;

    [Tooltip("最大到達距離 (m)")]
    public float mentumMaxDistance = 5f;

    [Tooltip("壁に当たったとき内側に押し戻すオフセット")]
    public float mentumSurfaceOffset = 0.5f;

    [Tooltip("地面に置くとき上に浮かせるオフセット")]
    public float mentumUpOffset = 0.5f;

    public GameObject mentumPre;
    public GameObject mentum;

    public Transform mentumPreInstance;

    public bool isMentum;


    [TextArea]
    public string memo = "Skill1 = Terra, Skill2 = Horus, Skill3 = Singing";

    private void Awake()
    {
        Skill1 = Terra;
        Skill2 = Horus;
        Skill3 = Mentum;
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

            Transform cam = Camera.main.transform;
            Vector3 origin = cam.position;
            Vector3 direction = cam.forward;

            RaycastHit hit;
            Vector3 spawnPos;
            Quaternion spawnRot = Quaternion.identity;

            if (Physics.Raycast(origin, direction, out hit, terraMaxDistance, ground, QueryTriggerInteraction.Ignore))
            {
                spawnPos = hit.point + hit.normal * terraSurfaceOffset;
                spawnRot = GetRotation(cam, hit.normal);
            }
            else
            {
                spawnPos = origin + direction.normalized * terraMaxDistance;
                spawnRot = GetRotation(cam, Vector3.up);
            }

            // 下方向に地面チェック
            RaycastHit groundHit;
            if (Physics.Raycast(spawnPos + Vector3.up * 1f, Vector3.down, out groundHit, 2f, ground, QueryTriggerInteraction.Ignore))
            {
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
            if (Physics.Raycast(origin, direction, out hit, horusMaxDistance, ground, QueryTriggerInteraction.Ignore))
            {
                spawnPos = hit.point + hit.normal * horusSurfaceOffset;
                spawnRot = GetProjectedRotation(direction, hit.normal);
            }
            else
            {
                spawnPos = origin + direction.normalized * horusMaxDistance;
                spawnRot = GetProjectedRotation(direction, Vector3.up);
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
                CmdHorus(spawnPos, spawnRot);


            }
        }
        else
        {
            DestroyHorusPre();
        }
        if (isMentum)
        {

            Transform cam = Camera.main.transform;
            Transform player = transform;
            Vector3 origin = cam.position;
            Vector3 direction = cam.forward;

            RaycastHit hit;
            Vector3 spawnPos;
            Quaternion spawnRot;

            // レイがヒットした場合
            if (Physics.Raycast(origin, direction, out hit, mentumMaxDistance, ground, QueryTriggerInteraction.Ignore))
            {
                spawnPos = hit.point + Vector3.up * mentumSurfaceOffset;
            }
            else
            {
                spawnPos = origin + direction.normalized * mentumMaxDistance;
            }

            // 向きはカメラの方向そのまま
            spawnRot = Quaternion.LookRotation(player.forward, Vector3.up);

            // ---- 2. 下方向に地面チェック ----
            RaycastHit groundHit;
            if (Physics.Raycast(spawnPos + Vector3.up * 1f, Vector3.down, out groundHit, 2f, ground, QueryTriggerInteraction.Ignore))
            {
                spawnPos = groundHit.point + Vector3.up * mentumUpOffset;
            }

            MentumPre(spawnPos, spawnRot);

            if (Input.GetMouseButtonDown(0))
            {
                currentCharacter.skill3Energy--;
                CmdMentum(spawnPos, spawnRot);


            }

        }
        else
        {
            DestroyMentumPre();
        }
    }

    Quaternion GetProjectedRotation(Vector3 forward, Vector3 normal)
    {
        // forwardをnormalに投影して「地形に沿ったforward」を作る
        Vector3 projectedForward = Vector3.ProjectOnPlane(forward, normal).normalized;

        // その方向をforwardにして、upは法線にする
        return Quaternion.LookRotation(projectedForward, normal);
    }

    private Quaternion GetRotation(Transform camera, Vector3 normal)
    {
        switch (rotationMode)
        {
            case RotationMode.AlignWithSurfaceNormal:
                // カメラのforwardを地形に投影して法線に沿わせる
                Vector3 projectedForward = Vector3.ProjectOnPlane(camera.forward, normal).normalized;
                return Quaternion.LookRotation(projectedForward, normal);

            case RotationMode.FaceViewDirection:
                // カメラが向いている方向をそのまま使う（地形の傾き無視）
                return Quaternion.LookRotation(camera.forward, Vector3.up);

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
            isMentum = false;

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
    public void CmdHorus(Vector3 spawnPos, Quaternion spawnRot, NetworkConnectionToClient conn = null)
    {


        HorusSpawn(spawnPos, spawnRot, conn);

    }

    public void ChangeHorus()
    {
        if (!isHorus)
        {

            shootManager.canShoot = false;
            isHorus = true;
            isTerra = false;
            isMentum = false;

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

    public void HorusSpawn(Vector3 spawnPos, Quaternion dir, NetworkConnectionToClient conn = null) 
    {

        
        GameObject horusPrefab = Instantiate(horus, spawnPos, dir);

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
    public void Mentum()
    {

        if (currentCharacter.skill3Energy <= 0)
        {
            return;
        }

        ChangeMentum();



    }

    [Command]
    public void CmdMentum(Vector3 spawnPos, Quaternion spawnRot, NetworkConnectionToClient conn = null)
    {


        MentumSpawn(spawnPos, spawnRot, conn);

    }

    public void ChangeMentum()
    {
        if (!isMentum)
        {

            shootManager.canShoot = false;
            isMentum = true;
            isTerra = false;
            isHorus = false;

        }
        else
        {
            shootManager.canShoot = true;
            isMentum = false;
            shootManager.shootInputAhead = true;
        }
    }

    public void MentumPre(Vector3 pos, Quaternion dir)
    {
        if (mentumPreInstance == null)
        {
            mentumPreInstance = Instantiate(mentumPre, pos, dir).transform;
        }
        else
        {
            mentumPreInstance.transform.position = pos;
            mentumPreInstance.transform.rotation = dir;
        }
    }

    public void MentumSpawn(Vector3 spawnPos, Quaternion spawnRot, NetworkConnectionToClient conn = null)
    {


        GameObject mentumPrefab = Instantiate(mentum, spawnPos, spawnRot);

        // 誰が生成依頼したかを記録
        var ownerTag = mentumPrefab.AddComponent<SpawnOwner>();
        ownerTag.ownerNetId = conn.identity.netId; // 依頼者のnetIdを記録

        NetworkServer.Spawn(mentumPrefab, conn);
        RoundManager.spawns.Add(mentumPrefab);

        RpcChangeMentum(connectionToClient);
    }

    [TargetRpc]
    public void RpcChangeMentum(NetworkConnection target)
    {
        ChangeMentum();
    }

    public void DestroyMentumPre()
    {
        if (mentumPreInstance != null)
        {
            Destroy(mentumPreInstance.gameObject);
        }

    }


}





