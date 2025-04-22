using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class CustomNetworkManager : NetworkManager
{
    [Header("Custom Settings")]
    public int requiredPlayers = 2;
    [Scene] public string battleSceneName = "Battle"; // [Scene]属性でビルド設定を強制確認

    [Header("Player References")]
    public NetworkConnectionToClient defender;
    public NetworkConnectionToClient attacker;

    private int playersInLobby = 0;
    private readonly List<NetworkConnectionToClient> pendingConnections = new List<NetworkConnectionToClient>();

    #region Initialization
    public override void Awake()
    {
        base.Awake();
        // NetworkManagerの永続化を保証
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Server Callbacks
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        playersInLobby++;
        pendingConnections.Add(conn);
        Debug.Log($"Player connected (ID: {conn.connectionId}), Total: {playersInLobby}");

        if (playersInLobby >= requiredPlayers)
        {
            AssignRoles();
            StartCoroutine(StartBattleSequence("Battle"));
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

    private IEnumerator StartBattleSequence(string name)
    {
        Debug.Log("Starting battle sequence...");

        // 1. シーン遷移
        ServerChangeScene(name);
        yield return WaitForSceneLoad(name);

        yield return new WaitForSeconds(1f); // スポーン間隔を空ける

        // 2. 全プレイヤーをスポーン
        foreach (var conn in pendingConnections)
        {
            ServerSpawnPlayer(conn);
            yield return new WaitForSeconds(0.5f); // スポーン間隔を空ける
        }
    }

    private IEnumerator WaitForSceneLoad(string sceneName)
    {
        float timeout = Time.time + 10f;
        while (SceneManager.GetActiveScene().name != sceneName)
        {
            if (Time.time > timeout)
            {
                Debug.LogError("Scene load timeout!");
                yield break;
            }
            yield return null;
        }
        Debug.Log($"Scene loaded: {sceneName}");
    }
    #endregion

    #region Player Spawning
    [Server]
    private void ServerSpawnPlayer(NetworkConnectionToClient conn)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned!");
            return;
        }

        // スポーン位置決定
        bool isAttacker = (conn == attacker);
        Vector3 spawnPos = isAttacker ?
            new Vector3(-11.14f, 5.32f, 1f) :
            new Vector3(9f, 2.71f, 0f);

        // プレイヤー生成
        GameObject player = Instantiate(
            playerPrefab,
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
        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log($"Spawned player for connection {conn.connectionId} at {spawnPos}");
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
            StartCoroutine(StartBattleSequence("Practice"));
        }
    }
    #endregion
}