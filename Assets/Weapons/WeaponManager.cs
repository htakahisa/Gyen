using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    // ���݂̏�������
    [SyncVar]
    private WeaponType currentWeapon;

    public int magazine;

    public bool isReloading = false;

    // ����̃f�[�^�x�[�X
    private Dictionary<WeaponType, WeaponData> weaponDatabase = new Dictionary<WeaponType, WeaponData>()
    {
        { WeaponType.Lover, new WeaponData("Lover", 30, 70, 0.13f, 0.02f, 0.03f, 7, 0.8f,false, false, 80f, 0f, 1, false) },
        { WeaponType.Leo, new WeaponData("Leo", 25, 60, 0.11f, 0.05f, 0.03f, 10, 1f,false, false, 80f, 0f, 1, true) },
        { WeaponType.Liet, new WeaponData("Liet", 50, 133, 0.3f, 0f, 0.15f, 5, 1.3f,false, false, 80f, 0f ,1, false) },
        { WeaponType.AntiREX, new WeaponData("Anti-REX", 60, 160, 0.8f, 0f, 0.24f, 3, 1.3f, false, false, 80f, 0f ,1, false) },
        { WeaponType.Kafka, new WeaponData("Kafka", 35, 80, 0.08f, 0.07f, 0.07f, 20, 1.5f,true, false, 60f, 0.08f, 5, true) },
        { WeaponType.Hirtuarl, new WeaponData("Hirtuarl", 40, 160, 0.1f, 0.06f, 0.05f, 30, 1.5f,true, false, 50f, 0.1f, 1, true) },
        { WeaponType.Hazard, new WeaponData("Hazard", 150, 200, 1f, 0f, 0f, 5, 3f,true, true, 30f, 0.2f, 1, false) },
        { WeaponType.RapetPuppet, new WeaponData("RapetPuppet", 50, 100, 0.06f, 0.08f, 0.02f, 100, 4f,true, true, 45f, 0.5f, 1, true) },
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
            Reload();
            Debug.Log($"Switched to {newWeapon}");
        }
    }

    // �����؂�ւ���
    public void BuyWeapon(WeaponType newWeapon)
    {
        if (weaponDatabase.ContainsKey(newWeapon))
        {
            currentWeapon = newWeapon;
            SetMagazineMax();
            Debug.Log($"Switched to {newWeapon}");
        }
    }

    // ����̎�ނ��`
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
