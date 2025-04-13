using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class BuyPanel : MonoBehaviour
{
    private GameObject player;

    public GameObject panel;

    private bool isCursorLocked = true;

    // Start is called before the first frame update
    void Awake()
    {
        panel.SetActive(false);
        // 初期状態でカーソルをロックし、非表示にする
        LockCursor();
        Invoke("StartGetPlayer", 2f);
    }

    void StartGetPlayer()
    {
        player = RoundManager.rm.GetMyPlayer();
    }

    // Update is called once per frame
    void Update()
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
