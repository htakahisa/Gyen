using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopDetailPanel : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescription;

    private void Start()
    {
        HideDetail();
    }

    public void ShowWeaponDetail(Sprite sprite, WeaponDatabase weapon)
    {
        itemImage.sprite = sprite;
        itemNameText.text = weapon.name;
        itemDescription.text = weapon.description;
        gameObject.SetActive(true);
    }
    public void ShowArmerDetail(Sprite sprite, string name, string description)
    {
        itemImage.sprite = sprite;
        itemNameText.text = name;
        itemDescription.text = description;
        gameObject.SetActive(true);
    }

    public void HideDetail()
    {
        gameObject.SetActive(false);
    }
}
