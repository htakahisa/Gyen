using UnityEngine;

public class CharacterPanelManager : MonoBehaviour
{
    public GameObject characterPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (characterPanel != null)
        {
            characterPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            characterPanel.SetActive(!characterPanel.activeSelf);
        }
    }
}
