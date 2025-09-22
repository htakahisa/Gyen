using Mirror;

public class SpawnOwner : NetworkBehaviour
{
    [SyncVar] public uint ownerNetId;

    public bool IsMine()
    {
        if (NetworkClient.connection?.identity == null) return false;
        return NetworkClient.connection.identity.netId == ownerNetId;
    }

    public NetworkIdentity WhoseThis()
    {
        if (NetworkServer.spawned.TryGetValue(ownerNetId, out NetworkIdentity ownerIdentity))
        {
            return ownerIdentity;
        }
        return null;
    }
}
