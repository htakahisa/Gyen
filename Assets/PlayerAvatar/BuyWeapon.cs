using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static WeaponStatus;

public class BuyWeapon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CreditManager.PurchaseSlot slot;
    public WeaponType weaponType;
    public WeaponDatabase weapon;
    private Image image;

    public Image iconImage;
    public ShopDetailPanel detailPanel;

    private void Start()
    {
        image = transform.GetChild(0).GetComponent<Image>();
        image.color = Color.gray;
        iconImage = transform.GetChild(3).GetComponent<Image>();
        detailPanel = transform.parent.GetComponentInChildren<ShopDetailPanel>();
    }


    // Start is called before the first frame update
    void Update()
    {
        if (weaponType == RoundManager.rm.GetMyPlayer().GetComponentInChildren<WeaponManager>().mainWeaponType || weaponType == RoundManager.rm.GetMyPlayer().GetComponentInChildren<WeaponManager>().subWeaponType)
        {
            image.color = Color.white;
        }
        else
        {
            image.color = Color.gray;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        detailPanel.ShowWeaponDetail(iconImage.sprite, weapon);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        detailPanel.HideDetail();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Buy();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Sell();
        }
    }

    public void Buy()
    {
        if (RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CanBuy(weapon.cost, slot))       
        {
            RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CmdBuy(weapon.cost, slot);
            RoundManager.rm.GetMyPlayer().GetComponentInChildren<WeaponManager>().CmdBuyWeapon(weaponType);
        }
    }

    public void Sell()
    {

        RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().CmdSell(slot);
        RoundManager.rm.GetMyPlayer().GetComponentInChildren<WeaponManager>().CmdSellWeapon(weaponType);

    }

}
