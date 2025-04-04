using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // ���݂̏�������
    [SerializeField] private WeaponType currentWeapon;

    // ����̃f�[�^�x�[�X
    private Dictionary<WeaponType, WeaponData> weaponDatabase = new Dictionary<WeaponType, WeaponData>()
    {
        { WeaponType.Lover, new WeaponData("Lover", 30, 70, 0.13f, 9, false, 80f, 1, false) },

    };

    // ���݂̕���̃f�[�^���擾
    public WeaponData GetCurrentWeaponData()
    {
        if (weaponDatabase.TryGetValue(currentWeapon, out WeaponData data))
        {
            return data;
        }
        return null;
    }

    // �����؂�ւ���
    public void SwitchWeapon(WeaponType newWeapon)
    {
        if (weaponDatabase.ContainsKey(newWeapon))
        {
            currentWeapon = newWeapon;
            Debug.Log($"Switched to {newWeapon}");
        }
    }

    // ����̎�ނ��`
    public enum WeaponType
    {
        Lover,

    }
}
