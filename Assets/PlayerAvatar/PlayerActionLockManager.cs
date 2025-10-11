using Mirror;
using StarterAssets;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAction
{
    Move,
    Shoot,
    Ability
}

public class PlayerActionLockManager : NetworkBehaviour
{
    // MirrorのSyncVarは単純型しか扱えないため、状態はboolで個別に同期
    [SyncVar(hook = nameof(OnMoveLockChanged))] private bool canMove = true;
    [SyncVar(hook = nameof(OnShootLockChanged))] private bool canShoot = true;
    [SyncVar(hook = nameof(OnAbilityLockChanged))] private bool canUseAbility = true;

    private readonly Dictionary<PlayerAction, HashSet<string>> lockReasons = new()
    {
        { PlayerAction.Move, new HashSet<string>() },
        { PlayerAction.Shoot, new HashSet<string>() },
        { PlayerAction.Ability, new HashSet<string>() }
    };

    public bool CanMove => canMove;
    public bool CanShoot => canShoot;
    public bool CanUseAbility => canUseAbility;

    // --- 公開API（サーバーから呼ぶ） ---
    public void AddLock(PlayerAction action, string reason)
    {
        if (!lockReasons[action].Add(reason))
            return;

        if (!isServer)
        {
            CmdAddLock(action, reason);
            return;
        }

        UpdateState(action);
    }

    [Command (requiresAuthority = false)]
    public void CmdAddLock(PlayerAction action, string reason)
    {
        if (lockReasons[action].Add(reason))
            UpdateState(action);
    }

    public void RemoveLock(PlayerAction action, string reason)
    {

        if (!lockReasons[action].Remove(reason))
            return;

        if (!isServer) 
        {
            CmdRemoveLock(action, reason);
            return;
        }
        
        UpdateState(action);
    }


    [Command(requiresAuthority = false)]
    public void CmdRemoveLock(PlayerAction action, string reason)
    {
        if (lockReasons[action].Remove(reason))
            UpdateState(action);
    }

    [Server]
    public void ServerRemoveLockAll(PlayerAction action)
    {
        lockReasons[action].Clear();
        UpdateState(action);
    }


    [Server]
    private void UpdateState(PlayerAction action)
    {
        bool canDo = lockReasons[action].Count == 0;

        switch (action)
        {
            case PlayerAction.Move:
                canMove = canDo;
                break;
            case PlayerAction.Shoot:
                canShoot = canDo;
                break;
            case PlayerAction.Ability:
                canUseAbility = canDo;
                break;
        }
    }

    // --- クライアント同期処理 ---
    private void OnMoveLockChanged(bool _, bool newValue)
    {
        if (GetComponentInChildren<ThirdPersonController>() != null)
        {
            GetComponentInChildren<ThirdPersonController>().SetMovementEnabled(newValue);
        }
    }

    private void OnShootLockChanged(bool _, bool newValue)
    {
        if (GetComponentInChildren<ShootManager>() != null)
        {
            GetComponentInChildren<ShootManager>().SetShootingEnabled(newValue);
        }
    }

    private void OnAbilityLockChanged(bool _, bool newValue)
    {
        if (GetComponentInChildren<AbilityController>() != null)
        {
            GetComponent<AbilityController>().SetAbilityEnabled(newValue);
        }
    }
}
