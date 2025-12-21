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
    public TextMeshProUGUI mapNameText;

    public TextMeshProUGUI headShotRate;

    public GameObject winText;
    public GameObject speedrunText;

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
            creditText.text = "Credit : " + RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>()?.credit;
            if (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE)
            {
                myRoundText.text = RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>()?.rounds.ToString();
                enemyRountText.text = RoundManager.rm.GetOtherPlayer().GetComponent<CreditManager>()?.rounds.ToString();
            }
            if (RoundManager.rm.currentMode == RoundManager.Mode.DOUBLETAP)
            {
                myRoundText.text = RoundManager.rm.GetMyPlayer().GetComponent<CreditManager>()?.rounds.ToString();
                enemyRountText.text = RoundManager.rm.GetOtherPlayer().GetComponent<CreditManager>()?.rounds.ToString();
            }
            if (RoundManager.rm.GetMyPlayer().GetComponentInChildren<WeaponManager>() != null)
            {
                magazineText.text = RoundManager.rm.GetMyPlayer().GetComponentInChildren<WeaponManager>()?.magazine.ToString();
                magazineSizeText.text = "/" + RoundManager.rm.GetMyPlayer().GetComponentInChildren<WeaponManager>()?.GetCurrentWeaponStats()?.magazineSize;

                if (RoundManager.rm.currentMode == RoundManager.Mode.PRACTICE)
                {
                    headShotRate.text = "HS% : " + RoundManager.rm.GetMyPlayer().GetComponentInChildren<ServerCheckShoot>()?.GetHeadShotRate();
                }
            }
            if (RoundManager.rm.currentMode != RoundManager.Mode.PRACTICE)
            {
                mapNameText.text = RoundManager.rm.mapName;
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

        if (result == "speedrun")
        {
            resultText = speedrunText;
        }

        resultText.SetActive(true);
        yield return new WaitForSeconds(5f);
        resultText.SetActive(false);

    }

}
