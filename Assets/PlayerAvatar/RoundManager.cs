using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : NetworkBehaviour
{
    [SerializeField]
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

    public List<GameObject> respawns = new List<GameObject>();

    public PlayerManager playerManager;

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
        RUN,
        JUMP,
        CROUCH
    }


    private void Update()
    {
        if (!hasLoaded)
        {
            if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.gameObject.GetComponent<PlayerManager>() != null)
            {
                if (NetworkClient.localPlayer.gameObject.GetComponent<PlayerManager>().hasLoaded)
                {
                    playerManager = NetworkClient.localPlayer.gameObject.GetComponent<PlayerManager>();
                    StartCoroutine(StartGetPlayers());
                    
                }
            }
        }
    }

    public void RoundEnd(GameObject loser)
    {
        Round++;

        GameObject winner = myPlayer == loser ? otherPlayer : myPlayer;

        FinisherManager.instance.PlayPlayerFinisher(winner.GetComponentInChildren<WeaponManager>().GetCurrentWeaponStats(), loser);
        StartCoroutine(ResetRound());
        GiveCredits(winner, loser);
        GiveRound(winner);

        RpcSwitchBuyPhase();
        RpcResultText(loser);
        Invoke("RpcSwitchBattlePhase", 20f);
    }

    [ClientRpc]
    public void RpcSwitchBuyPhase()
    {
        StartCoroutine(SwitchBuyPhase());
    }

    public IEnumerator SwitchBuyPhase()
    {
        yield return new WaitForSeconds(5f);
        CurrentPhase = Phase.BUY;
    }

    [ClientRpc]
    public void RpcResultText(GameObject loser)
    {
        string result = "";

        if (loser == GetMyPlayer())
        {
            result = "lose";
        }
        else
        {
            result = "win";
        }
        StartCoroutine(TextManager.textManager.ResultCoroutine(result));
    }

    [ClientRpc]
    public void RpcSwitchBattlePhase()
    {
        CurrentPhase = Phase.BATTLE;
    }



    public IEnumerator ResetRound()
    {
        StartCoroutine(ResetPlayers());

        yield return new WaitForSeconds(5f);

        ResetWeapons();
        ServerResetAllObjects();
        AudioManager.Instance.CmdStopBGM();
    }

    [Server]
    public void ServerResetAllObjects()
    {
        // クライアント側でオブジェクトをリセット
        foreach(var spawn in spawns)
        {
            NetworkServer.Destroy(spawn);
        }

        // クライアント側でオブジェクトをリセット
        foreach (var respawns in respawns)
        {
            ServerSpawn(respawns);
        }

    }

    [Server]
    public void ServerSpawn(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab);
        NetworkServer.Spawn(instance);
        spawns.Add(instance);
    }

    public IEnumerator ResetPlayers()
    {
        playerManager.canMove = false;
        myPlayer.GetComponent<CharacterTransfromNetwork>().isSynchronize = false;
        myPlayer.GetComponentInChildren<ShootManager>().canShoot = false;

        if (otherPlayer != null)
        {
            otherPlayer.GetComponent<PlayerManager>().canMove = false;
            otherPlayer.GetComponent<CharacterTransfromNetwork>().isSynchronize = false;
            otherPlayer.GetComponentInChildren<ShootManager>().canShoot = false;
        }

        yield return new WaitForSeconds(5f);

    

        SetPosition();
        SetHp();

    }


    private IEnumerator StartGetPlayers()
    {

        yield return new WaitForSeconds(0.1f);

        // 自分のプレイヤーを取得
        myPlayer = playerManager.GetLocalPlayer();

        // 相手のプレイヤーを取得
        otherPlayer = playerManager.GetOtherPlayer();


        players.Add(myPlayer);
        players.Add(otherPlayer);

        myPlayer.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponStatus.WeaponType.Lover);
        otherPlayer.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponStatus.WeaponType.Lover);
        if (Mode == "Practice")
        {
            myPlayer.GetComponentInChildren<CreditManager>().AddCredit(99999 - 800);
        }
        else
        {
            otherPlayer.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponStatus.WeaponType.Lover);
        }
        hasLoaded = true;
    }

    [Server]
    public void SetPosition()
    {
        if (attacker != null)
        {
            attacker.GetComponentInChildren<ThirdPersonController>().ResetPos(attackSpawnPos);
        }
        if (defender != null)
        {
            defender.GetComponentInChildren<ThirdPersonController>().ResetPos(defenceSpawnPos);
        }
    }


    [Server]
    public void SetHp()
    {

        attacker.GetComponent<HpMaster>().ResetHp();
        defender.GetComponent<HpMaster>().ResetHp();
        attacker.GetComponent<HpMaster>().armer = 1;
        defender.GetComponent<HpMaster>().armer = 1;
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

    [Server]
    public void ResetWeapons()
    {
        myPlayer.GetComponentInChildren<ShootManager>().ResetZoom();
        myPlayer.GetComponentInChildren<ShootManager>().StopAllCoroutines();
        myPlayer.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponStatus.WeaponType.Lover);
        myPlayer.GetComponentInChildren<ShootManager>().isBursting = false;

        myPlayer.GetComponentInChildren<CharacterSkills>().ResetSkill();
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


    [Command]
    public void CmdCallDance()
    {
        GetMyPlayer().GetComponentInChildren<ThirdPersonController>().RpcDance();
    }

    [Command]
    public void CmdCallEndDance()
    {
        GetMyPlayer().GetComponentInChildren<ThirdPersonController>().RpcEndDance();
    }


    public enum Phase
    {
        BUY,
        BATTLE,
    }

}
