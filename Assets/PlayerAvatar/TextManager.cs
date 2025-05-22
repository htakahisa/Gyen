using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TextManager : MonoBehaviour
{

    public TextMeshProUGUI creditText;
    public TextMeshProUGUI myRoundText;
    public TextMeshProUGUI enemyRountText;
    public TextMeshProUGUI magazineText;
    public TextMeshProUGUI magazineSizeText;

    public TextMeshProUGUI headShotRate;

    public GameObject winText;
    public GameObject loseText;

    public static TextManager textManager;

    // Start is called before the first frame update
    void Awake()
    {
        textManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (RoundManager.rm.GetMyPlayer() != null)
        {
            creditText.text = "Credit : " + RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().credit;
            if (RoundManager.rm.Mode == "1VS1")
            {
                myRoundText.text = RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>().rounds.ToString();
                enemyRountText.text = RoundManager.rm.GetOtherPlayer().GetComponent<CreditManager>().rounds.ToString();
            }
            magazineText.text =  RoundManager.rm.GetMyPlayer().GetComponent<WeaponManager>().magazine.ToString();
            magazineSizeText.text = "/" + RoundManager.rm.GetMyPlayer().GetComponent<WeaponManager>().GetCurrentWeaponData().magazineSize;

            if(RoundManager.rm.Mode == "Practice")
            {
                headShotRate.text = "HS% : " + RoundManager.rm.GetMyPlayer().GetComponent<ServerCheckShoot>().GetHeadShotRate();
            }

        }
    }

    public IEnumerator ResultCoroutine(string result)
    {
        GameObject resultText = null;

        if (result == "win")
        {
            resultText = winText;
        }

        if (result == "lose")
        {
            resultText = loseText;
        }

        resultText.SetActive(true);
        yield return new WaitForSeconds(5f);
        resultText.SetActive(false);

    }

}
