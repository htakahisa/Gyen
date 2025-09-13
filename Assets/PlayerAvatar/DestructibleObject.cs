using Mirror;
using UnityEngine;

public class DestructibleObject : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHpChanged))]
    public int hp = 100; // 各プレイヤーのHP

    public float armer = 1; // 各プレイヤーのHP
    [SyncVar]
    public bool isInvincible = false;

    [SyncVar]
    public bool isDead = false;

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
            Debug.Log($"{netId} のプレイヤーが倒された");
            if (RoundManager.rm.Mode == "1VS1")
            {
                RoundManager.rm.RoundEnd(gameObject);
            }
        }
    }
}
