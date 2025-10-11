using UnityEngine;
using UnityEngine.UI;
using Mirror;


public class BombUI : NetworkBehaviour
{

    public Slider defuseProgressBar; // 0..1


    BombManager bomb;


    void Start()
    {
                
    }


    void Update()
    {
        if (RoundManager.rm.attacker == RoundManager.rm.GetMyPlayer())
        {
            return;
        }
        if (bomb == null)
        {
            defuseProgressBar.value = 0f;
            defuseProgressBar.gameObject.SetActive(false);
            bomb = FindObjectOfType<BombManager>();
        }
        if (bomb == null) return;
        if (bomb.IsArmed)
        {
            defuseProgressBar.value = bomb.DefuseProgress / bomb.DefuseTime;
        }
        else
        {
            defuseProgressBar.value = 0f;
        }
        if (defuseProgressBar.value == 0)
        {
            defuseProgressBar.gameObject.SetActive(false);
        }
        else
        {
            defuseProgressBar.gameObject.SetActive(true);
        }

    }
}