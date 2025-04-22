using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayCollector : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab; // �{�^���v���n�u
    [SerializeField] private Transform contentParent; // ScrollView��Content�I�u�W�F�N�g
    [SerializeField] private GameObject replayManager; // replayManager�I�u�W�F�N�g

    public static List<Tuple<string, string>> replayPairs = new List<Tuple<string, string>>();

    public static Tuple<string, string> replayingPath = new Tuple<string, string>("","");

    void Start()
    {
        LoadReplayPairs();
        GenerateButtons();
    }

    // record1 �� record2 �̃y�A��T��
    private void LoadReplayPairs()
    {
        replayPairs.Clear();

        string streamingAssetsPath = Application.streamingAssetsPath;
        string[] allFiles = Directory.GetFiles(streamingAssetsPath, "*.mp4");

        // record1 ���܂ރt�@�C���𒊏o
        List<string> record1Files = new List<string>();
        foreach (string file in allFiles)
        {
            string fileName = Path.GetFileName(file);
            if (fileName.Contains("record1"))
            {
                record1Files.Add(file);
            }
        }

        // record2 �̑Ή��t�@�C����T���ăy�A��
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

        // ���ʂ����O�ɏo�́i�f�o�b�O�p�j
        foreach (var pair in replayPairs)
        {
            Debug.Log($"Found pair: {pair.Item1} | {pair.Item2}");
        }
    }

    // �y�A�̐������{�^���𐶐�
    private void GenerateButtons()
    {
        Debug.Log(replayPairs.Count);

        foreach (var pair in replayPairs)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, contentParent);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            // �{�^������ record1 �̃t�@�C�����ɐݒ�
            string fileName = Path.GetFileName(pair.Item1);
            buttonText.text = fileName;

            // �{�^���N���b�N���̏���
            button.onClick.AddListener(() => OnReplayButtonClick(pair.Item1, pair.Item2));
        }
    }

    // �{�^���������ꂽ�Ƃ��̏���
    private void OnReplayButtonClick(string record1Path, string record2Path)
    {
        Debug.Log($"Selected Record1: {record1Path}");
        Debug.Log($"Selected Record2: {record2Path}");

        gameObject.SetActive(false);
        replayManager.SetActive(true);

        replayingPath = new Tuple<string, string>(record1Path, record2Path);

        // �����œ���Đ���t�@�C��������s��
        // ��: VideoPlayer �ōĐ�����Ȃ�
    }
}