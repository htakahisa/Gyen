using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : NetworkBehaviour
{
    public static readonly List<PlayerManager> Players = new List<PlayerManager>();

    public void Update()
    {
        if (!Players.Contains(this))
        {
            Players.Add(this);
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
        return Players.Find(p => p.isLocalPlayer).gameObject;
    }

    public static GameObject GetOtherPlayer()
    {
        GameObject localPlayer = GetLocalPlayer();
        if (localPlayer == null) return null;

        return Players.Find(p => p != localPlayer).gameObject;
    }
}