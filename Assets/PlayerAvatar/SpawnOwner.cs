using Mirror;

public class SpawnOwner : NetworkBehaviour
{
    [SyncVar] public uint ownerNetId;
    [SyncVar] public bool friendlyFire;

    public bool IsMine()
    {
        if (NetworkClient.connection?.identity == null) return false;
        return NetworkClient.connection.identity.netId == ownerNetId;
    }

    public NetworkIdentity WhoseThis()
    {
        // サーバー側の辞書
        if (NetworkServer.active)
        {
            if (NetworkServer.spawned.TryGetValue(ownerNetId, out NetworkIdentity id))
                return id;
        }

        // クライアント側の辞書
        if (NetworkClient.spawned.TryGetValue(ownerNetId, out NetworkIdentity id2))
            return id2;

        return null;
    }

}
