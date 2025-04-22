using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : NetworkBehaviour
{
    public static readonly List<PlayerManager> Players = new List<PlayerManager>();

    public static bool hasLoaded = false;

    public void Update()
    {
        if (!Players.Contains(this))
        {
            Players.Add(this);
        }

        int needPlayersCount = 100;

        if(RoundManager.rm.Mode == "1VS1")
        {
            needPlayersCount = 2;
        }
        if (RoundManager.rm.Mode == "Practice")
        {
            needPlayersCount = 1;
        }
       
        if(Players.Count >= needPlayersCount)
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

    public static GameObject GetLocalPlayer()
    {
        PlayerManager player = Players.Find(p => p.isLocalPlayer);
        return player != null ? player.gameObject : null;
    }

    public static GameObject GetOtherPlayer()
    {

        GameObject localPlayer = GetLocalPlayer();
        if (localPlayer == null || Players.Count <= 1) return null;
        

        return Players.Find(p => p.gameObject != localPlayer)?.gameObject;
    }
}