using Mirror;
using StarterAssets;
using UnityEngine;

public class Lucifer : CharacterSkills
{
    public AbilityController abilityController;
    public CharacterSkills currentCharacter;
    public ShootManager shootManager;
    public HpMaster hpMaster;
    public WeaponManager weaponManager;
    public ServerCheckShoot serverCheckShoot;

    [TextArea]
    public string memo = "Skill1 = Rebelliousness, Skill2 = Lightload, Skill3 = Darkness";

    private void Awake()
    {
        Skill1 = Rebelliousness;
        Skill2 = Lightload;
        Skill3 = null;
    }

    public void Update()
    {
        serverCheckShoot.isDarkness = true;
    }




    public void Rebelliousness()
    {
        if (currentCharacter.skill2Energy <= 0)
        {
            return;
        }
        currentCharacter.skill2Energy--;
        weaponManager.EquipWeapon(WeaponStatus.WeaponType.Rebelliousness);
    }
    public void Lightload()
    {
        if (currentCharacter.skill2Energy <= 0)
        {
            return;
        }

        if(GetComponentInChildren<ServerCheckShoot>().GetDarkOrb() != null)
        {
            CmdCollectOrb();
        }
    }

    public void CmdCollectOrb()
    {
        GetComponentInChildren<ServerCheckShoot>().GetDarkOrb().GetComponent<Darkness>().Collected();
        currentCharacter.skill2Energy--;
        GetComponentInChildren<ServerCheckShoot>().DestroyOrb(GetComponentInChildren<ServerCheckShoot>().GetDarkOrb());
        GetComponentInChildren<ScanCamera>().CameraTimeOn(1f);
    }






}





