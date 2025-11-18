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

    private List<GameObject> players = new List<GameObject>();

    public List<MapDatas> oneVsOneMaps = new List<MapDatas>();

    public List<DuelLandFanaticsDatas> duelLandFanaticsMaps = new List<DuelLandFanaticsDatas>();
    public List<DuelLandHereticsDatas> duelLandHereticsMaps = new List<DuelLandHereticsDatas>();

    public static RoundManager rm;
    public Vector3 attackSpawnPos;
    public Vector3 defenceSpawnPos;
    public Vector3 attackSpawnRot;
    public Vector3 defenceSpawnRot;
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
    public DuelLandFanaticsDatas duelLandFanaticsData;
    public DuelLandHereticsDatas duelLandHereticsData;

    Coroutine spawnBotCoroutine;

    public int botCount;

    public string mapName;

    public Coroutine startGetting;
    public bool isFanatics;

    public bool hasMapLoad;
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
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DuelLandLoad(2);
            ServerResetAllObjects();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            DuelLandLoad(3);
            ServerResetAllObjects();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            DuelLandLoad(4);
            ServerResetAllObjects();
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
        if (isFanatics)
        {
            DuelLandLoad(Random.Range(0, duelLandFanaticsMaps.Count));
        }
        else
        {
            DuelLandLoad(Random.Range(0, duelLandHereticsMaps.Count));
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
                    prefab.GetComponent<CharacterTransfromNetwork>().yaw = gimmickData.rotation.y;
                    prefab.GetComponent<BotManager>().weapon = gimmickData.weapon;
                    prefab.GetComponent<BotManager>().armer = gimmickData.armer;
                    NetworkServer.Spawn(prefab);
                    spawns.Add(prefab);
                    spawnedIndex.Add(index);
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
#if !UNITY_EDITOR
            StartCoroutine(RecordRoutine());
#endif

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
            yield return new WaitForSeconds(duelLandHereticsData.waitForSecondsList[spawnedIndex.Count]);


            GameObject prefab = Instantiate(gimmick, gimmickData.position, Quaternion.Euler(gimmickData.rotation));
            prefab.GetComponent<CharacterTransfromNetwork>().yaw = gimmickData.rotation.y;
            prefab.GetComponent<BotManager>().weapon = gimmickData.weapon;
            prefab.GetComponent<BotManager>().armer = gimmickData.armer;
            prefab.GetComponent<BotManager>().moveVector = gimmickData.moveVector;
            prefab.GetComponent<BotManager>().moveAsScriptTime = gimmickData.moveAsScriptTime;
            prefab.GetComponent<BotManager>().movingAsScript = gimmickData.movingAsScript;
            NetworkServer.Spawn(prefab);
            spawns.Add(prefab);
            spawnedIndex.Add(index);
        }
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

        if (duelLandFanaticsData != null)
        {
            defenceSpawnPos = duelLandFanaticsData.defenderPos;
            spikePos = duelLandFanaticsData.spikePos;
            currentMode = Mode.DUELLAND;
            mapName = duelLandFanaticsData.mapName;
            currentBotMove = BotMove.CROUCH;
            currentBotMove = BotMove.CROUCH;
            if (isServer)
            {
                if (currentMode == Mode.DUELLAND) 
                { 

                    respawns.Add(new ObjectAndPosition(duelLandFanaticsData.mapPrefab, new Vector3(0, 0, 0)));
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
                        respawns.Add(new ObjectAndPosition(duelLandHereticsData.mapPrefab, new Vector3(0, 0, 0)));
                        respawns.Add(new ObjectAndPosition(spike, spikePos));
                        hasMapLoad = true;
                    }
                } 
            }
        }
        if (attacker != null)
        {
            attacker.GetComponent<AbilityController>().SwitchForm(AbilityController.PlayerForm.Human);

            yield return new WaitWhile(() => attacker.GetComponent<AbilityController>().currentForm != AbilityController.PlayerForm.Human);

            attacker.GetComponentInChildren<ThirdPersonController>().RpcEndPraying();
            attacker.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Move);
            attacker.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Shoot);
            attacker.GetComponent<PlayerActionLockManager>().ServerRemoveLockAll(PlayerAction.Ability);
            attacker.GetComponentInChildren<ThirdPersonController>().ResetPos(attackSpawnPos);
            attacker.GetComponentInParent<CharacterTransfromNetwork>().yaw = attackSpawnRot.y;
            attacker.GetComponentInParent<CharacterTransfromNetwork>().isSynchronize = false;
            attacker.transform.rotation = Quaternion.Euler(attackSpawnRot);
            attacker.GetComponentInParent<CharacterTransfromNetwork>().isSynchronize = true;
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
            defender.GetComponentInParent<CharacterTransfromNetwork>().yaw = defenceSpawnRot.y;
            defender.transform.rotation = Quaternion.Euler(defenceSpawnRot);
        }


    }
    IEnumerator RecordRoutine()
    {
        if (currentMode == Mode.DUELLAND)
        {
            MatchRecorder recorder = GetComponent<MatchRecorder>();


            yield return recorder.StopRecordingAndWait().AsCoroutine();
            recorder.ClearCamera();


            foreach (var player in players)
            {
                Camera cam = player.GetComponentInChildren<ThirdPersonController>().recordCamera;
                recorder.AddCamera(player.GetComponentInChildren<ThirdPersonController>().recordCamera);
                recorder.AddHiddenObject(cam, player);
            }
            foreach (var bot in GetBots())
            {
                Camera cam = bot.GetComponentInChildren<ThirdPersonController>().recordCamera;
                recorder.AddCamera(bot.GetComponentInChildren<ThirdPersonController>().recordCamera);
                recorder.AddHiddenObject(cam, bot);
            }

            recorder.StartRecording();



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

        if (otherPlayer != null)
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

    public void AddBotCount(int i)
    {
        botCount += i;
        if(botCount <= 0)
        {
            DuelLandRetry();
        }
    }

    public enum Phase
    {
        BUY,
        BATTLE,
    }

}
