using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TextManager : MonoBehaviour
{

    public TextMeshProUGUI creditText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI magazineText;
    public TextMeshProUGUI magazineSizeText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (RoundManager.rm.GetMyPlayer() != null)
        {
            creditText.text = "Credit : " + RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().credit;
            if (RoundManager.rm.Mode == "1VS1")
            {
                roundText.text = RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().rounds + " " + RoundManager.rm.GetOtherPlayer().GetComponent<CreditManager>().rounds;
            }
            magazineText.text =  RoundManager.rm.GetMyPlayer().GetComponent<WeaponManager>().magazine.ToString();
            magazineSizeText.text = "/" + RoundManager.rm.GetMyPlayer().GetComponent<WeaponManager>().GetCurrentWeaponData().magazineSize;
        }
    }
}
