using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    // 現在の所持武器
    [SyncVar]
    private WeaponType currentWeapon;


    // 武器のデータベース
    private Dictionary<WeaponType, WeaponData> weaponDatabase = new Dictionary<WeaponType, WeaponData>()
    {
        { WeaponType.Lover, new WeaponData("Lover", 30, 70, 0.13f, 0.02f, 0.03f, 9, false, false, 80f, 0f, 1, false) },
        { WeaponType.Leo, new WeaponData("Leo", 25, 60, 0.11f, 0.05f, 0.03f, 12, false, false, 80f, 0f, 1, true) },
        { WeaponType.Liet, new WeaponData("Liet", 50, 133, 0.3f, 0f, 0.15f, 6, false, false, 80f, 0f ,1, false) },
        { WeaponType.AntiREX, new WeaponData("Anti-REX", 60, 160, 0.8f, 0f, 0.24f, 3, false, false, 80f, 0f ,1, false) },
        { WeaponType.Kafka, new WeaponData("Kafka", 35, 80, 0.08f, 0.07f, 0.07f, 20, true, false, 60f, 0.08f, 5, true) },
        { WeaponType.Hirtuarl, new WeaponData("Hirtuarl", 40, 160, 0.1f, 0.06f, 0.05f, 30, true, false, 50f, 0.1f, 1, true) },
        { WeaponType.Hazard, new WeaponData("Hazard", 150, 200, 1f, 0f, 0f, 5, true, true, 30f, 0.2f, 1, false) },
        { WeaponType.RapetPuppet, new WeaponData("RapetPuppet", 50, 100, 0.06f, 0.04f, 0.02f, 100, true, true, 45f, 0.5f, 1, true) },
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
            Debug.Log($"Switched to {newWeapon}");
        }
    }

    // 武器の種類を定義
    public enum WeaponType
    {
        Lover,
        Leo,
        Liet,
        AntiREX,
        Kafka,
        Hirtuarl,
        Hazard,
        RapetPuppet

    }
}
