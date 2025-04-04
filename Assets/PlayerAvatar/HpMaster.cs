using UnityEngine;
using Mirror;

public class HpMaster : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHpChanged))]
    public int hp = 100; // 各プレイヤーのHP

    [SyncVar]
    public bool isDead = false;

    private void OnHpChanged(int oldValue, int newValue)
    {
        Debug.Log($"{netId} のHPが {oldValue} → {newValue} に変更");
    }

    [Server]
    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        if (hp <= 0) return;
        hp -= damage;
        if (hp <= 0)
        {
            isDead = true;
            hp = 0;
            Debug.Log($"{netId} のプレイヤーが倒された");
            RoundManager.rm.ResetRound();
        }
    }

    [Server]
    public void ResetHp()
    {
        isDead = false;
        hp = 100;

    }


}