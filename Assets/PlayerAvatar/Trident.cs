using Mirror;
using UnityEngine;

public class Trident : CharacterSkills
{
    public AbilityController abilityController;
    public GameObject limepre;
    public CharacterSkills currentCharacter;

    private void Awake()
    {
        Skill1 = Lime;
        Skill2 = Yellow;
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
            if (currentCharacter.skill1Energy <= 0)
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
}





