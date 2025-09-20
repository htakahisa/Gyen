using Mirror;

public class SpawnOwner : NetworkBehaviour
{
    [SyncVar] public uint ownerNetId;

    public bool IsMine()
    {
        if (NetworkClient.connection?.identity == null) return false;
        return NetworkClient.connection.identity.netId == ownerNetId;
    }

}
