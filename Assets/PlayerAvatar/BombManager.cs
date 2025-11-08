using System.Collections;
using System.Collections.Generic;
using Mirror;
using StarterAssets;
using UnityEngine;


public class BombManager : NetworkBehaviour
{
    [Header("Bomb settings")]
    public float roundTime = 90f; // 秒（爆弾が設置されてからの時間）
    public float defuseTime = 5f; // CT が長押しで解除する時間（秒）
    public float defuseRange = 3f; // ディフューズ可能な距離


    [SyncVar(hook = nameof(OnTimerChanged))]
    public float timer;



    [SyncVar(hook = nameof(OnArmedChanged))]
    private bool isArmed = false;


    [SyncVar(hook = nameof(OnDefuseProgressChanged))]
    private float defuseProgress = 0f; // 0..defuseTime


    [SyncVar]
    private bool isDefusing = false; // 誰かが現在ディフューズ中か

    public List<float> countDownIntervals = new List<float> {1, 0.7f, 0.5f };


    private void Start()
    {
        // サーバーのみでタイマー処理を行う
        if (isServer)
        {
            timer = 0f;
        }
    }


    #region Server control


    [Server]
    public void ArmBomb(float startTime = 0f)
    {
        isArmed = true;
        timer = startTime; // 0 からカウントアップでも、roundTime - を使ってカウントダウンにしても良い
                           // 本実装はカウントアップで roundTime に達したら爆発
        StartCoroutine(ServerTimerCoroutine());
        StartCoroutine(CountSoundCoroutine());
    }


    [Server]
    public void DisarmBomb()
    {
        isArmed = false;
        defuseProgress = 0f;
        isDefusing = false;
        ServerOnBombDisarmed();
    }

    [Server]
    IEnumerator ServerTimerCoroutine()
    {
        timer = 0f;
        isArmed = true;

        while (isArmed)
        {
            timer += Time.deltaTime;

            if (timer >= roundTime)
            {
                isArmed = false;
                ServerOnBombExploded();
                yield break;
            }

            yield return null; // フレーム更新
        }
    }

    [Server]
    IEnumerator CountSoundCoroutine()
    {
        while (isArmed)
        {
            int countStep;
            if (timer >= 55)
                countStep = 2;
            else if (timer >= 30)
                countStep = 1;
            else
                countStep = 0;

            if (countStep >= 0)
            {
                AudioManager.Instance.CmdPlaySoundAtPoint(
                    AudioManager.Sounds.SPIKECOUNTDOWN,
                    transform.position,
                    1,
                    30
                );
                yield return new WaitForSeconds(countDownIntervals[countStep]);
            }
            else
            {
                yield return new WaitForSeconds(0.5f); // 判定再チェックまでの待ち
            }
        }
    }


    // クライアントからディフューズ要求 (開始)
    [Command(requiresAuthority = false)]
    public void CmdStartDefuse(NetworkIdentity playerIdentity)
    {
        if (!isArmed) CmdStopDefuse(playerIdentity);
        if (RoundManager.rm.attacker == playerIdentity.gameObject) return;

        var playerObj = playerIdentity.gameObject;

        if (!IsPlayerInRange(playerObj)) CmdStopDefuse(playerIdentity); // 距離チェック

        AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.DEFUSE, playerObj.transform.position, 1, 30);

        // 誰かが既にディフューズ中なら無視
        if (isDefusing && defuseProgress > 0f) CmdStopDefuse(playerIdentity);
        playerObj.GetComponentInChildren<ThirdPersonController>().RpcPraying();
        isDefusing = true;
        // サーバー側でディフューズ進行を開始するコルーチン
        StartCoroutine(ServerDefuseCoroutine(playerIdentity));
    }


    // クライアントからディフューズ停止 (キー放し / 離脱)
    [Command(requiresAuthority = false)]
    public void CmdStopDefuse(NetworkIdentity playerIdentity)
    {
        if (!isArmed) return;
        if (RoundManager.rm.attacker == playerIdentity.gameObject) return;

        var playerObj = playerIdentity.gameObject;


        playerObj.GetComponentInChildren<ThirdPersonController>().RpcEndPraying();

        isDefusing = false;
        defuseProgress = 0f;
    }


    [Server]
    IEnumerator ServerDefuseCoroutine(NetworkIdentity playerIdentity)
    {
        defuseProgress = 0f;


        while (isArmed && isDefusing)
        {
            // プレイヤーがまだ有効で、範囲内で、CTであることを再確認
            if (playerIdentity == null || playerIdentity.gameObject == null) break;
            var playerObj = playerIdentity.gameObject;
            if (RoundManager.rm.attacker == playerIdentity.gameObject) break;
            if (!IsPlayerInRange(playerObj))
            {
                CmdStopDefuse(playerIdentity);
                break;
            }


            // 1 フレーム分増やす（サーバーのタイマーに合わせる）
            yield return null;
            defuseProgress += Time.deltaTime;


            if (defuseProgress >= defuseTime)
            {
                // ディフューズ成功
                DisarmBomb();
                yield break;
            }


            // もし isDefusing が false になったらループは終了する
            if (!isDefusing) break;
        }


        // 中断
        defuseProgress = 0f;
        isDefusing = false;
    }


    [Server]
    private bool IsPlayerInRange(GameObject player)
    {
        var dist = Vector3.Distance(player.transform.position, transform.position);
        return dist <= defuseRange;
    }


    #endregion


    #region Hooks and RPCs


    void OnTimerChanged(float oldVal, float newVal)
    {
        // クライアント側で UI 更新など
    }


    void OnArmedChanged(bool oldVal, bool newVal)
    {
        // 爆弾が設置された/解除されたをクライアント側で処理
    }


    void OnDefuseProgressChanged(float oldVal, float newVal)
    {
        // クライアント側で UI の進捗バーを更新
    }


    [Server]
    void ServerOnBombExploded()
    {
        Debug.Log("Bomb exploded! Terrorists win.");
        if (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE)
        {
            RoundManager.rm.RoundEnd(RoundManager.rm.defender);
        }
        if (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND)
        {
            RoundManager.rm.DuelLandRetry();
        }
    }


    [Server]
    void ServerOnBombDisarmed()
    {
        Debug.Log("Bomb disarmed! Counter-Terrorists win.");
        if (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE)
        {
            RoundManager.rm.RoundEnd(RoundManager.rm.attacker);
        }
        if (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND)
        {
            RoundManager.rm.DuelLandRetry();
        }
    }


    #endregion


    #region Client helpers


    // クライアントが現在のタイマーを取得するためのプロパティ
    public float Timer => timer;
    public bool IsArmed => isArmed;
    public float DefuseProgress => defuseProgress;
    public float DefuseTime => defuseTime;


    #endregion
}