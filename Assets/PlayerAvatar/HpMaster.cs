using UnityEngine;
using Mirror;
using UnityEngine.Events;
using StarterAssets;

public class HpMaster : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHpChanged))]
    public int hp = 100; // 各プレイヤーのHP

    public int maxHp = 100;

    [SyncVar]
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

    private void Awake()
    {
        if (isServer)
        {
            ResetHp();

        }
    }


    private void Update()
    {
        if (transform.position.y <= -30)
        {
            TakeDamage(10, false);
        }

    }
    

    [Server]
    public void TakeDamage(int damage, bool headshot)
    {
        if (isDead || isInvincible || (RoundManager.rm.Mode == "1VS1" && RoundManager.rm.CurrentPhase == RoundManager.Phase.BUY))
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
            OnDeath(headshot);
        }
    }

    [Server]
    public void ResetHp()
    {
        isDead = false;
        hp = maxHp;

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

    public void OnDeath(bool headshot)
    {
        if (onDeath == EventOnDeath.LOSEROUND)
        {
            RoundManager.rm.RoundEnd(gameObject);
        }
        if (onDeath == EventOnDeath.RESPAWNTARGET)
        {
            ResetHp();
            GetComponent<BotManager>().ResetPos();
            FinisherManager.instance.PlayPlayerFinisher(RoundManager.rm.GetMyPlayer().GetComponentInChildren<WeaponManager>().GetCurrentWeaponStats(), gameObject, headshot);
        }
        if (onDeath == EventOnDeath.DESTROY)
        {
            NetworkServer.Destroy(gameObject);
        }
        if (onDeath == EventOnDeath.HORUSDESTROY)
        {
            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.HORUSDESTROYED, transform.position, 1f, 30);
            NetworkServer.Destroy(gameObject);
        }
        if (onDeath == EventOnDeath.FORMTOHUMAN)
        {
            GetComponentInParent<AbilityController>().SwitchForm(AbilityController.PlayerForm.Human);
            ResetHp();
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
            
            owner.GetComponent<HpMaster>().TakeDamage(-healHp, false);
            owner.GetComponentInChildren<ServerCheckShoot>().DestroyOrb(gameObject);
        }
        if (onDeath == EventOnDeath.ITWEAKSDESTROY)
        {
            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.ITWEAKSDESTROYED, transform.position, 1f, 30);
            NetworkServer.Destroy(gameObject);
        }
        if (onDeath == EventOnDeath.IMIRRORDESTROY)
        {
            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.IMIRRORDESTROYED, transform.position, 1f, 30);
            NetworkServer.Destroy(gameObject);
        }
        if (onDeath == EventOnDeath.PLAYERDEAD)
        {
            GetComponent<PlayerActionLockManager>().AddLock(PlayerAction.Move, "Dead");
            GetComponent<PlayerActionLockManager>().AddLock(PlayerAction.Shoot, "Dead");
            GetComponentInChildren<ThirdPersonController>().RpcResetSpeed();
            RoundManager.rm.Finisher(gameObject, headshot);
            if(RoundManager.rm.defender == gameObject)
            {
                RoundManager.rm.RoundEnd(gameObject);
            }
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
        PLAYERDEAD,
    }



}