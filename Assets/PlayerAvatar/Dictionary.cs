using UnityEngine;

public class Dictionary : MonoBehaviour
{
    public GameObject dictionary;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenDictionary()
    {
        dictionary.SetActive(!dictionary.activeSelf);
    }
}
