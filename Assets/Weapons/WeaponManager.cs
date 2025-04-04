using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // 現在の所持武器
    [SerializeField] private WeaponType currentWeapon;

    // 武器のデータベース
    private Dictionary<WeaponType, WeaponData> weaponDatabase = new Dictionary<WeaponType, WeaponData>()
    {
        { WeaponType.Lover, new WeaponData("Lover", 30, 70, 0.13f, 9, false, 80f, 1, false) },

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

    }
}
