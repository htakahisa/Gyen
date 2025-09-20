using UnityEngine;

public class CharacterSelect : MonoBehaviour
{

    public int characterIndex;
    public CharacterManager lobbyPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select()
    {
        CharacterManager[] players = FindObjectsOfType<CharacterManager>();
        foreach (var p in players)
        {
            if (p.isLocalPlayer)
            {
                lobbyPlayer = p;
            }
        }
        lobbyPlayer.CmdSelectCharacter(characterIndex);
    }
}
