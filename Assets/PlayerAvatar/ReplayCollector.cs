using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayCollector : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab; // ボタンプレハブ
    [SerializeField] private Transform contentParent; // ScrollViewのContentオブジェクト
    [SerializeField] private GameObject replayManager; // replayManagerオブジェクト

    public static List<Tuple<string, string>> replayPairs = new List<Tuple<string, string>>();

    public static Tuple<string, string> replayingPath = new Tuple<string, string>("","");

    void Start()
    {
        LoadReplayPairs();
        GenerateButtons();
    }

    // record1 と record2 のペアを探索
    private void LoadReplayPairs()
    {
        replayPairs.Clear();

        string streamingAssetsPath = Application.streamingAssetsPath;
        string[] allFiles = Directory.GetFiles(streamingAssetsPath, "*.mp4");

        // record1 を含むファイルを抽出
        List<string> record1Files = new List<string>();
        foreach (string file in allFiles)
        {
            string fileName = Path.GetFileName(file);
            if (fileName.Contains("record1"))
            {
                record1Files.Add(file);
            }
        }

        // record2 の対応ファイルを探してペア化
        foreach (string record1File in record1Files)
        {
            string record1FileName = Path.GetFileName(record1File);
            string record2FileName = record1FileName.Replace("record1", "record2");
            string record2File = Path.Combine(streamingAssetsPath, record2FileName);

            if (File.Exists(record2File))
            {
                replayPairs.Add(new Tuple<string, string>(record1File, record2File));
            }
        }

        // 結果をログに出力（デバッグ用）
        foreach (var pair in replayPairs)
        {
            Debug.Log($"Found pair: {pair.Item1} | {pair.Item2}");
        }
    }

    // ペアの数だけボタンを生成
    private void GenerateButtons()
    {
        Debug.Log(replayPairs.Count);

        foreach (var pair in replayPairs)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, contentParent);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            // ボタン名を record1 のファイル名に設定
            string fileName = Path.GetFileName(pair.Item1);
            buttonText.text = fileName;

            // ボタンクリック時の処理
            button.onClick.AddListener(() => OnReplayButtonClick(pair.Item1, pair.Item2));
        }
    }

    // ボタンが押されたときの処理
    private void OnReplayButtonClick(string record1Path, string record2Path)
    {
        Debug.Log($"Selected Record1: {record1Path}");
        Debug.Log($"Selected Record2: {record2Path}");

        gameObject.SetActive(false);
        replayManager.SetActive(true);

        replayingPath = new Tuple<string, string>(record1Path, record2Path);

        // ここで動画再生やファイル操作を行う
        // 例: VideoPlayer で再生するなど
    }
}