using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class BuyPanel : MonoBehaviour
{
    private GameObject player;

    public GameObject panel;

    public bool isCursorLocked = true;

    public static BuyPanel buyPanel;

    private bool hasLoaded = false;

    // Start is called before the first frame update
    void Awake()
    {
        buyPanel = this;
        panel.SetActive(false);
        // 初期状態でカーソルをロックし、非表示にする
        LockCursor();
        
    }



    void StartGetPlayer()
    {
        player = RoundManager.rm.GetMyPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if (RoundManager.rm != null)
        {
            if (RoundManager.rm.hasLoaded && PlayerManager.hasLoaded && !hasLoaded)
            {
                StartGetPlayer();
                hasLoaded = true;
            }
        }

        if (RoundManager.rm.CurrentPhase == RoundManager.Phase.BUY)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                panel.SetActive(!panel.activeSelf);

                if (isCursorLocked)
                {
                    UnlockCursor();
                }
                else
                {
                    LockCursor();
                }
            }
        }
        else
        {
            panel.SetActive(false);
            LockCursor();
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }

    // シーン切り替え時などにカーソル状態がリセットされないようにする
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            if (isCursorLocked)
            {
                LockCursor();
            }
        }
    }
}
