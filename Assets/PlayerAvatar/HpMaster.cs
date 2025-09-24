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
    private void Update()
    {
        if (transform.position.y <= -30)
        {
            TakeDamage(10);
        }
    }
    

    [Server]
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible)
        {
            return;
        }

        int correctedDamage = damage;

        if (damage >= 0) {
            correctedDamage = (int)(damage * armer); 
        }

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
        if (onDeath == EventOnDeath.LOSEROUND)
        {
            RoundManager.rm.RoundEnd(gameObject);
        }
        if (onDeath == EventOnDeath.RESPAWNTARGET)
        {
            ResetHp();
            GetComponent<BotManager>().ResetPos();
        }
        if (onDeath == EventOnDeath.DESTROY)
        {
            NetworkServer.Destroy(gameObject);
        }
        if (onDeath == EventOnDeath.HORUSDESTROY)
        {
            NetworkIdentity owner = GetComponent<SpawnOwner>().WhoseThis();
            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.HORUSDESTROYED, transform.position, 1f);
            NetworkServer.Destroy(gameObject);
        }
        if (onDeath == EventOnDeath.FORMTOHUMAN)
        {
            GetComponent<AbilityController>().SwitchForm(AbilityController.PlayerForm.Human);
        }
        if (onDeath == EventOnDeath.LIGHTLOAD)
        {

            NetworkIdentity owner = GetComponent<SpawnOwner>().WhoseThis();

            int healHp = 0;
            if (owner.GetComponent<HpMaster>().hp + GetComponent<Darkness>().darknessSize >= 100)
            {
                healHp = 100 - owner.GetComponent<HpMaster>().hp;
            }
            else
            {
                healHp  = GetComponent<Darkness>().darknessSize;
            }

           

            owner.GetComponent<HpMaster>().TakeDamage(-healHp);
            owner.GetComponentInChildren<ServerCheckShoot>().DestroyOrb(gameObject);
        }
    }

    public enum EventOnDeath
    {
        LOSEROUND,
        RESPAWNTARGET,
        DESTROY,
        HORUSDESTROY,
        FORMTOHUMAN,
        LIGHTLOAD,
    }



}