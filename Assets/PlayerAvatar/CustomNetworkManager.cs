using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class CustomNetworkManager : NetworkManager
{
    [Header("Custom Settings")]
    public int requiredPlayers = 2;
    [Scene] public string loadSceneName = "Loading"; // [Scene]属性でビルド設定を強制確認

    [Header("Player References")]
    public NetworkConnectionToClient defender;
    public NetworkConnectionToClient attacker;

    private int playersInLobby = 0;

    public string targetAddress = "192.168.11.15";
    public ushort port = 7777;

    private readonly List<NetworkConnectionToClient> pendingConnections = new List<NetworkConnectionToClient>();

    public GameObject[] playerPrefabs;

    #region Initialization
    public override void Awake()
    {
        base.Awake();
        // NetworkManagerの永続化を保証
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Server Callbacks


    public void Connect()
    {
        StartCoroutine(TryJoinCoroutine());
    }

    private IEnumerator TryJoinCoroutine()
    {
        // ① まず targetAddress にクライアントで接続を試みる
        networkAddress = targetAddress;
        StartClient();
        yield return new WaitForSeconds(1f); // 接続待ち

        if (NetworkClient.isConnected && !NetworkServer.active)
        {
            Debug.Log("クライアントとして接続成功");
            yield break; // 接続成功したら終了
        }

        // ② localhost に接続を試みる
        StopClient();
        networkAddress = "localhost";
        StartClient();
        yield return new WaitForSeconds(1f);

        if (NetworkClient.isConnected && !NetworkServer.active)
        {
            Debug.Log("localhost にクライアントとして接続成功");
            yield break;
        }

        // ③ どちらも失敗 → 自分がホストになる
        StopClient();
        networkAddress = "localhost";
        StartHost();
        Debug.Log("ホストとして起動");
    }






    bool IsInRoom()
    {
        // クライアントが接続されていて、ネットワークがアクティブであるかを確認
        return NetworkClient.isConnected && NetworkManager.singleton.isNetworkActive;
    }


    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        playersInLobby++;
        pendingConnections.Add(conn);
        Debug.Log($"Player connected (ID: {conn.connectionId}), Total: {playersInLobby}");

        

        if (playersInLobby >= requiredPlayers)
        {
            AssignRoles();
            ServerChangeScene("Loading");
        }
    }



    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        playersInLobby--;
        pendingConnections.Remove(conn);
        Debug.Log($"Player disconnected (ID: {conn.connectionId}), Total: {playersInLobby}");

    }
    #endregion

    #region Game Flow
    private void AssignRoles()
    {
        // ランダムに攻撃側と防御側を決定
        if (Random.Range(0, 2) == 0)
        {
            defender = pendingConnections[0];
            attacker = pendingConnections[1];
        }
        else
        {
            defender = pendingConnections[1];
            attacker = pendingConnections[0];
        }
        Debug.Log($"Roles assigned - Attacker: {attacker.connectionId}, Defender: {defender.connectionId}");
    }



    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == "Battle")
        {
            
            // 2. 全プレイヤーをスポーン
            foreach (var conn in pendingConnections)
            {               
                StartCoroutine(ServerSpawnPlayer(conn));
            }
        }
        if (sceneName == "Practice")
        {
            Vector3 spawnPos = RoundManager.rm.defenceSpawnPos;

            // プレイヤー生成
            GameObject player = Instantiate(
                playerPrefabs[1],
                spawnPos,
                Quaternion.identity
            );
            NetworkServer.AddPlayerForConnection(NetworkServer.localConnection, player);
            
        }
     
    }


    #endregion

    #region Player Spawning
    [Server]
    public IEnumerator ServerSpawnPlayer(NetworkConnectionToClient conn)
    {
        yield return new WaitUntil(() => conn.isReady);

        var characterManager = conn.identity.GetComponent<CharacterManager>();
        int index = characterManager.selectedCharacter;

        // スポーン位置決定
        bool isAttacker = (conn == attacker);
        Vector3 spawnPos = isAttacker ?
            RoundManager.rm.attackSpawnPos :
            RoundManager.rm.defenceSpawnPos;

        // プレイヤー生成
        GameObject player = Instantiate(
            playerPrefabs[index],
            spawnPos,
            Quaternion.identity
        );

        // RoundManagerに参照を登録
        if (RoundManager.rm != null)
        {
            if (isAttacker)
            {
                RoundManager.rm.attacker = player;
                RoundManager.rm.attackSpawnPos = spawnPos;
            }
            else
            {
                RoundManager.rm.defender = player;
                RoundManager.rm.defenceSpawnPos = spawnPos;
            }
        }

        // ネットワーク登録
        NetworkServer.ReplacePlayerForConnection(conn, player, true);
        Debug.Log($"Spawned player for connection {conn.connectionId} at {spawnPos}");
    }
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        // クライアントが準備できたことをサーバーに伝える
        NetworkClient.Ready();
    }
        #endregion

        #region Public Methods
        public void StartHostGame()
    {
        if (NetworkServer.active || NetworkClient.isConnected)
        {
            Debug.LogWarning("Already connected!");
            return;
        }

        StartHost();
        Debug.Log("Host started successfully");
    }

    public void StartPracticeMode()
    {
        StartHost();
        if (playersInLobby > 0)
        {
            ServerChangeScene("Practice");
        }
    }
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();

        // 新しいシーンで準備完了をサーバーに通知
        NetworkClient.Ready();
    }

        #endregion
}