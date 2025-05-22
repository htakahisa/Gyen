using StarterAssets;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    public ThirdPersonController tpc;
    public CharacterController cc;
    public float moveTime = 0f;
    public float currentMoveDirection;
    public float moveDuration;

    public float jumpTime = 0f;
    public float jumpDuration;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (RoundManager.rm.currentBotMove != RoundManager.BotMove.STOP)
        {
            // 移動時間が終了したら新しい方向を決定
            if (moveTime <= 0)
            {
                currentMoveDirection = Random.Range(-1, 2);
                moveDuration = Random.Range(0.3f, 1f);
            }

            // 移動実行
            tpc.BotMove(currentMoveDirection, RoundManager.rm.currentBotMove == RoundManager.BotMove.WALK);
            moveTime += Time.deltaTime;

            // 移動時間が終了したらリセット
            if (moveTime >= moveDuration)
            {
                moveTime = 0;
            }

            if(RoundManager.rm.currentBotMove == RoundManager.BotMove.JUMP)
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

        }
        else
        {
            tpc.BotStop();
        }
    }

    public void ResetPos()
    {
        cc.enabled = false;
        transform.position = new Vector3(Random.Range(-8.53f, 2.77f), 0.01000023f, Random.Range(1.20f, 3.27f));
        cc.enabled = true;
    }
}
