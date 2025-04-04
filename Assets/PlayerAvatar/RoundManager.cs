using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : NetworkBehaviour
{
    private GameObject myPlayer;
    private GameObject otherPlayer;

    public GameObject attacker;
    public GameObject defender;

    private List<GameObject> players = new List<GameObject>();

    public static RoundManager rm;
    public Vector3 attackSpawnPos;
    public Vector3 defenceSpawnPos;

    // Start is called before the first frame update
    void Awake()
    {       
        rm = this;
        Invoke("StartGetPlayers", 2f);
    }

    private void Update()
    {

       
    }


    public void ResetRound()
    {
        RpcResetAllObjects();
        ResetPlayers();
    }


    [ClientRpc]
    public void RpcResetAllObjects()
    {
        // �N���C�A���g���ŃI�u�W�F�N�g�����Z�b�g
    }





    public void ResetPlayers()
    {

        SetPosition();
        SetHp();

    }


    private void StartGetPlayers()
    {
        // �����̃v���C���[���擾
        myPlayer = PlayerManager.GetLocalPlayer();

        // ����̃v���C���[���擾
        otherPlayer = PlayerManager.GetOtherPlayer();

        players.Add(myPlayer);
        players.Add(otherPlayer);

    }

    [Server]
    public void SetPosition()
    {
        attacker.GetComponent<ThirdPersonController>().ResetPos(attackSpawnPos);
        defender.GetComponent<ThirdPersonController>().ResetPos(defenceSpawnPos);
    }


    [Server]
    public void SetHp()
    {

        attacker.GetComponent<HpMaster>().ResetHp();
        defender.GetComponent<HpMaster>().ResetHp();
        
    }



    

    public GameObject GetMyPlayer()
    {
        return myPlayer;
    }

    public GameObject GetOtherPlayer()
    {
        return otherPlayer;
    }
}
