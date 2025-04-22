using System;
using TMPro;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    public GameObject[] objects; // �؂�ւ���I�u�W�F�N�g�̔z��iInspector�Őݒ�j
    private int currentIndex = 0; // ���ݕ\�����̃I�u�W�F�N�g�C���f�b�N�X

    public static float playbackSpeed = 1f;

    public static bool isPaused = false;

    public TextMeshProUGUI playSpeedText;

    void Start()
    {
        // �������: �ŏ��̃I�u�W�F�N�g�̂݃A�N�e�B�u
        SetActiveObject(currentIndex);
    }

    void Update()
    {
        // ���N���b�N�����o
        if (Input.GetMouseButtonDown(0))
        {
            // �C���f�b�N�X���X�V
            currentIndex = (currentIndex + 1) % objects.Length;

            // �I�u�W�F�N�g��؂�ւ�
            SetActiveObject(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isPaused)
            {
                isPaused = true;
            }
            else
            {
                
                isPaused = false;
            }
        }


        if (Input.GetKeyDown(KeyCode.A))
        {
            playbackSpeed /= 1.5f;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            playbackSpeed *= 1.5f;
        }

        playSpeedText.text = "playSpeed : " + Math.Round(playbackSpeed, 1);

    }

    void SetActiveObject(int index)
    {
        // ���ׂẴI�u�W�F�N�g���A�N�e�B�u��
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].GetComponentInChildren<Renderer>() != null)
            {
                foreach (var renderer in objects[i].GetComponentsInChildren<Renderer>()) {
                    renderer.enabled = (i == index); 
                }
            }
        }
    }
}