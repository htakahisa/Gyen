using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReloadBar : MonoBehaviour
{

    WeaponManager weaponManager;
    public Slider reloadBar;

    float reloadDeltaTime = 0;
    Coroutine reloadCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (weaponManager == null)
        {
            if (RoundManager.rm.hasLoaded)
            {
                weaponManager = RoundManager.rm.GetMyPlayer().GetComponentInChildren<WeaponManager>();
            }
        }
        else
        {
            if (weaponManager.isReloading)
            {
                if (reloadCoroutine == null)
                {
                    reloadCoroutine = StartCoroutine(CountingReload(weaponManager.GetCurrentWeaponStats().magazineSize - weaponManager.magazine));
                }
            }
            else
            {
                if (reloadCoroutine != null)
                {
                    StopCoroutine(reloadCoroutine);
                }
                reloadCoroutine = null;
                reloadDeltaTime = 0;
                reloadBar.gameObject.SetActive(false);
            }
        }


    
    }

    public IEnumerator CountingReload(int reloadingMagazineNumber)
    {
        reloadBar.gameObject.SetActive(true);

        while (weaponManager.isReloading)
        {
            reloadBar.value = reloadDeltaTime / weaponManager.GetCurrentWeaponStats().reloadTime;
            yield return new WaitForSeconds(weaponManager.GetCurrentWeaponStats().reloadTime / reloadingMagazineNumber - (weaponManager.GetCurrentWeaponStats().reloadTime / reloadingMagazineNumber * 0.1f));
            reloadDeltaTime += weaponManager.GetCurrentWeaponStats().reloadTime / reloadingMagazineNumber;
        }

        

    }


}
