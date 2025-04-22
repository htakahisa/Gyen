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
        RoundManager roundManager = RoundManager.rm;

        if (roundManager.Mode == "1VS1")
        {

            if (roundManager.GetMyPlayer().GetComponent<CreditManager>().CanBuy(cost, false))
            {
                roundManager.GetMyPlayer().GetComponent<CreditManager>().CmdBuyArmer(cost);
                roundManager.GetMyPlayer().GetComponent<HpMaster>().armer = armer;
            }
        }
        if (roundManager.Mode == "Practice")
        {
            
            roundManager.GetMyPlayer().GetComponent<HpMaster>().armer = armer;
            foreach (var gameObject in RoundManager.rm.GetBots())
            {
                gameObject.GetComponent<HpMaster>().armer = armer;
            }

        }
    }

}
