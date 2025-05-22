using UnityEngine;
using Mirror;

public class HpMaster : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHpChanged))]
    public int hp = 100; // �e�v���C���[��HP

    public float armer = 1; // �e�v���C���[��HP
    [SyncVar]
    public bool isInvincible = false;

    [SyncVar]
    public bool isDead = false;

    private void OnHpChanged(int oldValue, int newValue)
    {
        Debug.Log($"{netId} ��HP�� {oldValue} �� {newValue} �ɕύX");
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
            Debug.Log($"{netId} �̃v���C���[���|���ꂽ");
            if (RoundManager.rm.Mode == "1VS1")
            {
                RoundManager.rm.RoundEnd(gameObject);
            }
            if (RoundManager.rm.Mode == "Practice")
            {
                ResetHp();
                GetComponent<BotManager>().ResetPos();
            }
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


}