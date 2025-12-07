using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static WeaponStatus;

public class WeaponManager : NetworkBehaviour
{
    public ShootManager shootManager;
    public Transform weaponHolder;
    public List<WeaponStatus> weapons;

    [SyncVar] public int magazine;  // ← 現在装備中の武器の残弾数
    [SyncVar] public bool isReloading = false;

    [SyncVar] public int currentWeaponIndex = -1;

    [SyncVar] public WeaponType mainWeaponType;
    [SyncVar] public WeaponType subWeaponType;
    [SyncVar] public WeaponType abilityWeaponType;
    public Coroutine reloadCoroutine;

    private PlayerInputActions inputActions;

    public override void OnStartAuthority()
    {
        if (!isLocalPlayer) return;
        inputActions = new PlayerInputActions();
        enabled = true;
        inputActions.Player.Enable();

        inputActions.Player.SwitchPrimary.performed += _ => CmdSwitchWeapon(mainWeaponType);
        inputActions.Player.SwitchSidearm.performed += _ => CmdSwitchWeapon(subWeaponType);
    }


    private void Update()
    {
        if (!isLocalPlayer) return;

    }

    public void BotEquipWeapon(WeaponType type)
    {
        RpcEquipWeapon(type);
    }

    public WeaponType GetCurrentWeaponType()
    {
        if (currentWeaponIndex == -1)
        {
            Debug.LogWarning("現在装備中の武器はありません");
            return default;
        }
        return weapons[currentWeaponIndex].weaponType;
    }

    public WeaponStatus GetCurrentWeaponSlot()
    {
        if (currentWeaponIndex == -1) return null;
        return weapons[currentWeaponIndex];
    }

    public WeaponDatabase GetCurrentWeaponStats()
    {
        var slot = GetCurrentWeaponSlot();
        return slot != null ? slot.dataBase : null;
    }

    [Command(requiresAuthority = false)]
    public void CmdEquipWeapon(WeaponType type)
    {
        RpcEquipWeapon(type);
    }

    [ClientRpc]
    public void RpcEquipWeapon(WeaponType type)
    {
        shootManager.ResetZoom();
        int index = weapons.FindIndex(w => w.weaponType == type);
        if (index == -1) return;

        // 現在の武器の弾数を保存
        if (currentWeaponIndex != -1)
        {
            var prevSlot = weapons[currentWeaponIndex];
            prevSlot.currentAmmo = magazine;
        }

        // 非表示
        if (currentWeaponIndex != -1 && weapons[currentWeaponIndex].instance != null)
            weapons[currentWeaponIndex].instance.SetActive(false);

        // インスタンス生成
        if (weapons[index].instance == null)
        {
            var newWeapon = Instantiate(weapons[index].weaponPrefab, weaponHolder);
            weapons[index].instance = newWeapon;
        }

        weapons[index].instance.SetActive(true);
        currentWeaponIndex = index;

        // ★ 切り替えた武器の弾数を magazine に反映
        var newSlot = weapons[index];
        magazine = newSlot.currentAmmo;

        // 見た目・射撃タイプ反映
        GetComponent<ThirdPersonController>().CmdChangeGunType(newSlot.dataBase.gunType);
    }

    [ClientRpc]
    public void RpcBuyWeapon(WeaponType type)
    {
        var slot = weapons.Find(w => w.weaponType == type);
        if (slot == null) return;

        var gunType = slot.dataBase.gunType;
        if (gunType == "Rifle")
        {
            mainWeaponType = type;
        }
        else if (gunType == "Pistol")
        {
            subWeaponType = type;
        }
        else if (gunType == "Ability")
        {
            abilityWeaponType = type;
        }

        // ★ 弾数初期化
        slot.currentAmmo = slot.dataBase.magazineSize;

        StartCoroutine(BuyWeaponAndSetMagazine(type));
    }

    // 購入処理（メイン／サブ判定は前回実装のまま）
    [Command(requiresAuthority = false)]
    public void CmdBuyWeapon(WeaponType type)
    {
        if(RoundManager.rm.currentMode == RoundManager.Mode.DOUBLETAP)
        {
            foreach (var bot in RoundManager.rm.GetMyBots(GetComponentInParent<NetworkIdentity>()))
            {
                bot.GetComponentInChildren<WeaponManager>().RpcBuyWeapon(type);
            }
        }
        RpcBuyWeapon(type);
    }

    // 売却処理（メイン／サブ判定は前回実装のまま）
    [ClientRpc]
    public void RpcSellWeapon(WeaponType type)
    {
        var slot = weapons.Find(w => w.weaponType == type);
        if (slot == null) return;

        var gunType = slot.dataBase.gunType;
        if (gunType == "Rifle")
        {
            mainWeaponType = WeaponType.Hotaru;
            CmdSwitchWeapon(mainWeaponType);
        }
        else if (gunType == "Pistol")
        {
            subWeaponType = WeaponType.Lover;
            CmdSwitchWeapon(subWeaponType);
        }

        // ★ 弾数初期化
        slot.currentAmmo = slot.dataBase.magazineSize;

        StartCoroutine(BuyWeaponAndSetMagazine(type));
    }

    [Command(requiresAuthority = false)]
    public void CmdSellWeapon(WeaponType type)
    {
        RpcSellWeapon(type);
    }

    private IEnumerator BuyWeaponAndSetMagazine(WeaponType type)
    {
        var slot = weapons.Find(w => w.weaponType == type);
        if (slot != null)
        {
            CmdEquipWeapon(type);
            yield return new WaitWhile(() => GetCurrentWeaponType() != type);
            StartCoroutine(SetMagazineMax(0f));
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
        var slot = GetCurrentWeaponSlot();
        if (slot == null) return;

        var data = slot.dataBase;
        if ((isReloading || magazine == data.magazineSize) && shootManager.canShoot)
            return;

        GetComponent<ThirdPersonController>().Reloading();
        isReloading = true;
        reloadCoroutine = StartCoroutine(SetMagazineMax(data.reloadTime));
    }

    public IEnumerator SetMagazineMax(float time)
    {
        yield return new WaitForSeconds(time);

        var slot = GetCurrentWeaponSlot();
        if (slot == null) yield break;

        var tpc = GetComponent<ThirdPersonController>();
        tpc.EndReloading();
        isReloading = false;

        magazine = slot.dataBase.magazineSize;
        slot.currentAmmo = magazine;   // ★ 武器データにも反映
    }

    [Command(requiresAuthority = false)]
    public void CmdSwitchWeapon(WeaponType type)
    {
        if (mainWeaponType.Equals(default) || subWeaponType.Equals(default) || abilityWeaponType.Equals(default))
            return;

        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
            var tpc = GetComponent<ThirdPersonController>();
            tpc.EndReloading();
        }

        RpcEquipWeapon(type);
        
    }

    public void HideWeapon()
    {
        if (currentWeaponIndex != -1)
            weapons[currentWeaponIndex].instance.SetActive(false);
    }

    // u should call this method only when u had hide weapon then.
    public void ReHideWeapon()
    {
        if (currentWeaponIndex != -1)
            weapons[currentWeaponIndex].instance.SetActive(true);
    }

    [Command]
    public void CmdStopReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
        }
    }
}
