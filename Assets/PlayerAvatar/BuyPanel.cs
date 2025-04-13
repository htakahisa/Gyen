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
        // ������ԂŃJ�[�\�������b�N���A��\���ɂ���
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

    // �V�[���؂�ւ����ȂǂɃJ�[�\����Ԃ����Z�b�g����Ȃ��悤�ɂ���
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
