using Mirror;
using UnityEngine;

public class CharacterManager : NetworkBehaviour
{


    [SyncVar] public int selectedCharacter = -1;

    [Command]
    public void CmdSelectCharacter(int select)
    {
        selectedCharacter = select;
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

}
