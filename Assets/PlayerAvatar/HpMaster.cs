using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class HpMaster : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHpChanged))]
    public int hp = 100; // 各プレイヤーのHP

    public float armer = 1; // 各プレイヤーのHP
    [SyncVar]
    public bool isInvincible = false;

    [SyncVar]
    public bool isDead = false;

    public EventOnDeath onDeath;

    private void OnHpChanged(int oldValue, int newValue)
    {
        Debug.Log($"{netId} のHPが {oldValue} → {newValue} に変更");
    }

    [Server]
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible)
        {
            return;
        }

        int correctedDamage = (int)(damage * armer);

        if (hp <= 0) return;
        hp -= correctedDamage;
        if (hp <= 0)
        {
            isDead = true;
            hp = 0;
            Debug.Log($"{netId} のオブジェクトが壊された");
            OnDeath();
        }
    }

    [Server]
    public void ResetHp()
    {
        isDead = false;
        hp = 100;

    }

    [Command]
    public void CmdInvincible(bool invincible)
    {
        isInvincible = invincible;

    }

    public void OnDeath()
    {
        if (onDeath == EventOnDeath.LoseRound)
        {
            RoundManager.rm.RoundEnd(gameObject);
        }
        if (onDeath == EventOnDeath.RespawnTarget)
        {
            ResetHp();
            GetComponent<BotManager>().ResetPos();
        }
        if (onDeath == EventOnDeath.Destroy)
        {
            NetworkServer.Destroy(gameObject);
        }
        if (onDeath == EventOnDeath.FormToHuman)
        {
            GetComponent<AbilityController>().SwitchForm(AbilityController.PlayerForm.Human);
        }
    }

    public enum EventOnDeath
    {
        LoseRound,
        RespawnTarget,
        Destroy,
        FormToHuman
    }



}