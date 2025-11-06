using StarterAssets;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/TridentSkillData")]
public class TridentCharacterData : CharacterDataBase
{
    public int Skill1Energy;
    public int Skill2Energy;
    public int Skill3Energy;
    
    public AbilityController abilityController;
    public GameObject limePre;
    public HpMaster hpMaster;
    public GameObject healEffect;
    public GameObject healEffectInstance;

    public float singInterval;
}

[CreateAssetMenu(menuName = "Character/EjahSkillData")]
public class EjahCharacterData : CharacterDataBase
{
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
}
