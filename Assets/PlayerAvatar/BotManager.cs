using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
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
    public float foundDelayTime;
    public float foundDelayTimeDelta;

    // 速度による増分の最大値（0.2f → 合計0.5fまで）
    public float maxSpeedBonus = 0.2f;

    // 速度の最大想定値（走っている時の最大速度に合わせる）
    public float maxSpeed = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        wm.RpcBuyWeapon(weapon);
        hm.armer = this.armer;
        if (movingAsScript)
        {
            StartCoroutine(MovingAsScript());
        }
    }

    public IEnumerator MovingAsScript()
    {
        Debug.Log("Before wait, active=" + gameObject.activeInHierarchy);

        yield return new WaitForSeconds(moveAsScriptTime);
        
        movingAsScript = false;
    }
    public float GetFoundDelayTimeDelta()
    {
        float speed = cc.velocity.magnitude;

        // speed(0〜maxSpeed) → bonus(0〜maxSpeedBonus) に変換
        float bonus = Mathf.Lerp(0f, maxSpeedBonus, speed / maxSpeed);

        // 基本値 + 増分
        return foundDelayTime + bonus;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GetComponent<SpawnOwner>().IsMine() && GetComponent<SpawnOwner>().ownerNetId != 12345) return;

        if(wm.magazine == 0)
        {
            wm.CmdReload();
        }
        bool jump = false;

        foundDelayTimeDelta = GetFoundDelayTimeDelta();

        if (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND)
        {
            sm.StartFoundDelay(foundDelayTimeDelta);
            if (RoundManager.rm.doesBotShoot && tpc.GetSpeed() <= 1f && tpc.Grounded)
            {
                sm.BotShoot();
            }
        }
        if (RoundManager.rm.currentMode == RoundManager.Mode.DOUBLETAP)
        {
            if (RoundManager.rm.CurrentPhase == RoundManager.Phase.BATTLE) 
            {
                sm.StartFoundDelay(foundDelayTimeDelta); 
            }
            if (RoundManager.rm.doesBotShoot && tpc.GetSpeed() <= 1f && tpc.Grounded)
            {
                if (RoundManager.rm.CurrentPhase == RoundManager.Phase.BATTLE)
                {
                    sm.BotShoot();
                }
            }
        }
        if (RoundManager.rm.currentMode == RoundManager.Mode.DOUBLETAP) return;

        if (movingAsScript)
        {
            tpc.BotMove(moveVector, 0, RoundManager.rm.currentBotMove == RoundManager.BotMove.WALK, false);
        }
        else
        {
        
            if (RoundManager.rm.currentBotMove != RoundManager.BotMove.STOP)
            {
                if (RoundManager.rm.currentBotMove == RoundManager.BotMove.WALK || RoundManager.rm.currentBotMove == RoundManager.BotMove.RUN)
                {

                    if (moveTime <= 0)
                    {
                        currentMoveDirection = Random.Range(-1, 2);
                        moveDuration = Random.Range(0.3f, 1f);
                    }
                }
                else
                {
                    currentMoveDirection = 0;
                }

                // 移動実行
                tpc.BotMove(currentMoveDirection, 0, RoundManager.rm.currentBotMove == RoundManager.BotMove.WALK, crouchTime < crouchDuration && sm.hasFound);
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
                            jump = true;
                            jumpTime = 0;
                            jumpDuration = Random.Range(1.0f, 5.0f);
                        }

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

        }

        tpc.jumpBot = jump;
    }

    public void ResetPos()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        GetComponentInChildren<ThirdPersonController>().ServerUpdateAllPositions(new Vector3(Random.Range(-8.53f, 2.77f), -0.02000034f, Random.Range(1.20f, 3.27f)), Vector3.zero);
    }
}
