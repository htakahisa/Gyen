using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityGLTF;

public class RoundManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject myPlayer;
    private GameObject otherPlayer;

    public GameObject attacker;
    public GameObject defender;

    public List<GameObject> attackers = new List<GameObject>();
    public List<GameObject> defenders = new List<GameObject>();

    public List<GameObject> dead = new List<GameObject>();

    private List<GameObject> players = new List<GameObject>();

    public List<MapDatas> oneVsOneMaps = new List<MapDatas>();

    public List<MapDatas> doubleTapMaps = new List<MapDatas>();

    public List<DuelLandFanaticsDatas> duelLandFanaticsMaps = new List<DuelLandFanaticsDatas>();
    public List<DuelLandHereticsDatas> duelLandHereticsMaps = new List<DuelLandHereticsDatas>();

    public static RoundManager rm;
    public Vector3 attackSpawnPos;
    public Vector3 defenceSpawnPos;
    public Vector3 attackSpawnRot;
    public Vector3 defenceSpawnRot;
    public Vector3 spikePos;
    public GameObject spike;

    [SyncVar]
    public int Round = 1;
    public Phase CurrentPhase;

    public Mode currentMode;

    public bool hasLoaded = false;

    public BotMove currentBotMove = BotMove.STOP;
    public bool doesBotShoot = true;
    public List<int> spawnedIndex = new List<int>();

    public List<GameObject> spawnedGimmicks = new List<GameObject>();

    public static List<GameObject> spawns = new List<GameObject>();

    [System.Serializable]
    public struct ObjectAndPosition
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;

        public ObjectAndPosition(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
        }
    }

    // Inspectorで設定可能にする
    public List<ObjectAndPosition> respawns = new List<ObjectAndPosition>();

    public PlayerManager playerManager;

    public float timeInRound;
    public bool hasRoundEnded;

    public bool hasReset;

    public MapDatas mapData;
    public DuelLandFanaticsDatas duelLandFanaticsData;
    public DuelLandHereticsDatas duelLandHereticsData;

    Coroutine spawnBotCoroutine;

    public int botCount;

    public string mapName;

    public Coroutine startGetting;
    public bool isFanatics;

    public bool hasMapLoad;

    public GameObject botPrefab;


    public enum Mode
    {
        ONEVSONE,
        PRACTICE,
        DUELLAND,
        DOUBLETAP,
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
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            DuelLandLoad(0);
            ServerResetAllObjects();
            ResetStatus();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DuelLandLoad(1);
            ServerResetAllObjects();
            ResetStatus();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DuelLandLoad(2);
            ServerResetAllObjects();
            ResetStatus();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            DuelLandLoad(3);
            ServerResetAllObjects();
            ResetStatus();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            DuelLandLoad(4);
            ServerResetAllObjects();
            ResetStatus();
        }

    }

    public void RoundEnd(GameObject loser)
    {
        if (hasRoundEnded) return;
        hasRoundEnded = true;
        Round++;

        GameObject winner = myPlayer == loser ? otherPlayer : myPlayer;

        RpcSwitchBuyPhase();
        StartCoroutine(ResetRound(winner == myPlayer));
        GiveCredits(winner, loser);
        GiveRound(winner);

        RpcResultText(loser);
        Invoke("RpcSwitchBattlePhase", 20f);
    }

    public void DuelLandRetry(bool win)
    {
        if (hasRoundEnded) return;
        hasRoundEnded = true;
        Round++;

        StartCoroutine(ResetRound(win));
    }


    public void Finisher(GameObject loser, bool headshot)
    {

        GameObject winner = myPlayer == loser ? otherPlayer : myPlayer;
        FinisherManager.instance.PlayPlayerKillBanner(loser, headshot);
        FinisherManager.instance.PlayPlayerFinisher(winner.GetComponentInChildren<WeaponManager>().GetCurrentWeaponStats(), loser);
        
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
        dead.Clear();
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

    public IEnumerator ResetRound(bool win)
    {
        if (currentMode == Mode.DUELLAND)
        {
            if (isFanatics)
            {
                if (win) 
                {
                    GetComponent<BadgeManager>().ConfirmBadge(duelLandFanaticsData);
                }
                DuelLandLoad(Random.Range(0, duelLandFanaticsMaps.Count));
            }
            else
            {
                if (win)
                {
                    GetComponent<BadgeManager>().ConfirmBadge(duelLandHereticsData);
                }
                DuelLandLoad(Random.Range(0, duelLandHereticsMaps.Count));
            }
            
        }
        StartCoroutine(ResetPlayers());

        yield return new WaitWhile(() => !hasReset);
        hasReset = false;
        ResetStatus();
        ServerResetAllObjects();
        AudioManager.Instance.CmdStopBGM();
    }

    public void DuelLandLoad(int index)
    {
        if (currentMode == Mode.DUELLAND)
        {
            hasRoundEnded = false;
            if (isFanatics)
            {
                duelLandFanaticsData = duelLandFanaticsMaps[index];
                mapName = duelLandFanaticsData.mapName;
            }
            else
            {
                attackSpawnRot = duelLandHereticsData.attackerRot;
                duelLandHereticsData = duelLandHereticsMaps[index];
                mapName = duelLandHereticsData.mapName;
            }
            
        }
    }

    [Server]
    public void ServerResetAllObjects()
    {

        // クライアント側でオブジェクトをリセット
        foreach (var spawn in spawns)
        {
            NetworkServer.Destroy(spawn);
        }

        foreach (var item in respawns)
        {
            if (item.prefab != null)
            {
                ObjectSpawn(item.prefab, item.position);
                if (item.prefab.GetComponent<CharacterTransfromNetwork>() != null && item.prefab.GetComponent<BotManager>() != null)
                {
                    item.prefab.transform.rotation = item.rotation;
                }
            }
            else
            {
                Debug.LogWarning("Prefabが設定されていません。");
            }
        }

        spawnedIndex.Clear();

        int gimmicks = 4;

        if (currentMode == Mode.DUELLAND)
        {
            if (isFanatics)
            {
                if (duelLandFanaticsData.gimmicks.Count < 4)
                {
                    gimmicks = duelLandFanaticsData.gimmicks.Count;
                }
                for (; spawnedIndex.Count < gimmicks;)
                {
                    int index = Random.Range(0, duelLandFanaticsData.gimmicks.Count);
                    var gimmickData = duelLandFanaticsData.gimmicks[index];
                    GameObject gimmick = gimmickData.prefab;

                    if (spawnedIndex.Contains(index)) continue;

                    GameObject prefab = Instantiate(gimmick, gimmickData.position, Quaternion.Euler(gimmickData.rotation));
                    prefab.GetComponent<BotManager>().weapon = gimmickData.weapon;
                    prefab.GetComponent<BotManager>().armer = gimmickData.armer;
                    prefab.GetComponent<BotManager>().foundDelayTime = gimmickData.foundDelayTime;
                    prefab.GetComponent<SpawnOwner>().ownerNetId = 12345;
                    NetworkServer.Spawn(prefab);
                    spawnedGimmicks.Add(prefab);
                    spawns.Add(prefab);
                    spawnedIndex.Add(index);
                }
                foreach (var standing in duelLandFanaticsData.standingGimmicks)
                {
                    GameObject gimmick = standing.prefab;
                    GameObject prefab = Instantiate(gimmick, standing.position, Quaternion.Euler(standing.rotation));
                    if (prefab.GetComponent<BotManager>() != null)
                    {
                        prefab.GetComponent<BotManager>().weapon = standing.weapon;
                        prefab.GetComponent<BotManager>().armer = standing.armer;
                        prefab.GetComponent<BotManager>().foundDelayTime = standing.foundDelayTime;
                        prefab.GetComponent<SpawnOwner>().ownerNetId = 12345;
                    }
                    if(prefab.GetComponent<DestroyTimer>() != null)
                    {
                        prefab.GetComponent<DestroyTimer>().time = 0;
                    }
                    NetworkServer.Spawn(prefab);
                    spawnedGimmicks.Add(prefab);
                    spawns.Add(prefab);
                }
                // クライアント側でオブジェクトをリセット
                foreach (var item in respawns)
                {
                    if (item.prefab != null)
                    {
                        ObjectSpawn(item.prefab, item.position);
                        if (item.prefab.GetComponent<CharacterTransfromNetwork>() != null && item.prefab.GetComponent<BotManager>() != null)
                        {
                            item.prefab.transform.rotation = item.rotation;
                            item.prefab.GetComponent<SpawnOwner>().ownerNetId = 12345;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Prefabが設定されていません。");
                    }
                }
            }
            else
            {
                if (duelLandHereticsData.gimmicks.Count < 4)
                {
                    gimmicks = duelLandHereticsData.gimmicks.Count;
                }
                if (spawnBotCoroutine != null)
                {
                    StopCoroutine(spawnBotCoroutine);
                }
                spawnBotCoroutine = StartCoroutine(SpawnBotCoroutine(gimmicks));
            }
        }


        if (!Application.isEditor)
        {
            StartCoroutine(RecordRoutine());
        }

        StartCoroutine(WaitMapLoad());

    }

    public IEnumerator WaitMapLoad()
    {
        yield return new WaitForSeconds(0.1f);
        hasMapLoad = true;
    }

    public IEnumerator SpawnBotCoroutine(int gimmicks)
    {

        botCount = gimmicks;
        for (; spawnedIndex.Count < gimmicks;)
        {

            int index = Random.Range(0, duelLandHereticsData.gimmicks.Count);
            var gimmickData = duelLandHereticsData.gimmicks[index];
            GameObject gimmick = gimmickData.prefab;

            if (spawnedIndex.Contains(index)) continue;

            yield return new WaitWhile(() => botCount + spawnedIndex.Count != gimmicks);

            foreach (var spawn in spawns)
            {
                NetworkServer.Destroy(spawn);
            }

            foreach (var item in respawns)
            {
                if (item.prefab != null)
                {
                    ObjectSpawn(item.prefab, item.position);
                    if (item.prefab.GetComponent<CharacterTransfromNetwork>() != null && item.prefab.GetComponent<BotManager>() != null)
                    {
                        item.prefab.transform.rotation = item.rotation;
                        item.prefab.GetComponent<SpawnOwner>().ownerNetId = 12345;
                        item.prefab.GetComponent<CharacterTransfromNetwork>().SetRotation(item.rotation.y);
                    }

                }
                else
                {
                    Debug.LogWarning("Prefabが設定されていません。");
                }
            }

            yield return new WaitForSeconds(duelLandHereticsData.waitForSecondsList[spawnedIndex.Count]);

            foreach (var standing in duelLandHereticsData.standingGimmicks)
            {
                GameObject stand = standing.prefab;
                GameObject prefab = Instantiate(stand, standing.position, Quaternion.Euler(standing.rotation));
                if (prefab.GetComponent<BotManager>() != null)
                {
                    prefab.GetComponent<BotManager>().weapon = standing.weapon;
                    prefab.GetComponent<BotManager>().armer = standing.armer;
                    prefab.GetComponent<BotManager>().foundDelayTime = standing.foundDelayTime;
                    standing.prefab.GetComponent<CharacterTransfromNetwork>().SetRotation(standing.rotation.y);
                }
                if (prefab.GetComponent<DestroyTimer>() != null)
                {
                    prefab.GetComponent<DestroyTimer>().time = 0;
                }
                prefab.GetComponent<SpawnOwner>().ownerNetId = 12345;
                NetworkServer.Spawn(prefab);
                spawnedGimmicks.Add(prefab);
                spawns.Add(prefab);
            }

            GameObject bot = Instantiate(gimmick, gimmickData.position, Quaternion.Euler(gimmickData.rotation));
            bot.GetComponent<BotManager>().weapon = gimmickData.weapon;
            bot.GetComponent<BotManager>().armer = gimmickData.armer;
            bot.GetComponent<BotManager>().moveVector = gimmickData.moveVector;
            bot.GetComponent<BotManager>().moveAsScriptTime = gimmickData.moveAsScriptTime;
            bot.GetComponent<BotManager>().movingAsScript = gimmickData.movingAsScript;
            bot.GetComponent<BotManager>().foundDelayTime = gimmickData.foundDelayTime;
            bot.GetComponent<CharacterTransfromNetwork>().SetRotation(gimmickData.rotation.y);
            bot.GetComponent<SpawnOwner>().ownerNetId = 12345;
            NetworkServer.Spawn(bot);
            spawnedGimmicks.Add(bot);
            spawns.Add(bot);
            spawnedIndex.Add(index);

            foreach (var gimmickWithBotData in duelLandHereticsData.gimmicksWithBot)
            {
                if (gimmickWithBotData.withIndex != index) continue;

                //probabilityが90なら、90~99の10通りにおいてcontinueで通過しない。つまり100分の90通りにおいて通過するため90%で通過する。
                if (Random.Range(0, 100) >= gimmickWithBotData.probability) continue;

                GameObject gimmickWithBot = gimmickWithBotData.prefab;
                GameObject with = Instantiate(gimmickWithBot, gimmickWithBotData.position, Quaternion.Euler(gimmickWithBotData.rotation));
                with.GetComponent<SpawnOwner>().ownerNetId = 12345;
                NetworkServer.Spawn(with);
                spawnedGimmicks.Add(with);
                spawns.Add(with); 
            }

        }

        yield return new WaitWhile(() => botCount + spawnedIndex.Count != gimmicks);

        foreach (var spawn in spawns)
        {
            NetworkServer.Destroy(spawn);
        }

        foreach (var item in respawns)
        {
            if (item.prefab != null)
            {
                ObjectSpawn(item.prefab, item.position);
                if (item.prefab.GetComponent<CharacterTransfromNetwork>() != null && item.prefab.GetComponent<BotManager>() != null)
                {
                    item.prefab.transform.rotation = item.rotation;
                    item.prefab.GetComponent<CharacterTransfromNetwork>().SetRotation(item.rotation.y);
                }

            }
            else
            {
                Debug.LogWarning("Prefabが設定されていません。");
            }
        }

    }


    public IEnumerator BombArmCoroutine()
    {
        if (currentMode == Mode.ONEVSONE)
        {
            yield return new WaitWhile(() => CurrentPhase == Phase.BUY); 
        }

        if (currentMode == Mode.DOUBLETAP)
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

    public GameObject ObjectSpawn(GameObject prefab, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), bool AddToSpawns = true, bool instantiated = false, NetworkConnectionToClient conn = null)
    {
        if (isServer)
        {
            return ServerSpawn(prefab, pos, rot, AddToSpawns, instantiated, conn);
        }
        else
        {
            GameObject instance;
            if (instantiated)
            {
                instance = prefab;
            }
            else
            {
                instance = Instantiate(prefab, pos, rot);
            }

            NetworkServer.Spawn(instance, conn);
            SpawnsAdd(instance);
            return instance;
        }
    }

    [Server]
    public GameObject ServerSpawn(GameObject prefab, Vector3 pos, Quaternion rot = default(Quaternion), bool AddToSpawns = true, bool instantiated = false, NetworkConnectionToClient conn = null)
    {

        GameObject instance;
        if (instantiated)
        {
            instance = prefab;
        }
        else
        {
            instance = Instantiate(prefab, pos, rot);
        }

        NetworkServer.Spawn(instance, conn);
        if (AddToSpawns)
        {
            spawns.Add(instance);
        }
        return instance;
    }

    [Command]
    public void SpawnsAdd(GameObject prefab)
    {
        spawns.Add(prefab);
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
            if (isFanatics)
            {
                duelLandFanaticsData = duelLandFanaticsMaps[Random.Range(0, duelLandFanaticsMaps.Count)];
            }
            else
            {
                duelLandHereticsData = duelLandHereticsMaps[Random.Range(0, duelLandHereticsMaps.Count)];
            }
        }
        if (currentMode == Mode.ONEVSONE)
        {
            mapData = oneVsOneMaps[Random.Range(0, oneVsOneMaps.Count)];
            Invoke("RpcSwitchBattlePhase", 15f);
        }
        if (currentMode == Mode.DOUBLETAP)
        {
            currentBotMove = BotMove.CROUCH;
            mapData = doubleTapMaps[Random.Range(0, doubleTapMaps.Count)];
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
                respawns.Add(new ObjectAndPosition(mapData.mapPrefab, new Vector3(0, 0, 0), Quaternion.identity));
                respawns.Add(new ObjectAndPosition(spike, spikePos, Quaternion.identity));                
            }
        }

        if (duelLandFanaticsData != null)
        {
            defenceSpawnPos = duelLandFanaticsData.defenderPos;
            spikePos = duelLandFanaticsData.spikePos;
            currentMode = Mode.DUELLAND;
            mapName = duelLandFanaticsData.mapName;
            currentBotMove = BotMove.CROUCH;
            if (isServer)
            {
                if (currentMode == Mode.DUELLAND) 
                { 

                    respawns.Add(new ObjectAndPosition(duelLandFanaticsData.mapPrefab, new Vector3(0, 0, 0), Quaternion.identity));
                    respawns.Add(new ObjectAndPosition(spike, spikePos, Quaternion.identity));
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
        if (currentMode == Mode.DOUBLETAP)
        {
            // 相手のプレイヤーを取得
            otherPlayer = playerManager.GetOtherPlayer();
            players.Add(otherPlayer);
        }


        if (isServer)
        {
            if (currentMode == Mode.DOUBLETAP)
            {
                NetworkIdentity attNetId = attacker.GetComponent<NetworkIdentity>();
                NetworkConnectionToClient attConn = attNetId.connectionToClient;
                var attackerBot = ObjectSpawn(botPrefab, attackSpawnPos, Quaternion.Euler(attackSpawnRot), false, false, attConn);
                attackerBot.GetComponent<SpawnOwner>().ownerNetId = attacker.GetComponent<NetworkIdentity>().netId;
                attackers.Add(attackerBot);

                NetworkIdentity defNetId = defender.GetComponent<NetworkIdentity>();
                NetworkConnectionToClient defConn = defNetId.connectionToClient;
                var defenderBot = ObjectSpawn(botPrefab, defenceSpawnPos, Quaternion.Euler(defenceSpawnRot), false, false, defConn);
                defenderBot.GetComponent<SpawnOwner>().ownerNetId = defender.GetComponent<NetworkIdentity>().netId;

                defenders.Add(defenderBot);
            }
        }

        players.Add(myPlayer);
        if (attacker != null)
        {
            attackers.Add(attacker);
        }
        if (defender != null)
        {
            defenders.Add(defender);
        }

        if (isServer)
        {
            foreach (var attack in attackers)
            {
                attack.GetComponent<SpawnOwner>().ownerNetId = attacker.GetComponent<NetworkIdentity>().netId; 
            }
            foreach (var defence in defenders)
            {
                defence.GetComponent<SpawnOwner>().ownerNetId = defender.GetComponent<NetworkIdentity>().netId;
            }
        }

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
            yield return new WaitWhile(() => !hasReset);
            ServerResetAllObjects();
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

        if (duelLandHereticsData != null)
        {
            attackSpawnPos = duelLandHereticsData.attackerPos;
            attackSpawnRot = duelLandHereticsData.attackerRot;
            spikePos = duelLandHereticsData.spikePos;
            currentMode = Mode.DUELLAND;
            mapName = duelLandHereticsData.mapName;
            currentBotMove = BotMove.CROUCH;
            if (!hasMapLoad) {

                if (isServer)
                {
                    if (currentMode == Mode.DUELLAND)
                    {
                        respawns.Add(new ObjectAndPosition(duelLandHereticsData.mapPrefab, new Vector3(0, 0, 0), Quaternion.identity));
                        respawns.Add(new ObjectAndPosition(spike, spikePos, Quaternion.identity));
                    }
                } 
            }
        }

        foreach (var attacker in attackers)
        {
            if (attacker != null)
            {
                attacker.GetComponent<AbilityController>().SwitchForm(AbilityController.PlayerForm.Human);

                yield return new WaitWhile(() => attacker.GetComponent<AbilityController>().currentForm != AbilityController.PlayerForm.Human);

                attacker.GetComponentInChildren<ThirdPersonController>().RpcEndPraying();
                attacker.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Move);
                attacker.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Shoot);
                attacker.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Ability);
                Vector3 spawnPos = attackSpawnPos;
                if(attacker.GetComponent<BotManager>() != null)
                {
                    spawnPos.y += 30;
                    attacker.GetComponentInChildren<ShootManager>().hasFound = false;
                }
                attacker.GetComponentInParent<CharacterTransfromNetwork>().SetSynchronize(false);
                attacker.GetComponentInChildren<ThirdPersonController>().ResetPos(spawnPos, attackSpawnRot);
                yield return new WaitWhile(() => attacker.transform.rotation != Quaternion.Euler(attackSpawnRot));
                attacker.GetComponentInParent<CharacterTransfromNetwork>().SetSynchronize(true);

            }
        }
        foreach (var defender in defenders)
        {
            if (defender != null)
            {
                defender.GetComponent<AbilityController>().SwitchForm(AbilityController.PlayerForm.Human);

                yield return new WaitWhile(() => defender.GetComponent<AbilityController>().currentForm != AbilityController.PlayerForm.Human);

                defender.GetComponentInChildren<ThirdPersonController>().RpcEndPraying();
                defender.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Move);
                defender.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Shoot);
                defender.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Ability);
                Vector3 spawnPos = defenceSpawnPos;
                if (defender.GetComponent<BotManager>() != null)
                {
                    spawnPos.y += 30;
                    defender.GetComponentInChildren<ShootManager>().hasFound = false;
                }
                defender.GetComponentInParent<CharacterTransfromNetwork>().SetSynchronize(false);
                defender.GetComponentInChildren<ThirdPersonController>().ResetPos(spawnPos, defenceSpawnRot);
                yield return new WaitWhile(() => defender.transform.rotation != Quaternion.Euler(defenceSpawnRot));
                defender.GetComponentInParent<CharacterTransfromNetwork>().SetSynchronize(true);
            }
        }


    }
    IEnumerator RecordRoutine()
    {
        if (currentMode == Mode.DUELLAND)
        {
            MatchRecorder recorder = GetComponent<MatchRecorder>();
            yield return recorder.StopRecordingAndWait().AsCoroutine();
            recorder.ClearCamera();
            recorder.StartRecording();

        }
    }


        [Server]
    public void GiveCredits(GameObject winner, GameObject loser)
    {
        winner.GetComponent<CreditManager>().ResetCurrentPaying();
        loser.GetComponent<CreditManager>().ResetCurrentPaying();
        winner.GetComponent<CreditManager>().AddCredit(1000 + Round * 700);
        loser.GetComponent<CreditManager>().AddCredit(1000 + Round * 500);

    }

    [Server]
    public void GiveRound(GameObject winner)
    {

        winner.GetComponent<CreditManager>().GiveRound();

    }

    [Server]
    public void ResetStatus()
    {

        List<GameObject> players = new List<GameObject>();

        foreach (var bot in GetBots())
            players.Add(bot);
        if (GetMyPlayer() != null)
        {
            players.Add(GetMyPlayer());
        }
        if (GetOtherPlayer() != null)
        {
            players.Add(GetOtherPlayer());
        }

        foreach (var player in players)
        {
            player.GetComponentInChildren<ShootManager>().ResetZoom();
            player.GetComponentInChildren<ShootManager>().StopAllCoroutines();
            player.GetComponentInChildren<ShootManager>().isBursting = false;
            if (player.GetComponentsInChildren<CharacterSkills>().FirstOrDefault(c => c.enabled) != null)
            {
                player.GetComponentsInChildren<CharacterSkills>().FirstOrDefault(c => c.enabled).ResetSkill();
            }
            player.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Move);
            player.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Shoot);
            player.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Ability);
            player.GetComponentInChildren<ThirdPersonController>().RpcResetAllParameters();
            player.GetComponentInChildren<ThirdPersonController>().RpcControllerEnabled(true);
            player.GetComponent<HpMaster>().ResetHp();
            player.GetComponentInChildren<ShootManager>().StopFoundDelay();

            if (currentMode == Mode.ONEVSONE || currentMode == Mode.DOUBLETAP || player.GetComponentInChildren<WeaponManager>().GetCurrentWeaponSlot() == null)
            {
                player.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponStatus.WeaponType.Hotaru);
                player.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(WeaponStatus.WeaponType.Lover);
                player.GetComponent<HpMaster>().armer = 1;
            }
            else
            {
                StartCoroutine(ResetMagazine(player));
            }
        }

        StartCoroutine(BombArmCoroutine());

    }

    public void SetMode(int index)
    {
        RpcSetMode(index);
    }

    [ClientRpc]
    public void RpcSetMode(int selectedMode)
    {
        currentMode = (Mode)selectedMode;
    }

    public IEnumerator ResetMagazine(GameObject player)
    {
        var currentType = player.GetComponentInChildren<WeaponManager>().GetCurrentWeaponType();
        player.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(player.GetComponentInChildren<WeaponManager>().mainWeaponType);
        yield return null;
        player.GetComponentInChildren<WeaponManager>().CmdBuyWeapon(player.GetComponentInChildren<WeaponManager>().subWeaponType);
        yield return null;
        player.GetComponentInChildren<WeaponManager>().CmdSwitchWeapon(currentType);
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

        foreach (var bot in FindObjectsByType<BotManager>(FindObjectsSortMode.None))
        {
            botsList.Add(bot.gameObject);
        }

        return botsList;
    }

    public List<GameObject> GetMyBots(NetworkIdentity player = null)
    {
        List<GameObject> botsList = new List<GameObject>();

        NetworkIdentity identity = player;
        if(identity == null)
        {
            identity = GetMyPlayer().GetComponent<NetworkIdentity>();
        }

        foreach (var bot in FindObjectsByType<BotManager>(FindObjectsSortMode.None))
        {
            if (bot.GetComponent<SpawnOwner>().WhoseThis() != identity)
                continue;

            botsList.Add(bot.gameObject);
        }

        return botsList;
    }
    public List<GameObject> GetOtherBots(NetworkIdentity player = null)
    {
        List<GameObject> botsList = new List<GameObject>();

        NetworkIdentity identity = player;
        if (identity == null)
        {
            identity = GetMyPlayer().GetComponent<NetworkIdentity>();
        }

        foreach (var bot in FindObjectsByType<BotManager>(FindObjectsSortMode.None))
        {
            if (bot.GetComponent<SpawnOwner>().WhoseThis() == identity)
                continue; // break → continue に変更（マジで重要）

            botsList.Add(bot.gameObject);
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

    public void AddBotCount(int i)
    {
        botCount += i;
        if(botCount <= 0)
        {
            DuelLandRetry(true);
        }
    }

    public bool IsGameObjectSpawnedAsGimmick(GameObject obj)
    {
        return spawnedGimmicks.Contains(obj);    
    }

    public void PlayerDead(bool attacker, GameObject obj)
    {
        dead.Add(obj);
        if (!attacker)
        {
            if (IsDefendersAllDead())
            {
                RoundEnd(defender);
            }
        }
    }

    private bool IsDefendersAllDead()
    {
        // attackers が 1つでも dead に入っていないなら false
        foreach (var a in defenders)
        {
            if (!dead.Contains(a))
            {
                return false;
            }
        }

        // 全員 dead に入っていたら attackers の負け
        return true;
    }


    public enum Phase
    {
        BUY,
        BATTLE,
    }

}
