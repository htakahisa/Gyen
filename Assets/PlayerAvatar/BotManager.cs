using Mirror;
using StarterAssets;
using System.Collections;
using UnityEngine;
using static WeaponStatus;

public class BotManager : NetworkBehaviour
{
    public ThirdPersonController tpc;
    public CharacterController cc;
    public WeaponManager wm;
    public ShootManager sm;
    public HpMaster hm;
    public float moveTime = 0f;
    public float currentMoveDirection;
    public float moveDuration;

    public float jumpTime = 0f;
    public float jumpDuration;

    public float crouchTime = 0f;
    public float crouchDuration;

    public WeaponType weapon;
    public float armer;
    public int moveVector;
    public float moveAsScriptTime;
    public bool movingAsScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wm.RpcBuyWeapon(WeaponStatus.WeaponType.ReiNe);
        hm.armer = this.armer;
        if (movingAsScript)
        {
            MovingAsScript();
        }
    }

    public void MovingAsScript()
    {
        float timer = 0;

        while (timer <= moveAsScriptTime)
        {
            timer += Time.deltaTime;
            tpc.BotMove(moveVector, RoundManager.rm.currentBotMove == RoundManager.BotMove.WALK, false);
        }
        tpc.BotStop();
        movingAsScript = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (RoundManager.rm.doesBotShoot && tpc.GetSpeed() == 0 && tpc.Grounded)
        {
            sm.BotShoot();
        }
        if (!movingAsScript) {
            if (RoundManager.rm.currentBotMove != RoundManager.BotMove.STOP)
            {
                // 移動時間が終了したら新しい方向を決定
                if (moveTime <= 0)
                {
                    currentMoveDirection = Random.Range(-1, 2);
                    moveDuration = Random.Range(0.3f, 1f);
                }

                // 移動実行
                tpc.BotMove(currentMoveDirection, RoundManager.rm.currentBotMove == RoundManager.BotMove.WALK, crouchTime < crouchDuration);
                moveTime += Time.deltaTime;

                // 移動時間が終了したらリセット
                if (moveTime >= moveDuration)
                {
                    moveTime = 0;
                }

                if (RoundManager.rm.currentBotMove == RoundManager.BotMove.JUMP)
                {
                    if (tpc.Grounded)
                    {
                        jumpTime += Time.deltaTime;

                        if (jumpTime >= jumpDuration)
                        {
                            tpc.BotJumpAndGravity(true);
                            jumpTime = 0;
                            jumpDuration = Random.Range(1.0f, 5.0f);
                        }
                        else
                        {
                            tpc.BotJumpAndGravity(false);
                        }

                    }
                    else
                    {
                        tpc.BotJumpAndGravity(false);
                    }

                }

                if (RoundManager.rm.currentBotMove == RoundManager.BotMove.CROUCH)
                {
                    if (tpc.Grounded)
                    {
                        crouchTime += Time.deltaTime;

                        if (crouchTime >= crouchDuration + Random.Range(1f, 2.0f))
                        {
                            crouchTime = 0;
                            crouchDuration = Random.Range(0.3f, 1.0f);
                        }


                    }

                }

            }
            else
            {
                tpc.BotStop();
            } 
        }
    }

    public void ResetPos()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        GetComponentInChildren<ThirdPersonController>().ServerUpdateAllPositions(new Vector3(Random.Range(-8.53f, 2.77f), -0.02000034f, Random.Range(1.20f, 3.27f)));
    }
}
