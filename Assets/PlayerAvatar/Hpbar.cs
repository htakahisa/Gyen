using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hpbar : NetworkBehaviour
{
    public Slider hpbar;
    public Image barColor;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (RoundManager.rm.GetMyPlayer() != null)
        {
            hpbar.value = RoundManager.rm.GetMyPlayer().GetComponent<HpMaster>().hp;
            if(RoundManager.rm.GetMyPlayer().GetComponent<HpMaster>().armer == 1)
            {
                // â©óŒ (Yellow-Green)
                barColor.color = new Color(0.6f, 1.0f, 0.2f);
            }
            if (RoundManager.rm.GetMyPlayer().GetComponent<HpMaster>().armer == 0.8f)
            {
                // äDêF (Gray)
                barColor.color = new Color(0.5f, 0.5f, 0.5f);
            }
            if (RoundManager.rm.GetMyPlayer().GetComponent<HpMaster>().armer == 0.67f)
            {
                // â©êF (Yellow)
                barColor.color = new Color(1.0f, 1.0f, 0.0f);
            }
        }
    }
}
