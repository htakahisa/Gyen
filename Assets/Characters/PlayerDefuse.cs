using Mirror;
using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkIdentity))]
public class PlayerDefuse : NetworkBehaviour
{
    private PlayerInput playerInput; private InputAction interactAction;
    // BombManager は一つだけの想定。複数ある場合は最寄りの Bomb を探す処理に置き換えてください。
    BombManager nearestBomb;

    public PlayerActionLockManager lockManager;
    public IEnumerator InitialSetInput()
    {
        // PlayerInput の初期化タイミングを待つ
        yield return new WaitForSeconds(0.1f);
        playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput != null)
        {
            interactAction = playerInput.actions.FindAction("Interact");
            if (interactAction != null)
                interactAction.Enable();
        }
    }


    private void Start()
    {
        StartCoroutine(InitialSetInput());
    }


    void Update()
    {
        if (!isLocalPlayer) return; // 自分のプレイヤーのみ入力を読む
        if (interactAction == null) return;


        FindNearestBombIfNeeded();
        if (nearestBomb == null) return;


        // 押下開始
        if (interactAction.WasPressedThisFrame())
        {
            nearestBomb.CmdStartDefuse(netIdentity);
;       }


        // 押下中
        if (interactAction.IsPressed())
        {
            
        }


        // 放したとき
        if (interactAction.WasReleasedThisFrame())
        {
            nearestBomb.CmdStopDefuse(netIdentity);
        }
    }



    void FindNearestBombIfNeeded()
    {
        if (nearestBomb != null) return;


        var bombs = FindObjectsOfType<BombManager>();
        float minDist = float.MaxValue;
        BombManager best = null;


        foreach (var b in bombs)
        {
            var d = Vector3.Distance(transform.position, b.transform.position);
            if (d < minDist)
            {
                minDist = d;
                best = b;
            }
        }


        if (best != null && minDist <= best.defuseRange + 0.5f)
        {
            nearestBomb = best;
        }
    }
}