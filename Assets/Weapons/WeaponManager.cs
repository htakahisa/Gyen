using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    // 現在の所持武器
    [SyncVar]
    private WeaponType currentWeapon;

    public int magazine;

    public bool isReloading = false;

    // 武器のデータベース
    private Dictionary<WeaponType, WeaponData> weaponDatabase = new Dictionary<WeaponType, WeaponData>()
    {
        { WeaponType.Hotaru, new WeaponData("Hotaru", 50, 150, 0f, 0f, 0f, 0, 0f, true, false, 55f, 0f, 1, false) },
        { WeaponType.Lover, new WeaponData("Lover", 30, 70, 0.13f, 0.02f, 0.015f, 7, 0.8f,false, false, 80f, 0f, 1, false) },
        { WeaponType.Leo, new WeaponData("Leo", 25, 60, 0.11f, 0.06f, 0.03f, 12, 1f,false, false, 80f, 0f, 1, true) },
        { WeaponType.Liet, new WeaponData("Liet", 50, 133, 0.3f, 0f, 0.05f, 8, 1.3f,false, false, 80f, 0f ,1, false) },
        { WeaponType.AntiREX, new WeaponData("Anti-REX", 65, 180, 0.8f, 0f, 0.1f, 3, 1.3f, false, false, 80f, 0f ,1, false) },
        { WeaponType.Kafka, new WeaponData("Kafka", 30, 80, 0.065f, 0.07f, 0.015f, 20, 1.5f,true, false, 65f, 0.05f, 5, true) },
        { WeaponType.KasMi, new WeaponData("KasMi", 35, 140, 0.07f, 0.03f, 0.017f, 33, 1.7f,true, false, 50f, 0.04f, 2, true) },
        { WeaponType.ReiNe, new WeaponData("ReiNe", 40, 160, 0.1f, 0.09f, 0.035f, 25, 1.5f,true, false, 60f, 0.07f, 1, true) },
        { WeaponType.Hazard, new WeaponData("Hazard", 150, 300, 1f, 0f, 0f, 2, 3f,true, true, 30f, 0.2f, 1, false) },
        { WeaponType.RapetPuppet, new WeaponData("RapetPuppet", 30, 70, 0.06f, 0.3f, 0.03f, 100, 2f,true, false, 40f, 0.3f, 1, true) },
        { WeaponType.Violets, new WeaponData("Violets", 45, 100, 0.05f, 0.2f, 0.025f, 152, 4f,true, false, 45f, 0.5f, 1, true) },
    };

    // 現在の武器のデータを取得
    public WeaponData GetCurrentWeaponData()
    {
        if (weaponDatabase.TryGetValue(currentWeapon, out WeaponData data))
        {
            return data;
        }
        return null;
    }

    // 武器を切り替える
    public void SwitchWeapon(WeaponType newWeapon)
    {
        if (weaponDatabase.ContainsKey(newWeapon))
        {
            currentWeapon = newWeapon;
            Reload();
            Debug.Log($"Switched to {newWeapon}");
        }
    }

    // 武器を切り替える
    public void BuyWeapon(WeaponType newWeapon)
    {
        if (weaponDatabase.ContainsKey(newWeapon))
        {
            currentWeapon = newWeapon;
            SetMagazineMax();
            Debug.Log($"Switched to {newWeapon}");
        }
    }

    // 武器の種類を定義
    public enum WeaponType
    {
        Hotaru,
        Lover,
        Leo,
        Liet,
        AntiREX,
        Kafka,
        KasMi,
        ReiNe,
        Hazard,
        RapetPuppet,
        Violets,
        

    }

    public void Reload()
    {
        isReloading = true;
        Invoke("SetMagazineMax", GetCurrentWeaponData().reloadTime);
    }

    public void SetMagazineMax()
    {
        isReloading = false;
        magazine = GetCurrentWeaponData().magazineSize;
    }

}
