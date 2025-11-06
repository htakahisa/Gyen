using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour
{
    public static readonly List<PlayerManager> Players = new List<PlayerManager>();

    public bool hasLoaded = false;









    public void Update()
    {
        if (!isLocalPlayer)
        {
            if(GetComponent<PlayerInput>() != null)
            {
                GetComponent<PlayerInput>().enabled = false;
            }
        }
        if (!Players.Contains(this))
        {
            Players.Add(this);
        }

        int needPlayersCount = 100;

        if (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE)
        {
            needPlayersCount = 2;
        }
        if (RoundManager.rm.currentMode == RoundManager.Mode.PRACTICE)
        {
            needPlayersCount = 1;
        }
        if (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND)
        {
            needPlayersCount = 1;
        }

        Players.RemoveAll(item => item == null);

        if (Players.Count >= needPlayersCount && !hasLoaded)
        {
            hasLoaded = true;
        }



    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (Players.Contains(this))
        {
            Players.Remove(this);
        }
    }

    public GameObject GetLocalPlayer()
    {
        PlayerManager player = Players.Find(p => p.isLocalPlayer);
        return player != null ? player.gameObject : null;
    }

    public GameObject GetOtherPlayer()
    {

        GameObject localPlayer = GetLocalPlayer();
        if (localPlayer == null || Players.Count <= 1) return null;
        

        return Players.Find(p => p.gameObject != localPlayer)?.gameObject;
    }


}