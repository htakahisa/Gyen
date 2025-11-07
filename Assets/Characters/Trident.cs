using Mirror;
using StarterAssets;
using UnityEngine;

public class Trident : CharacterSkills
{

    public AbilityController abilityController;
    public HpMaster hpMaster;
    public GameObject limePre;
    public GameObject healEffect;
    public GameObject healEffectInstance;

    //because of these will get the values from GetComponent in Awake(), u dont need to get the these values from CharacterData
    public CharacterSkills currentCharacter;
    public ShootManager shootManager;

    public float singInterval;

    public bool isSinging;
    public float singIntervalTimer;


    [TextArea]
    public string memo = "Skill1 = Lime, Skill2 = Yellow, Skill3 = Singing";

    private void Awake()
    {

        Skill1 = Lime;
        Skill2 = Yellow;
        Skill3 = Singing;
    }

    private void Update()
    {

        if (Input.GetMouseButton(0) && isSinging && hpMaster.hp < 100)
        {


            if (currentCharacter.skill3Energy <= 0)
            {
                CmdDestroyHealEffect();
                return;
            }

            singIntervalTimer += Time.deltaTime;

            if (singIntervalTimer >= singInterval)
            {
                CmdSpawnHealEffect();
                CmdSingHeal();
                singIntervalTimer = 0;
            }

        }
        else
        {
                CmdDestroyHealEffect();
                singIntervalTimer = 0;
        }

        if (healEffectInstance != null)
        {
            healEffectInstance.transform.position = transform.position;
        }
    }

        
    

    public void Lime()
    {
        if (currentCharacter.skill1Energy <= 0)
        {
            return;
        }

        
        Transform mainCamera = Camera.main.transform;
        CmdLime(mainCamera.position + mainCamera.forward, mainCamera.forward);
        
    }



    [Command]
    public void CmdLime(Vector3 pos, Vector3 dir)
    {
        currentCharacter.skill1Energy--;
        GameObject instance = Instantiate(limePre, pos, Quaternion.LookRotation(dir));
        NetworkServer.Spawn(instance);
        RoundManager.spawns.Add(instance);

    }

    public void Yellow()
    {
        if (abilityController.currentForm == AbilityController.PlayerForm.Human)
        {
            if (currentCharacter.skill2Energy <= 0)
            {
                return;
            }

            CmdSkill2Minus();
            abilityController.BeBird();
        }
        else if(abilityController.currentForm == AbilityController.PlayerForm.Bird)
        {
            abilityController.BeHuman();
        }
    }

    [Command]
    public void CmdSkill2Minus()
    {
        currentCharacter.skill2Energy--;
    }

    public void Singing()
    {
      
        
        if (!isSinging && hpMaster.hp < 100) {

            shootManager.canShoot = false;
            isSinging = true;

        }
        else
        {
            shootManager.canShoot = true;
            isSinging = false;
        }

    }

    [Command]
    public void CmdSingHeal()
    {
        currentCharacter.skill3Energy--;
        if (hpMaster.hp + 10 > 100)
        {
            hpMaster.TakeDamage(hpMaster.hp - 100, false);
        }
        else
        {
            hpMaster.TakeDamage(-10, false);
        }
    }

    [Command]
    public void CmdSpawnHealEffect()
    {
        if (healEffectInstance == null)
        {
            healEffectInstance = Instantiate(healEffect, transform.position, Quaternion.identity);
            NetworkServer.Spawn(healEffectInstance);
            RoundManager.spawns.Add(healEffectInstance);
        }
    }

    [Command]
    public void CmdDestroyHealEffect()
    {
        if (healEffectInstance != null)
        {
            NetworkServer.Destroy(healEffectInstance);
        }
    }

}





