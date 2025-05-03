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
    public Phase CurrentPhase;

    public string Mode = "1VS1";

    public bool hasLoaded = false;

    public BotMove currentBotMove = BotMove.STOP;

    public static List<GameObject> spawns = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        rm = this;
        CurrentPhase = Phase.BUY;
        if (SceneManager.GetActiveScene().name == "Battle")
        {
            Mode = "1VS1";
        }
        if (SceneManager.GetActiveScene().name == "Practice")
        {
            Mode = "Practice";
        }
        if (Mode == "1VS1")
        {
            Invoke("RpcSwitchBattlePhase", 15f);
        }


    }

    public enum BotMove 
    {
        STOP,
        WALK,
        RUN
    }


    private void Update()
    {
        if (!hasLoaded && PlayerManager.hasLoaded)
        {
            StartGetPlayers();
            hasLoaded = true;
        }
    }

    public void RoundEnd(GameObject loser)
    {
        Round++;

        GameObject winner = myPlayer == loser ? otherPlayer : myPlayer;

        ResetRound();
        RpcResetWeapons();
        GiveCredits(winner, loser);
        GiveRound(winner);

        RpcSwitchBuyPhase();
        Invoke("RpcSwitchBattlePhase", 15f);

    }

    [ClientRpc]
    public void RpcSwitchBuyPhase()
    {
        CurrentPhase = Phase.BUY;
    }

    [ClientRpc]
    public void RpcSwitchBattlePhase()
    {
        CurrentPhase = Phase.BATTLE;
    }



    public void ResetRound()
    {
        ServerResetAllObjects();
        ResetPlayers();
    }


    public void ServerResetAllObjects()
    {
        // クライアント側でオブジェクトをリセット
        foreach(var spawn in spawns)
        {
            ClientDestroy(spawn);
        }
    }

    [ClientRpc]
    public void ClientDestroy(GameObject instance)
    {
        Destroy(instance);
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


        players.Add(myPlayer);
        players.Add(otherPlayer);

        myPlayer.GetComponent<WeaponManager>().BuyWeapon(WeaponManager.WeaponType.Lover);
        if (Mode == "Practice")
        {
            myPlayer.GetComponent<CreditManager>().AddCredit(99999 - 800);
        }
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
        myPlayer.GetComponent<ShootManager>().StopAllCoroutines();
        myPlayer.GetComponent<WeaponManager>().BuyWeapon(WeaponManager.WeaponType.Lover);
        myPlayer.GetComponent<ShootManager>().isBursting = false;
    }


    public GameObject GetMyPlayer()
    {
        return myPlayer;
    }

    public GameObject GetOtherPlayer()
    {
        return otherPlayer;
    }

    public List<GameObject> GetBots()
    {
        List<GameObject> botsList = new List<GameObject>();

        foreach (var bots in FindObjectsByType<BotManager>(FindObjectsSortMode.None))
        {
            botsList.Add(bots.gameObject);
        }

        return botsList;
    }

    public enum Phase
    {
        BUY,
        BATTLE,
    }

}
