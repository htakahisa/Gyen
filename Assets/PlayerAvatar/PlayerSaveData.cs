using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerSaveData : MonoBehaviour
{

    public TMP_InputField sensitivityField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Save()
    {

        try
        {
            PlayerPrefs.SetFloat("Sensitivity", float.Parse(sensitivityField.text));
            PlayerPrefs.Save();
        }
        catch 
        {

        }


    }




}





