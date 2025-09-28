using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static WeaponStatus;

public class WeaponManager : NetworkBehaviour
{
    public ShootManager shootManager;



    [SyncVar]
    public int magazine;

    [SyncVar]
    public bool isReloading = false;

    public Transform weaponHolder;
    public List<WeaponStatus> weapons;

    [SyncVar]
    private int currentWeaponIndex = -1;



    // 武器のデータベース
    //private Dictionary<WeaponType, WeaponData> weaponDatabase = new Dictionary<WeaponType, WeaponData>()
    //{
    //    { WeaponType.Hotaru, new WeaponData("Hotaru", 50, 400, 0f, 0f, 0f, 0, 0f, true, false, 55f, 0f, 1, 0, false) },
    //    { WeaponType.Lover, new WeaponData("Lover", 20, 80, 0.17f, 0.04f, 0.3f, 7, 0.8f,false, false, 80f, 0f, 1, 0, false) },
    //    { WeaponType.Leo, new WeaponData("Leo", 30, 70, 0.11f, 0.1f, 0.35f, 12, 1.2f,false, false, 80f, 0f, 1, 0, true) },
    //    { WeaponType.Liet, new WeaponData("Liet", 35, 125, 0.15f, 0f, 0.15f, 9, 1.3f,false, false, 80f, 0f ,1, 0, false) },
    //    { WeaponType.AntiREX, new WeaponData("Anti-REX", 65, 280, 0.7f, 0f, 0.75f, 4, 1.3f, false, false, 80f, 0f , 1, 0, false) },
    //    { WeaponType.Kafka, new WeaponData("Kafka", 25, 80, 0.065f, 0.3f, 0.4f, 20, 1.5f,true, false, 65f, 0.02f, 5, 0.06f, true) },
    //    { WeaponType.FALLEN, new WeaponData("FALLEN", 70, 290, 0.4f, 0f, 0.4f, 12, 1.5f,true, false, 45f, 0.03f , 1, 0, false) },
    //    { WeaponType.KasMi, new WeaponData("KasMi", 45, 190, 0.09f, 0.05f, 0.17f, 30, 1.7f,true, false, 50f, 0.03f, 2, 0.07f, true) },
    //    { WeaponType.ReiNe, new WeaponData("ReiNe", 60, 200, 0.12f, 0.1f, 0.55f, 20, 1.5f,true, false, 60f, 0.05f, 1, 0, true) },
    //    { WeaponType.Hazard, new WeaponData("Hazard", 200, 400, 1f, 0f, 0f, 2, 3f,true, true, 25f, 0.1f, 1, 0, false) },
    //    { WeaponType.RapetPuppet, new WeaponData("RapetPuppet", 30, 50, 0.07f, 0.3f, 0.4f, 70, 2f,true, false, 40f, 0.1f, 1, 0, true) },
    //    { WeaponType.Violets, new WeaponData("Violets", 45, 100, 0.06f, 0.4f, 0.3f, 152, 4f,true, false, 45f, 0.2f, 1, 0, true) },
    //    { WeaponType.Rebelliousness, new WeaponData("Rebelliousness", 70, 400, 0.5f, 0f, 0.6f, 3, 1f, true, false, 65f, 0.05f, 2, 0.2f, false) },
    //};


private void Awake()
{
    int count = weapons.Count;
    for (int i = 0; i < count; i++)
    {
        var data = weapons[i];

        WeaponStatus slot = new WeaponStatus();
        slot.weaponType = data.weaponType;
        slot.weaponPrefab = data.weaponPrefab;
        slot.instance = null;

        weapons.Add(slot);
    }
}
    public WeaponType GetCurrentWeaponType()
    {
        if (currentWeaponIndex == -1)
        {
            Debug.LogWarning("現在装備中の武器はありません");
            return default; // 0番目のenumが返る（Pistolなど）
        }

        return weapons[currentWeaponIndex].weaponType;
    }

    public WeaponDatabase GetCurrentWeaponStats()
    {
        if (currentWeaponIndex == -1)
        {
            Debug.LogWarning("現在、装備中の武器はありません");
            return null;
        }
        return weapons[currentWeaponIndex].dataBase;
    }

    [Command]
    public void CmdEquipWeapon(WeaponType type)
    {
        RpcEquipWeapon(type);
    }


    [ClientRpc]
    public void RpcEquipWeapon(WeaponType type)
    {
        int index = weapons.FindIndex(w => w.weaponType == type);
        if (index == -1) return;

        if (currentWeaponIndex != -1 && weapons[currentWeaponIndex].instance != null)
            weapons[currentWeaponIndex].instance.SetActive(false);

        if (weapons[index].instance == null)
        {
            var newWeapon = Instantiate(weapons[index].weaponPrefab, weaponHolder);
            weapons[index].instance = newWeapon;
        }

        weapons[index].instance.SetActive(true);
        currentWeaponIndex = index;

        GetComponent<ThirdPersonController>().CmdChangeGunType(weapons[currentWeaponIndex].dataBase.gunType);

    }

    public void HideWeapon()
    {
        weapons[currentWeaponIndex].instance.SetActive(false);
    }


    // 武器を購入
    [Command(requiresAuthority = false)]
    public void CmdBuyWeapon(WeaponType type)
    {
        var slot = weapons.Find(w => w.weaponType == type);
        if (slot != null)
        {
            RpcEquipWeapon(type);
            SetMagazineMax();
            Debug.Log($"Switched to {type}");
        }
    }




    [Command(requiresAuthority = false)]
    public void CmdReload()
    {
        RpcReload();
    }

    [ClientRpc]
    public void RpcReload()
    {
        var slot = GetCurrentWeaponStats();
        if (slot != null)
        {
            if ((isReloading || magazine == slot.magazineSize) && shootManager.canShoot)
            {
                return;
            }
            GetComponent<ThirdPersonController>().Reloading();
            isReloading = true;
            Invoke("SetMagazineMax", GetCurrentWeaponStats().reloadTime);
        }
    }

    public void SetMagazineMax()
    {
        GetComponent<ThirdPersonController>().EndReloading();
        isReloading = false;
        magazine = GetCurrentWeaponStats().magazineSize;
    }

}
