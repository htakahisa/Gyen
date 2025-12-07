using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseWall : MonoBehaviour
{

    public GameObject phaseWall;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE)
        {
            phaseWall.SetActive(RoundManager.rm.CurrentPhase == RoundManager.Phase.BUY);
        }
        if (RoundManager.rm.currentMode == RoundManager.Mode.DOUBLETAP)
        {
            phaseWall.SetActive(RoundManager.rm.CurrentPhase == RoundManager.Phase.BUY);
        }
        if (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND)
        {
            phaseWall.SetActive(false);
        }
    }
}
