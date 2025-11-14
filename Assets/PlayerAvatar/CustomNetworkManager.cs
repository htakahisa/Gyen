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

    public GameObject characterPrefab;

    public Mode selectedMode = Mode.ONEVSONE;
    public bool isFanatics;


    public enum Mode
    {
        ONEVSONE,
        PRACTICE,
        DUELLAND,
    }

    #region Initialization
    public override void Awake()
    {
        base.Awake();
        // NetworkManagerの永続化を保証
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Server Callbacks

    public enum CharacterType
    {
        Trident,
        Ejah,
        Lucifer,
        Overdose,
    }

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

    public void DuelLandConnect(bool isfanatics)
    {
        isFanatics = isfanatics;
        requiredPlayers = 1;
        selectedMode = Mode.DUELLAND;
        Connect();
    }

    public void OneVsOneConnect()
    {
        requiredPlayers = 2;
        selectedMode = Mode.ONEVSONE;
        Connect();
    }

    private void AssignRoles()
    {
        if (pendingConnections.Count > 1)
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
        }
        else
        {
            if (selectedMode == Mode.DUELLAND)
            {
                if (isFanatics)
                {
                    defender = pendingConnections[0];
                }
                else
                {
                    attacker = pendingConnections[0];
                }
            }
            if (selectedMode == Mode.PRACTICE)
            {
                defender = pendingConnections[0];
            }
        }
        Debug.Log($"Roles assigned - Attacker: {attacker?.connectionId}, Defender: {defender?.connectionId}");
    }



    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == "Battle")
        {
            
            // 2. 全プレイヤーをスポーン
            foreach (var conn in pendingConnections)
            {               
                StartCoroutine(ServerSpawnPlayer(conn, sceneName));
            }
        }
        if (sceneName == "Practice")
        {

            // 2. 全プレイヤーをスポーン
            foreach (var conn in pendingConnections)
            {
                StartCoroutine(ServerSpawnPlayer(conn, sceneName));
            }
        }

    }


    #endregion

    #region Player Spawning
    [Server]
    public IEnumerator ServerSpawnPlayer(NetworkConnectionToClient conn, string sceneName)
    {
        yield return new WaitUntil(() => conn.isReady);

        var characterManager = conn.identity.GetComponent<CharacterManager>();
        int index = characterManager.selectedCharacter;

        if (sceneName == "Practice")
        {
            index = 0;
        }

            // スポーン位置決定
            bool isAttacker = (conn == attacker);
        Vector3 spawnPos = isAttacker ?
            RoundManager.rm.attackSpawnPos :
            RoundManager.rm.defenceSpawnPos;

        // プレイヤー生成
        GameObject player = Instantiate(characterPrefab, spawnPos, Quaternion.identity);

        // 置き換え（この時点でクライアントに権限が付く）
        NetworkServer.ReplacePlayerForConnection(conn, player, true);

        // クライアント側の準備完了を待つ
        SpawnTracker tracker = player.GetComponent<SpawnTracker>();
        yield return new WaitUntil(() => tracker.allClientsReady);

        // RpcRelay に登録
        RpcRelay relay = player.GetComponent<RpcRelay>();
        relay.RpcSetPlayersRole(isAttacker, player);
        relay.RpcSetCharacter(index, player);
        RoundManager.rm.currentMode = (RoundManager.Mode)(int)selectedMode;
        RoundManager.rm.isFanatics = isFanatics;

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
        selectedMode = Mode.PRACTICE;
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