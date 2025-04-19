using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WeaponManager;

public class BuyArmer : MonoBehaviour
{
    public float armer;
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
        if (RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CanBuy(cost, false))
        {
            RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CmdBuyArmer(cost);
            RoundManager.rm.GetMyPlayer().GetComponent<HpMaster>().armer = armer;
        }
    }

}
