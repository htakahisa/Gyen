using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WeaponManager;

public class BuyWeapon : MonoBehaviour
{
    public WeaponType name;
    public int cost;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Buy()
    {
        if (RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CanBuy(cost, true))       
        {
            RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CmdBuyWeapon(cost);
            RoundManager.rm.GetMyPlayer().GetComponent<WeaponManager>().BuyWeapon(name);
        }
    }

}
