using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditManager : NetworkBehaviour
{

    [SyncVar]
    public int credit = 800;

    [SyncVar]
    public int currentWeaponPaying = 0;

    [SyncVar]
    public int currentArmerPaying = 0;

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

        credit += currentWeaponPaying;
        credit -= value;
        currentWeaponPaying = value;
        
    }

    [Command]
    public void CmdBuyArmer(int value)
    {

        credit += currentArmerPaying;
        credit -= value;
        currentArmerPaying = value;

    }

    [Server]
    public void GiveRound()
    {
        rounds++;
    }

    public bool CanBuy(int cost, bool isWeapon)
    {
        return cost <= credit + (isWeapon ? currentWeaponPaying : currentArmerPaying);
    }

    public void ResetCurrentPaying()
    {
        currentWeaponPaying = 0;
        currentArmerPaying = 0;
    }

}
