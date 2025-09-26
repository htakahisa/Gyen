using Mirror;
using StarterAssets;
using System.Collections;
using UnityEngine;

public class Overdose : CharacterSkills
{
    public AbilityController abilityController;
    public CharacterSkills currentCharacter;
    public ShootManager shootManager;
    public HpMaster hpMaster;
    public enum RotationMode { Identity, AlignWithSurfaceNormal, FaceViewDirection }
    [Tooltip("生成時の回転方法")]
    public RotationMode rotationMode = RotationMode.Identity;

    public LayerMask ground;


    public GameObject tweaks;
    public GameObject tweaksPre;

    public Transform tweaksPreInstance;

    public bool isTweaks;

    [Tooltip("最大到達距離 (m)")]
    public float tweaksMaxDistance = 3f;

    [Tooltip("壁に当たったとき内側に押し戻すオフセット")]
    public float tweaksSurfaceOffset = 0.5f;

    [Tooltip("地面に置くとき上に浮かせるオフセット")]
    public float tweaksUpOffset = 0.5f;

    public bool isMirror;

    public GameObject mirror;

    [TextArea]
    public string memo = "Skill1 = Tweaks, Skill2 = Horus, Skill3 = Singing";

    private void Awake()
    {
        Skill1 = Tweaks;
        Skill2 = Mirror;
        //Skill3 = Mentum;
    }

    private void Update()
    {

        if (isTweaks)
        {

            Transform t = Camera.main.transform;

            if (tweaks == null)
            {
                Debug.LogWarning("prefabToSpawn が設定されていません。");
                return;
            }

            Vector3 origin = t.position;
            Vector3 direction = t.forward;

            RaycastHit hit;
            Vector3 spawnPos;
            Quaternion spawnRot = Quaternion.identity;

            // ---- 1. 前方レイキャスト（壁チェックのみ） ----
            if (Physics.Raycast(origin, direction, out hit, tweaksMaxDistance, ground, QueryTriggerInteraction.Ignore))
            {
                // 壁の手前に配置
                spawnPos = hit.point + hit.normal * tweaksSurfaceOffset;
                spawnRot = GetProjectedRotation(direction, hit.normal);

                // 仮設置（プレ表示）
                TweaksPre(spawnPos, spawnRot);

                // 左クリックで確定
                if (Input.GetMouseButtonDown(0))
                {
                    currentCharacter.skill1Energy--;
                    CmdTweaks(spawnPos, spawnRot);
                }
            }
            else
            {
                // 壁がなければプレビューを消す
                DestroyTweaksPre();
            }



        }
        else
        {
            DestroyTweaksPre();
        }
        if (isMirror)
        {
            // 左クリックで確定
            if (Input.GetMouseButtonDown(0))
            {
                currentCharacter.skill2Energy--;
                CmdMirror(Camera.main.transform.position, Camera.main.transform.rotation);
            }
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




    public void Tweaks()
    {

        if (currentCharacter.skill1Energy <= 0)
        {
            return;
        }

        ChangeTweaks();



    }


    [Command]
    public void CmdTweaks(Vector3 spawnPos, Quaternion spawnRot, NetworkConnectionToClient conn = null)
    {


        TweaksSpawn(spawnPos, spawnRot, conn);

    }


    public void ChangeTweaks()
    {
        if (!isTweaks)
        {

            shootManager.canShoot = false;
            isTweaks = true;
            isMirror = false;
        }
        else
        {
            shootManager.canShoot = true;
            isTweaks = false;
            shootManager.shootInputAhead = true;
        }
    }

    public void TweaksPre(Vector3 pos, Quaternion dir)
    {
        if (tweaksPreInstance == null)
        {
            tweaksPreInstance = Instantiate(tweaksPre, pos, dir).transform;
        }
        else
        {
            tweaksPreInstance.transform.position = pos;
            tweaksPreInstance.transform.rotation = dir;
        }
    }

    public void TweaksSpawn(Vector3 spawnPos, Quaternion dir, NetworkConnectionToClient conn = null)
    {


        GameObject tweaksPrefab = Instantiate(tweaks, spawnPos, dir);

        // 誰が生成依頼したかを記録
        var ownerTag = tweaksPrefab.AddComponent<SpawnOwner>();
        ownerTag.ownerNetId = conn.identity.netId; // 依頼者のnetIdを記録

        NetworkServer.Spawn(tweaksPrefab, conn);
        RoundManager.spawns.Add(tweaksPrefab);

        RpcChangeTweaks(connectionToClient);
    }

    [TargetRpc]
    public void RpcChangeTweaks(NetworkConnection target)
    {
        ChangeTweaks();
    }

    public void DestroyTweaksPre()
    {
        if (tweaksPreInstance != null)
        {
            Destroy(tweaksPreInstance.gameObject);
        }

    }

    public void Mirror()
    {

        if (currentCharacter.skill2Energy <= 0)
        {
            return;
        }

        ChangeMirror();

    }

    [Command]
    public void CmdMirror(Vector3 spawnPos, Quaternion spawnRot, NetworkConnectionToClient conn = null)
    {


        MirrorSpawn(spawnPos, spawnRot, conn);

    }
    public void ChangeMirror()
    {
        if (!isMirror)
        {

            shootManager.canShoot = false;
            isMirror = true;
            isTweaks = false;
        }
        else
        {
            shootManager.canShoot = true;
            isMirror = false;
            shootManager.shootInputAhead = true;
        }
    }

    public void MirrorSpawn(Vector3 spawnPos, Quaternion dir, NetworkConnectionToClient conn = null)
    {

        GameObject mirrorPrefab = Instantiate(mirror, spawnPos, dir);

        // 誰が生成依頼したかを記録
        var ownerTag = mirrorPrefab.AddComponent<SpawnOwner>();
        ownerTag.ownerNetId = conn.identity.netId; // 依頼者のnetIdを記録

        NetworkServer.Spawn(mirrorPrefab, conn);
        RoundManager.spawns.Add(mirrorPrefab);

        RpcChangeMirror(connectionToClient);
    }

    [TargetRpc]
    public void RpcChangeMirror(NetworkConnection target)
    {
        ChangeMirror();
    }


}





