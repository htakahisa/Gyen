using Mirror;
using StarterAssets;
using UnityEngine;

public class Trident : CharacterSkills
{
    public AbilityController abilityController;
    public GameObject limepre;
    public CharacterSkills currentCharacter;
    public ShootManager shootManager;
    public HpMaster hpMaster;

    public bool isSinging;
    public float singIntervalTimer;
    public float singInterval;

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
        if (isSinging)
        {
            if (Input.GetMouseButton(0))
            {


                if (currentCharacter.skill3Energy <= 0)
                {
                    return;
                }

                singIntervalTimer += Time.deltaTime;

                if (singIntervalTimer >= singInterval)
                {
                    currentCharacter.skill3Energy --;
                    CmdSingHeal();
                    singIntervalTimer = 0;
                }

            }
            else
            {
                singIntervalTimer = 0;
            }
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
        GameObject instance = Instantiate(limepre, pos, Quaternion.LookRotation(dir));
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
        hpMaster.TakeDamage(-10);
    }

}





