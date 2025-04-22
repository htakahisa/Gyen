using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public string weaponName;

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

    public bool isAuto;

    public WeaponData(string name, int damage, int headDamage, float rate, float Xrecoil, float Yrecoil, int magazineSize, float reloadTime ,bool zoomable, bool isNeedZoom, float zoomRatio, float zoomSpeed, int burst, bool isAuto)
    {
        this.weaponName = name;

        this.damage = damage;
        this.headDamage = headDamage;

        this.rate = rate;

        this.Xrecoil = Xrecoil;
        this.Yrecoil = Yrecoil;

        this.magazineSize = magazineSize;
        this.reloadTime = reloadTime;

        this.zoomable = zoomable;
        this.isNeedZoom = isNeedZoom;
        this.zoomRatio = zoomRatio;
        this.zoomSpeed = zoomSpeed;
        

        this.burst = burst;

        this.isAuto = isAuto;
    }
}