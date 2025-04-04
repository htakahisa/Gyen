using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hpbar : NetworkBehaviour
{
    private Slider hpbar;

    // Start is called before the first frame update
    void Start()
    {
        hpbar = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (RoundManager.rm.GetMyPlayer() != null)
        {
            hpbar.value = RoundManager.rm.GetMyPlayer().GetComponent<HpMaster>().hp;
        }
    }
}
