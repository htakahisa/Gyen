using System;
using TMPro;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    public GameObject[] objects; // 切り替えるオブジェクトの配列（Inspectorで設定）
    private int currentIndex = 0; // 現在表示中のオブジェクトインデックス

    public static float playbackSpeed = 1f;

    public static bool isPaused = false;

    public TextMeshProUGUI playSpeedText;

    void Start()
    {
        // 初期状態: 最初のオブジェクトのみアクティブ
        SetActiveObject(currentIndex);
    }

    void Update()
    {
        // 左クリックを検出
        if (Input.GetMouseButtonDown(0))
        {
            // インデックスを更新
            currentIndex = (currentIndex + 1) % objects.Length;

            // オブジェクトを切り替え
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
        // すべてのオブジェクトを非アクティブに
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