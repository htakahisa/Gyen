using Mirror;
using UnityEngine;

[System.Serializable]
public class WeaponStatus
{

    // 武器の種類を定義
    public enum WeaponType
    {
        Hotaru,
        Lover,
        Leo,
        Liet,
        AntiREX,
        Kafka,
        FALLEN,
        KasMi,
        ReiNe,
        Hazard,
        RapetPuppet,
        Violets,
        Rebelliousness,


    }


    // ★ 残弾数を追加
    public int currentAmmo;
    public int weaponIndex;
    public WeaponType weaponType;
    public GameObject weaponPrefab;
    public WeaponDatabase dataBase;    // ← ここが重要！

    [HideInInspector] public GameObject instance;
    [HideInInspector] public bool isOwned;
}