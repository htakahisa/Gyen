using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SpawnTracker : NetworkBehaviour
{
    // 全クライアントがスポーン完了したらtrueになる変数
    [SyncVar]
    public bool allClientsReady = false;

    // サーバー側でクライアントごとの準備状態を管理
    private Dictionary<NetworkConnection, bool> clientReady = new Dictionary<NetworkConnection, bool>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        // サーバーに接続中の全クライアントを初期化
        foreach (NetworkConnection conn in NetworkServer.connections.Values)
        {
            clientReady[conn] = false;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // 自分がスポーンしたらサーバーに通知
        if (isClient)
        {
            CmdNotifyServerReady();
        }
    }

    // クライアントからサーバーに「スポーン完了」を通知
    [Command(requiresAuthority = false)]
    private void CmdNotifyServerReady(NetworkConnectionToClient sender = null)
    {
        if (!clientReady.ContainsKey(sender)) clientReady[sender] = true;
        else clientReady[sender] = true;

        // 全クライアントの状態をチェック
        CheckAllClientsReady();
    }

    [Server]
    private void CheckAllClientsReady()
    {
        foreach (bool ready in clientReady.Values)
        {
            if (!ready) return; // まだ準備完了していないクライアントあり
        }

        allClientsReady = true; // 全員準備完了
        Debug.Log($"All clients ready for {gameObject.name}");
    }
}
