using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static WeaponManager;
using Mirror;

public class BuyArmer : NetworkBehaviour, IPointerClickHandler
{
    public float armer;
    public int cost;

    private Image image;


    private void Start()
    {
        image = transform.parent.GetChild(1).GetComponent<Image>();
        image.color = Color.gray;
    }


    // Update is called once per frame
    void Update()
    {
        if (armer == RoundManager.rm.GetMyPlayer().GetComponent<HpMaster>().armer)
        {
            image.color = Color.white;
        }
        else
        {
            image.color = Color.gray;
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

        if (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE)
        {

            if (RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CanBuy(cost, CreditManager.PurchaseSlot.Armor))
            {
                RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CmdBuy(cost, CreditManager.PurchaseSlot.Armor);
                CmdBuy(RoundManager.rm.GetMyPlayer());
            }
        }
        if (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND)
        {

            if (RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CanBuy(cost, CreditManager.PurchaseSlot.Armor))
            {
                RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CmdBuy(cost, CreditManager.PurchaseSlot.Armor);
                CmdBuy(RoundManager.rm.GetMyPlayer());
            }
        }
        if (RoundManager.rm.currentMode == RoundManager.Mode.PRACTICE)
        {

            RoundManager.rm.GetMyPlayer().GetComponent<HpMaster>().armer = armer;
            foreach (var gameObject in RoundManager.rm.GetBots())
            {
                gameObject.GetComponent<HpMaster>().armer = armer;
            }

        }
    }
        
    [Command(requiresAuthority = false)]
    public void CmdBuy(GameObject player)
    {
        player.GetComponent<HpMaster>().armer = armer;
    }

}
