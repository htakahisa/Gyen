using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class CustomNetworkManager : NetworkManager
{
    private int playersInLobby = 0;
    public int requiredPlayers = 2; // 必要なプレイヤー数
    public string battleSceneName = "Battle"; // バトルシーン名

    public NetworkConnectionToClient defender;
    public NetworkConnectionToClient attacker;

    // 接続したクライアントのリスト
    private List<NetworkConnectionToClient> pendingConnections = new List<NetworkConnectionToClient>();



    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn); // この呼び出しを忘れない
        playersInLobby++;
        Debug.Log($"Player connect. Total players: {playersInLobby}");
        Debug.Log($"Player connect for connection {conn.connectionId}");

        pendingConnections.Add(conn);

        if (playersInLobby >= requiredPlayers)
        {
            // スポーン位置を決定
            if(Random.Range(0, 2) == 0)
            {
                defender = pendingConnections[0];
                attacker = pendingConnections[1];
            }
            else
            {
                defender = pendingConnections[1];
                attacker = pendingConnections[0];
            }

            StartCoroutine(StartBattle(pendingConnections[0]));
            StartCoroutine(StartBattle(pendingConnections[1]));
        }
        
    }

    // ホストとしてゲームを開始するメソッド
    public void StartHost()
    {
        NetworkManager networkManager = GetComponent<NetworkManager>();

        // 既に接続されている場合は何もしない
        if (NetworkServer.active || NetworkClient.isConnected)
        {
            Debug.LogWarning("既に接続されています。");
            return;
        }

        // ホストとして開始（サーバーを起動し、ローカルクライアントを接続）
        networkManager.StartHost();

        Debug.Log("ホストとしてゲームを開始しました。");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        playersInLobby--;
        pendingConnections.Remove(conn);

        Debug.Log($"Player left. Total players: {playersInLobby}");
    }

    public void StartPractice()
    {

        StartHost();

        if (playersInLobby == 0)
        {
            return;
        }

        StartCoroutine(StartBattle(pendingConnections[0]));
    }


    private IEnumerator StartBattle(NetworkConnectionToClient conn)
    {
        Debug.Log("e");
        yield return new WaitForSeconds(2f); // 少し待機（演出用）
        ServerSceneChange();
        yield return new WaitForSeconds(1f);
        ServerSpawnPlayer(conn);
        
    }

    [ServerCallback]
    private void ServerSceneChange()
    {
        // 全員をバトルシーンへ移動
        ServerChangeScene(battleSceneName);
    }


    public void ServerSpawnPlayer(NetworkConnectionToClient conn)
    {
        RoundManager rm = RoundManager.rm;

        // ベースメソッドを呼び出さないことでデフォルトの動作を無効化

        Vector3 spawnPos;

        if (conn == attacker)
        {
            spawnPos = new Vector3(-11.13689f, 5.322976f, 1.0f);            
            rm.attackSpawnPos = spawnPos;
        }
        else
        {
            spawnPos = new Vector3(9, 2.708355f, 0);
            rm.defenceSpawnPos = spawnPos;
        }

        
        Quaternion spawnRot = Quaternion.identity;

        // プレイヤーをインスタンス化
        GameObject player = Instantiate(playerPrefab, spawnPos, spawnRot);

        // プレイヤーをスポーンし、接続に関連付け
        NetworkServer.AddPlayerForConnection(conn, player);

        if (conn == attacker)
        {
            rm.attacker = player;
        }
        else
        {
            rm.defender = player;
        }

        Debug.Log($"Player spawned for connection {conn.connectionId}");
        
    }

}
