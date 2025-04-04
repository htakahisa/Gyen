using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public string weaponName;

    public int damage;
    public int headDamage;

    public float rate;

    public float magazineSize;
    
    public bool zoomable;
    public float zoomRatio;

    public int burst;

    public bool isAuto;

    public WeaponData(string name, int damage, int headDamage, float rate, int magazineSize, bool zoomable, float zoomRatio, int burst, bool isAuto)
    {
        this.weaponName = name;

        this.damage = damage;
        this.headDamage = headDamage;

        this.rate = rate;

        this.magazineSize = magazineSize;

        this.zoomable = zoomable;
        this.zoomRatio = zoomRatio;

        this.burst = burst;

        this.isAuto = isAuto;
    }
}