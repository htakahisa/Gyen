using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class CustomNetworkManager : NetworkManager
{
    private int playersInLobby = 0;
    public int requiredPlayers = 2; // �K�v�ȃv���C���[��
    public string battleSceneName = "Battle"; // �o�g���V�[����

    public NetworkConnectionToClient defender;
    public NetworkConnectionToClient attacker;

    // �ڑ������N���C�A���g�̃��X�g
    private List<NetworkConnectionToClient> pendingConnections = new List<NetworkConnectionToClient>();

    public void Awake()
    {
#if UNITY_EDITOR

        

#endif
    }


    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        playersInLobby++;
        Debug.Log($"Player connect. Total players: {playersInLobby}");

        pendingConnections.Add(conn);

        if (playersInLobby >= requiredPlayers)
        {
            // �X�|�[���ʒu������
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

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        playersInLobby--;
        pendingConnections.Remove(conn);

        Debug.Log($"Player left. Total players: {playersInLobby}");
    }

    private IEnumerator StartBattle(NetworkConnectionToClient conn)
    {
        Debug.Log("e");
        yield return new WaitForSeconds(2f); // �����ҋ@�i���o�p�j
        // �S�����o�g���V�[���ֈړ�
        ServerChangeScene(battleSceneName);
        yield return new WaitForSeconds(1f);
        ServerSpawnPlayer(conn);
        
    }


    public void ServerSpawnPlayer(NetworkConnectionToClient conn)
    {
        RoundManager rm = RoundManager.rm;

        // �x�[�X���\�b�h���Ăяo���Ȃ����ƂŃf�t�H���g�̓���𖳌���

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

        // �v���C���[���C���X�^���X��
        GameObject player = Instantiate(playerPrefab, spawnPos, spawnRot);

        // �v���C���[���X�|�[�����A�ڑ��Ɋ֘A�t��
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
