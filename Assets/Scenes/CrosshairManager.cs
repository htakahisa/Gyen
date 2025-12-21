using System.Collections.Generic;
using UnityEngine;

public class CrosshairManager : MonoBehaviour
{

    public List<GameObject> crosshairs = new List<GameObject>();
    public static int crosshairIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        crosshairIndex = PlayerPrefs.GetInt("CrosshairIndex", 0);
        SetCrosshair();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChooseCrosshair(int index)
    {
        crosshairIndex = index;
        PlayerPrefs.SetInt("CrosshairIndex", crosshairIndex);
    }

    public void SetCrosshair()
    {
        int nowIndex = 0;

        foreach(var crosshair in crosshairs)
        {
            crosshair.SetActive(nowIndex == crosshairIndex);
            nowIndex++;
        }
    }

}
