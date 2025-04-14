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
        phaseWall.SetActive(RoundManager.rm.CurrentPhase == RoundManager.Phase.BUY);
    }
}
