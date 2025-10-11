using System.Collections;
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
    private float timer;


    [SyncVar(hook = nameof(OnArmedChanged))]
    private bool isArmed = false;


    [SyncVar(hook = nameof(OnDefuseProgressChanged))]
    private float defuseProgress = 0f; // 0..defuseTime


    [SyncVar]
    private bool isDefusing = false; // 誰かが現在ディフューズ中か


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
        while (isArmed)
        {
            yield return new WaitForSeconds(1f);
            timer += 1f;
            if (timer >= roundTime)
            {
                // 爆発 — テロリストの勝利
                isArmed = false;
                ServerOnBombExploded();
                // ゲームの勝敗処理は別コンポーネントで行っても良い
                yield break;
            }
        }
    }


    // クライアントからディフューズ要求 (開始)
    [Command(requiresAuthority = false)]
    public void CmdStartDefuse(NetworkIdentity playerIdentity)
    {
        if (!isArmed) return;
        if (RoundManager.rm.attacker == playerIdentity.gameObject) return;

        var playerObj = playerIdentity.gameObject;

        if (!IsPlayerInRange(playerObj)) return; // 距離チェック

        AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.DEFUSE, playerObj.transform.position, 1);

        // 誰かが既にディフューズ中なら無視
        if (isDefusing && defuseProgress > 0f) return;
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

        if (!IsPlayerInRange(playerObj)) return; // 距離チェック


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
        RoundManager.rm.RoundEnd(RoundManager.rm.defender);
    }


    [Server]
    void ServerOnBombDisarmed()
    {
        Debug.Log("Bomb disarmed! Counter-Terrorists win.");
        RoundManager.rm.RoundEnd(RoundManager.rm.attacker);
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