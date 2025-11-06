using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class RpcRelay : NetworkBehaviour
{

    public List<CharacterSkills> characterList = new List<CharacterSkills>();

    [ClientRpc]
    public void RpcSetPlayersRole(bool isAttacker, GameObject player)
    {
        if (RoundManager.rm != null)
        {
            if (isAttacker)
            {
                RoundManager.rm.attacker = player;
            }
            else
            {
                RoundManager.rm.defender = player;
            }
        }
    }

    [ClientRpc]
    public void RpcSetCharacter(int index, GameObject player)
    {
        // ‚·‚×‚Ä‚ÌƒLƒƒƒ‰‚ð–³Œø‰»
        foreach (var c in characterList)
        {
            if (c != null)
                c.enabled = false;
        }

        // index‚ÌƒLƒƒƒ‰‚Ì‚Ý—LŒø‰»
        if (index >= 0 && index < characterList.Count)
        {
            var selected = characterList[index];
            selected.enabled = true;
            player.GetComponent<SkillManager>().currentCharacter = selected;
        }
        else
        {
            Debug.LogWarning($"Invalid character index: {index}");
        }
    }

}
