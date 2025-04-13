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

    public int Round = 1;

    // Start is called before the first frame update
    void Awake()
    {       
        rm = this;
        Invoke("StartGetPlayers", 1.5f);
    }

    private void Update()
    {

       
    }

    public void RoundEnd(GameObject loser)
    {
        Round++;

        GameObject winner = myPlayer == loser ? otherPlayer : myPlayer;

        ResetRound();
        RpcResetWeapons();
        GiveCredits(winner, loser);
        GiveRound(winner);
    }



    public void ResetRound()
    {
        RpcResetAllObjects();
        ResetPlayers();
    }


    [ClientRpc]
    public void RpcResetAllObjects()
    {
        // クライアント側でオブジェクトをリセット
    }





    public void ResetPlayers()
    {

        SetPosition();
        SetHp();

    }


    private void StartGetPlayers()
    {
        // 自分のプレイヤーを取得
        myPlayer = PlayerManager.GetLocalPlayer();

        // 相手のプレイヤーを取得
        otherPlayer = PlayerManager.GetOtherPlayer();

        Debug.Log(gameObject,PlayerManager.GetOtherPlayer().transform);

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

    [Server]
    public void GiveCredits(GameObject winner, GameObject loser)
    {
        winner.GetComponent<CreditManager>().ResetCurrentPaying();
        loser.GetComponent<CreditManager>().ResetCurrentPaying();
        winner.GetComponent<CreditManager>().AddCredit(1000 + Round * 300);
        loser.GetComponent<CreditManager>().AddCredit(1000 + Round * 100);

    }

    [Server]
    public void GiveRound(GameObject winner)
    {

        winner.GetComponent<CreditManager>().GiveRound();

    }

    [ClientRpc]
    public void RpcResetWeapons()
    {
        myPlayer.GetComponent<ShootManager>().ResetZoom();
        myPlayer.GetComponent<WeaponManager>().SwitchWeapon(WeaponManager.WeaponType.Lover);

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
