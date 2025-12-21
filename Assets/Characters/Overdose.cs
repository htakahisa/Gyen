using Mirror;
using StarterAssets;
using System.Collections;
using UnityEngine;
using static WeaponStatus;

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

    [Tooltip("最大到達距離 (m)")]
    public float tweaksMaxDistance = 6f;

    [Tooltip("壁に当たったとき内側に押し戻すオフセット")]
    public float tweaksSurfaceOffset = 0.5f;

    [Tooltip("地面に置くとき上に浮かせるオフセット")]
    public float tweaksUpOffset = 0.5f;

    public GameObject mirror;

    [Tooltip("最大到達距離 (m)")]
    public float amMaxDistance = 5f;

    [Tooltip("壁に当たったとき内側に押し戻すオフセット")]
    public float amSurfaceOffset = 0.5f;

    [Tooltip("地面に置くとき上に浮かせるオフセット")]
    public float amUpOffset = 0.5f;

    public GameObject am;
    public GameObject amPre;

    public Transform amPreInstance;
    public Transform amInstance;

    public bool isAm;

    [TextArea]
    public string memo = "Skill1 = Tweaks, Skill2 = Horus, Skill3 = Singing";

    private void Awake()
    {
        Skill1 = Am;
        Skill2 = Mirror;
        Skill3 = Tweaks;
    }

    private void Update()
    {

        if (GetComponentInChildren<WeaponManager>().GetCurrentWeaponType() == WeaponType.ITweaks)
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

        if (GetComponentInChildren<WeaponManager>().GetCurrentWeaponType() == WeaponType.IAm)
        {

            Transform t = Camera.main.transform;

            if (am == null)
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
            if (Physics.Raycast(origin, direction, out hit, amMaxDistance, ground, QueryTriggerInteraction.Ignore))
            {
                // 壁の手前に配置
                spawnPos = hit.point + hit.normal * amSurfaceOffset;
                spawnRot = GetProjectedRotation(direction, hit.normal);

                // 仮設置（プレ表示）
                AmPre(spawnPos, spawnRot);

            }
            else
            {
                // 壁がなければプレビューを消す
                DestroyAmPre();
            }
        }
        else
        {
            DestroyAmPre();
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
    public void RequestTweaks()
    {
        if (!isOwned) return;
        CmdTweaks(tweaksPreInstance.position, tweaksPreInstance.rotation);
    }

    public void Tweaks()
    {
        if (currentCharacter.skill3Energy <= 0) return;
        GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponType.ITweaks);
    }


    [Command]
    public void CmdTweaks(Vector3 spawnPos, Quaternion spawnRot)
    {

        currentCharacter.skill3Energy--;
        TweaksSpawn(spawnPos, spawnRot);

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

    public void TweaksSpawn(Vector3 spawnPos, Quaternion dir)
    {
        var instance = RoundManager.rm.ObjectSpawn(tweaks, spawnPos, dir, true, false, connectionToClient);
        instance.GetComponent<SpawnOwner>().ownerNetId = netId;
    }


    public void DestroyTweaksPre()
    {
        if (tweaksPreInstance != null)
        {
            Destroy(tweaksPreInstance.gameObject);
        }

    }

    public void RequestMirror()
    {
        if (!isOwned) return;
        CmdMirror(Camera.main.transform.position, Camera.main.transform.forward);
    }

    public void Mirror()
    {

        if (currentCharacter.skill2Energy <= 0)
        {
            return;
        }
        GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponType.IMirror);

    }

    [Command]
    public void CmdMirror(Vector3 pos, Vector3 dir)
    {

        currentCharacter.skill2Energy--;
        MirrorSpawn(pos, dir);

    }

    public void MirrorSpawn(Vector3 pos, Vector3 dir)
    {
        var instance = RoundManager.rm.ObjectSpawn(mirror, pos, Quaternion.LookRotation(dir), true, false, connectionToClient);
        instance.GetComponent<SpawnOwner>().ownerNetId = netId;

    }

    public void RequestAm()
    {
        if (!isOwned) return;
        CmdAm(amPreInstance.position, amPreInstance.rotation);
    }

    public void Am()
    {
        if (amInstance != null) 
        {
            GetComponentInChildren<ThirdPersonController>().ServerUpdateAllPositions(amInstance.transform.position, amInstance.transform.rotation.eulerAngles);
            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.IAM, amInstance.transform.position, 0.5f, 30);
            CmdDestroyAm();
        }
        else
        {
            if (currentCharacter.skill1Energy <= 0)
            {
                return;
            }
            GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponType.IAm);
        }

    }

    [Command]
    public void CmdDestroyAm()
    {
        NetworkServer.Destroy(amInstance.gameObject);
    }

    [Command]
    public void CmdAm(Vector3 spawnPos, Quaternion spawnRot)
    {

        currentCharacter.skill1Energy--;
        AmSpawn(spawnPos, spawnRot);

    }


    public void AmPre(Vector3 pos, Quaternion dir)
    {
        if (amPreInstance == null)
        {
            amPreInstance = Instantiate(amPre, pos, dir).transform;
        }
        else
        {
            amPreInstance.transform.position = pos;
            amPreInstance.transform.rotation = dir;
        }
    }

    public void AmSpawn(Vector3 spawnPos, Quaternion dir)
    {
        amInstance = RoundManager.rm.ObjectSpawn(am, spawnPos, dir, true, false, connectionToClient).transform;
        amInstance.GetComponent<SpawnOwner>().ownerNetId = netId;
    }

    public void DestroyAmPre()
    {
        if (amPreInstance != null)
        {
            Destroy(amPreInstance.gameObject);
        }

    }

}





