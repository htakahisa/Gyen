using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditManager : NetworkBehaviour
{

    [SyncVar]
    public int credit = 800;

    [SyncVar]
    public int currentPaying = 0;

    [SyncVar]
    public int rounds = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Server]
    public void AddCredit(int value)
    {
        credit += value;
    }

    [Command]
    public void CmdBuyWeapon(int value)
    {

        credit += currentPaying;
        credit -= value;
        currentPaying = value;
        
    }

    [Server]
    public void GiveRound()
    {
        rounds++;
    }

    public bool CanBuy(int cost)
    {
        return cost <= credit + currentPaying;
    }

    public void ResetCurrentPaying()
    {
        currentPaying = 0;
    }

}
