using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public List<MapDatas> oneVsOneMaps = new List<MapDatas>();
    public List<DuelLandDatas> duelLandMaps = new List<DuelLandDatas>();

    public static RoundManager rm;
    public Vector3 attackSpawnPos;
    public Vector3 defenceSpawnPos;
    public Vector3 spikePos;
    public GameObject spike;

    public int Round = 1;
    public Phase CurrentPhase;

    public Mode currentMode;

    public bool hasLoaded = false;

    public BotMove currentBotMove = BotMove.STOP;
    public bool doesBotShoot = true;
    public List<int> spawnedIndex = new List<int>();

    public static List<GameObject> spawns = new List<GameObject>();

    [System.Serializable]
    public struct ObjectAndPosition
    {
        public GameObject prefab;
        public Vector3 position;

        public ObjectAndPosition(GameObject prefab, Vector3 position)
        {
            this.prefab = prefab;
            this.position = position;
        }
    }

    // Inspectorで設定可能にする
    public List<ObjectAndPosition> respawns = new List<ObjectAndPosition>();

    public PlayerManager playerManager;

    public float timeInRound;
    public bool hasRoundEnded;

    public bool hasReset;

    public MapDatas mapData;
    public DuelLandDatas duelLandData;

    public string mapName;

    public Coroutine startGetting;

    public enum Mode
    {
        ONEVSONE,
        PRACTICE,
        DUELLAND,
    }


    // Start is called before the first frame update
    void Awake()
    {
        rm = this;
       
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
                    if (startGetting == null)
                    {
                        playerManager = NetworkClient.localPlayer.gameObject.GetComponent<PlayerManager>();
                        startGetting = StartCoroutine(StartGetPlayers());
                    }
                    
                }
            }

        }
        if (CurrentPhase == Phase.BATTLE)
        {
            timeInRound += Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartCoroutine(ResetPlayers());
        }

    }

    public void RoundEnd(GameObject loser)
    {
        if (hasRoundEnded) return;
        hasRoundEnded = true;
        Round++;

        GameObject winner = myPlayer == loser ? otherPlayer : myPlayer;
        RpcSwitchBuyPhase();
        StartCoroutine(ResetRound());
        GiveCredits(winner, loser);
        GiveRound(winner);

        RpcResultText(loser);
        Invoke("RpcSwitchBattlePhase", 20f);
    }

    public void DuelLandRetry()
    {
        if (hasRoundEnded) return;
        hasRoundEnded = true;
        Round++;

        StartCoroutine(ResetRound());
    }


    public void Finisher(GameObject loser, bool headshot)
    {
        //if (currentMode == Mode.ONEVSONE || currentMode == Mode.PRACTICE)
        {
            GameObject winner = myPlayer == loser ? otherPlayer : myPlayer;
            FinisherManager.instance.PlayPlayerFinisher(winner.GetComponentInChildren<WeaponManager>().GetCurrentWeaponStats(), loser, headshot);
        }
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
        hasRoundEnded = false;
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
            if (timeInRound <= 15)
            {
                result = "speedrun";
            }
            else
            {
                result = "win";
            }
        }
        StartCoroutine(TextManager.textManager.ResultCoroutine(result));
        timeInRound = 0;
    }

    [ClientRpc]
    public void RpcSwitchBattlePhase()
    {
        CurrentPhase = Phase.BATTLE;
    }

    public IEnumerator ResetPractice()
    {

        yield return new WaitForSeconds(0.1f);
        SetConditions();

        yield return new WaitWhile(() => !hasReset);
        hasReset = false;
        ResetStatus();
        ServerResetAllObjects();
        AudioManager.Instance.CmdStopBGM();
    }

    public IEnumerator ResetRound()
    {
        StartCoroutine(ResetPlayers());

        yield return new WaitWhile(() => !hasReset);
        hasReset = false;
        ResetStatus();

        if (currentMode == Mode.DUELLAND)
        {
            hasRoundEnded = false;

            duelLandData = duelLandMaps[Random.Range(0, duelLandMaps.Count)];
            mapName = duelLandData.mapName;

        }
        ServerResetAllObjects();
        AudioManager.Instance.CmdStopBGM();
    }

    [Server]
    public void ServerResetAllObjects()
    {

        // クライアント側でオブジェクトをリセット
        foreach (var spawn in spawns)
        {
            NetworkServer.Destroy(spawn);
        }

        spawnedIndex.Clear();
        for (; spawnedIndex.Count < 4;)
        {
            int index = Random.Range(0, duelLandData.gimmicks.Count);
            var gimmickData = duelLandData.gimmicks[index];
            GameObject gimmick = gimmickData.prefab;

            if (spawnedIndex.Contains(index)) continue;

            GameObject prefab = Instantiate(gimmick, gimmickData.position, Quaternion.Euler(gimmickData.rotation));
            prefab.GetComponent<CharacterTransfromNetwork>().yaw = gimmickData.rotation.y;
            NetworkServer.Spawn(prefab);
            spawns.Add(prefab);
            spawnedIndex.Add(index);
        }

        // クライアント側でオブジェクトをリセット
        foreach (var item in respawns)
        {
            if (item.prefab != null)
            {
                ObjectSpawn(item.prefab, item.position);
                if (item.prefab.GetComponent<CharacterTransfromNetwork>() != null && item.prefab.GetComponent<BotManager>() != null)
                {
                    item.prefab.GetComponent<CharacterTransfromNetwork>().yaw = item.prefab.GetComponent<CharacterTransfromNetwork>().yaw;
                    item.prefab.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, item.prefab.GetComponent<CharacterTransfromNetwork>().yaw, transform.rotation.eulerAngles.z);
                }
            }
            else
            {
                Debug.LogWarning("Prefabが設定されていません。");
            }
        }

        StartCoroutine(BombArmCoroutine());
        

    }

    public IEnumerator BombArmCoroutine()
    {
        if (currentMode == Mode.ONEVSONE)
        {
            yield return new WaitWhile(() => CurrentPhase == Phase.BUY); 
        }
        
        var bombs = FindObjectsOfType<BombManager>();

        if (bombs == null || bombs.Length == 0) {

            yield return new WaitWhile(() => FindObjectsOfType<BombManager>() == null);

            bombs = FindObjectsOfType<BombManager>();
            yield return new WaitWhile(() => bombs == null);
            yield return new WaitWhile(() => bombs.Length == 0);
        }
        bombs[0].ArmBomb();
    }

    public void ObjectSpawn(GameObject prefab, Vector3 pos = default(Vector3))
    {
        ServerSpawn(prefab, pos);
    }

    [Server]
    public void ServerSpawn(GameObject prefab, Vector3 pos)
    {
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity);
        NetworkServer.Spawn(instance);
        spawns.Add(instance);
    }

    public IEnumerator ResetPlayers()
    {

        yield return new WaitForSeconds(4f);



        SetConditions();

    }



    private IEnumerator StartGetPlayers()
    {

        yield return new WaitForSeconds(0.3f);

        CurrentPhase = Phase.BUY;
        if (currentMode == Mode.DUELLAND)
        {
            duelLandData = duelLandMaps[Random.Range(0, duelLandMaps.Count)];
        }
        if (currentMode == Mode.ONEVSONE)
        {
            mapData = oneVsOneMaps[Random.Range(0, oneVsOneMaps.Count)];
            Invoke("RpcSwitchBattlePhase", 15f);
        }

        if (mapData != null)
        {
            attackSpawnPos = mapData.attackerPos;
            defenceSpawnPos = mapData.diffenderPos;
            spikePos = mapData.spikePos;
            mapName = mapData.mapName;
            if (isServer)
            {                
                respawns.Add(new ObjectAndPosition(mapData.mapPrefab, new Vector3(0, 0, 0)));
                respawns.Add(new ObjectAndPosition(spike, spikePos));                
            }
        }

        if (duelLandData != null)
        {
            defenceSpawnPos = duelLandData.diffenderPos;
            spikePos = duelLandData.spikePos;
            currentMode = Mode.DUELLAND;
            mapName = duelLandData.mapName;
            if (isServer)
            {
                if (currentMode == Mode.DUELLAND) 
                { 

                    respawns.Add(new ObjectAndPosition(duelLandData.mapPrefab, new Vector3(0, 0, 0)));
                    respawns.Add(new ObjectAndPosition(spike, spikePos));
                }
            }
        }

    

        // 自分のプレイヤーを取得
        myPlayer = playerManager.GetLocalPlayer();

        if (currentMode == Mode.ONEVSONE)
        {
            // 相手のプレイヤーを取得
            otherPlayer = playerManager.GetOtherPlayer();
            players.Add(otherPlayer);
        }

        players.Add(myPlayer);

        if (isServer)
        {
            if (currentMode == Mode.PRACTICE) 
            { 
                myPlayer.GetComponentInChildren<CreditManager>().credit = 0;
                myPlayer.GetComponentInChildren<CreditManager>().AddCredit(99999);
            }
            if (currentMode == Mode.DUELLAND)
            {
                myPlayer.GetComponentInChildren<CreditManager>().credit = 0;
                myPlayer.GetComponentInChildren<CreditManager>().AddCredit(99999);
            }

            SetConditions();
            ServerResetAllObjects();
            yield return new WaitWhile(() => !hasReset);
            hasReset = false;
            ResetStatus();

        }
        
        hasLoaded = true;
    }

    [Server]
    public void SetConditions()
    {
        StartCoroutine(SetConditionsCoroutine());
    }

    public IEnumerator SetConditionsCoroutine()
    {
        if (attacker != null)
        {
            attacker.GetComponent<AbilityController>().SwitchForm(AbilityController.PlayerForm.Human);

            yield return new WaitWhile(() => attacker.GetComponent<AbilityController>().currentForm != AbilityController.PlayerForm.Human);

            attacker.GetComponentInChildren<ThirdPersonController>().RpcEndPraying();
            attacker.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Move);
            attacker.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Shoot);
            attacker.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Ability);
            attacker.GetComponentInChildren<ThirdPersonController>().ResetPos(attackSpawnPos);
        }
        if (defender != null)
        {
            defender.GetComponent<AbilityController>().SwitchForm(AbilityController.PlayerForm.Human);

            yield return new WaitWhile(() => defender.GetComponent<AbilityController>().currentForm != AbilityController.PlayerForm.Human);

            defender.GetComponentInChildren<ThirdPersonController>().RpcEndPraying();
            defender.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Move);
            defender.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Shoot);
            defender.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Ability);
            defender.GetComponentInChildren<ThirdPersonController>().ResetPos(defenceSpawnPos);
        }

        
    }



    [Server]
    public void GiveCredits(GameObject winner, GameObject loser)
    {
        winner.GetComponent<CreditManager>().ResetCurrentPaying();
        loser.GetComponent<CreditManager>().ResetCurrentPaying();
        winner.GetComponent<CreditManager>().AddCredit(1500 + Round * 300);
        loser.GetComponent<CreditManager>().AddCredit(1000 + Round * 100);

    }

    [Server]
    public void GiveRound(GameObject winner)
    {

        winner.GetComponent<CreditManager>().GiveRound();

    }

    [Server]
    public void ResetStatus()
    {
        myPlayer.GetComponentInChildren<ShootManager>().ResetZoom();
        myPlayer.GetComponentInChildren<ShootManager>().StopAllCoroutines();
        myPlayer.GetComponentInChildren<ShootManager>().isBursting = false;
        myPlayer.GetComponentsInChildren<CharacterSkills>().FirstOrDefault(c => c.enabled).ResetSkill();
        myPlayer.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Move);
        myPlayer.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Shoot);
        myPlayer.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Ability);       
        myPlayer.GetComponentInChildren<ThirdPersonController>().RpcResetAllParameters();
        myPlayer.GetComponentInChildren<ThirdPersonController>().RpcControllerEnabled(true);
        myPlayer.GetComponent<HpMaster>().ResetHp();
        if (currentMode == Mode.ONEVSONE || myPlayer.GetComponentInChildren<WeaponManager>().GetCurrentWeaponSlot() == null)
        {
            myPlayer.GetComponent<HpMaster>().armer = 1;
            myPlayer.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponStatus.WeaponType.Hotaru);
            myPlayer.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponStatus.WeaponType.Lover);
        }
        else
        {
            StartCoroutine(myPlayer.GetComponentInChildren<WeaponManager>().SetMagazineMax(0f));
        }

        if (currentMode == Mode.ONEVSONE)
        {
            otherPlayer.GetComponentInChildren<ShootManager>().ResetZoom();
            otherPlayer.GetComponentInChildren<ShootManager>().StopAllCoroutines();
            otherPlayer.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponStatus.WeaponType.Hotaru);
            otherPlayer.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponStatus.WeaponType.Lover);
            otherPlayer.GetComponentInChildren<ShootManager>().isBursting = false;
            otherPlayer.GetComponentsInChildren<CharacterSkills>().FirstOrDefault(c => c.enabled).ResetSkill();
            otherPlayer.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Move);
            otherPlayer.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Shoot);
            otherPlayer.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Ability);
            otherPlayer.GetComponentInChildren<ThirdPersonController>().RpcResetAllParameters();
            otherPlayer.GetComponentInChildren<ThirdPersonController>().RpcControllerEnabled(true);
            otherPlayer.GetComponent<HpMaster>().ResetHp();
            otherPlayer.GetComponent<HpMaster>().armer = 1;
        }
    }

    [Command (requiresAuthority = false)]
    public void CmdHasReset()
    {
        hasReset = true;
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
