using Mirror;
using StarterAssets;
using UnityEngine;

public class Trident : CharacterSkills
{

    public TridentCharacterData tridentData;

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
        default1Energy = tridentData.Skill1Energy;
        default2Energy = tridentData.Skill2Energy;
        default3Energy = tridentData.Skill3Energy;
        limePre = tridentData.limePre;
        healEffect = tridentData.healEffect;
        healEffectInstance = tridentData.healEffectInstance;
        singInterval = tridentData.singInterval;



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
                currentCharacter.skill3Energy--;
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

        currentCharacter.skill1Energy--;
        {
            Transform mainCamera = Camera.main.transform;
            CmdLime(mainCamera.position + mainCamera.forward, mainCamera.forward);
        }
    }



    [Command]
    public void CmdLime(Vector3 pos, Vector3 dir)
    {
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

            currentCharacter.skill2Energy--;
            abilityController.BeBird();
        }
        else if(abilityController.currentForm == AbilityController.PlayerForm.Bird)
        {
            abilityController.BeHuman();
        }
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





