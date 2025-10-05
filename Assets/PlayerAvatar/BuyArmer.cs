using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static WeaponManager;

public class BuyArmer : MonoBehaviour, IPointerClickHandler
{
    public float armer;
    public int cost;

    private UIGradient image;


    private void Start()
    {
        image = GetComponentInChildren<UIGradient>();
        image.bottomColor = Color.black;
    }


    // Update is called once per frame
    void Update()
    {
        if (armer == RoundManager.rm.GetMyPlayer().GetComponent<HpMaster>().armer)
        {
            image.bottomColor = Color.blue;
        }
        else
        {
            image.bottomColor = Color.black;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Buy();
        }
    }

    public void Buy()
    {
        RoundManager roundManager = RoundManager.rm;

        if (roundManager.Mode == "1VS1")
        {

            if (roundManager.GetMyPlayer().GetComponent<CreditManager>().CanBuy(cost, CreditManager.PurchaseSlot.Armor))
            {
                roundManager.GetMyPlayer().GetComponent<CreditManager>().CmdBuy(cost, CreditManager.PurchaseSlot.Armor);
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
