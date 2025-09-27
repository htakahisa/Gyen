using UnityEngine;
using Mirror;
using UnityEngine.Events;
using StarterAssets;

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

    public GameObject lightLoadEffect;

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

    [Server]
    public void TakeStun(float stunLevel, float duration)
    {
        if (isDead || isInvincible)
        {
            return;
        }

        if (hp <= 0) return;

        GetComponentInChildren<ThirdPersonController>()?.TargetCameraStun(GetComponent<NetworkIdentity>().connectionToClient, stunLevel, duration);

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
            FinisherManager.instance.PlayPlayerFinisher(RoundManager.rm.GetMyPlayer().GetComponentInChildren<WeaponManager>().GetCurrentWeaponStats(), gameObject);
        }
        if (onDeath == EventOnDeath.DESTROY)
        {
            NetworkServer.Destroy(gameObject);
        }
        if (onDeath == EventOnDeath.HORUSDESTROY)
        {
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

            Vector3 pos = owner.transform.position;
            pos.y += 1;

            GameObject instance = Instantiate(lightLoadEffect, pos, owner.transform.rotation);
            NetworkServer.Spawn(instance);
            
            owner.GetComponent<HpMaster>().TakeDamage(-healHp);
            owner.GetComponentInChildren<ServerCheckShoot>().DestroyOrb(gameObject);
        }
        if (onDeath == EventOnDeath.ITWEAKSDESTROY)
        {
            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.ITWEAKSDESTROYED, transform.position, 1f);
            NetworkServer.Destroy(gameObject);
        }
        if (onDeath == EventOnDeath.IMIRRORDESTROY)
        {
            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.IMIRRORDESTROYED, transform.position, 1f);
            NetworkServer.Destroy(gameObject);
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
        ITWEAKSDESTROY,
        IMIRRORDESTROY,
    }



}