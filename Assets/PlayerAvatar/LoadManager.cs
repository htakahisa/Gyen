using Mirror;
using UnityEngine;

public class LoadManager : MonoBehaviour
{
    CustomNetworkManager customNetworkManager;
    public string battleSceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        customNetworkManager = GameObject.Find("NetworkManagers").GetComponent<CustomNetworkManager>();
        
    }
    private void Update()
    {
        ServerSpawnAllPlayers();
    }

    [Server]
    public void ServerSpawnAllPlayers() 
    { 
        foreach (var conn in NetworkServer.connections.Values)
        {
            var characterManager = conn.identity.GetComponent<CharacterManager>();
            if (characterManager == null || characterManager.selectedCharacter == -1)
                return; // ’N‚©–¢‘I‘ð‚È‚ç‚Ü‚¾‘Ò‚Â
        }
        customNetworkManager.ServerChangeScene(battleSceneName);
        NetworkServer.Destroy(gameObject);
    }

}
