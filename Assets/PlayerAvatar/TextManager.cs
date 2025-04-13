using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TextManager : MonoBehaviour
{

    public TextMeshProUGUI creditText;
    public TextMeshProUGUI roundText;

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
            roundText.text = RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().rounds + " " + RoundManager.rm.GetOtherPlayer().GetComponent<CreditManager>().rounds;
        }
    }
}
