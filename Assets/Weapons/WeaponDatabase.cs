using UnityEngine;
using static WeaponStatus;

[CreateAssetMenu(fileName = "WeaponStats", menuName = "FPS/Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    public string gunType;
    public string weaponName;
    public string description;
    public int cost;
    public int damage;
    public int headDamage;

    public float rate;

    public float Xrecoil;

    public float Yrecoil;

    public int magazineSize;
    public float reloadTime;

    public bool zoomable;
    public bool isNeedZoom;
    public float zoomRatio;
    public float zoomSpeed;

    public int burst;
    public float burstRate;

    public bool isAuto;
}

