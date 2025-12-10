using System.Collections.Generic;
using UnityEngine;

public class BotSettingPanel : MonoBehaviour
{
    public List<GameObject> children = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND || RoundManager.rm.currentMode == RoundManager.Mode.DOUBLETAP)
        {
            foreach(var child in children)
            {
                child.SetActive(true);
            }
        }
        else
        {
            foreach (var child in children)
            {
                child.SetActive(false);
            }
        }
    }
}
