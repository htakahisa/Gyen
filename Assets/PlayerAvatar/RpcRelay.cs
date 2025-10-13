using Mirror;
using UnityEngine;

public class RpcRelay : NetworkBehaviour
{
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

}
