using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static BadgeManager;

public class BadgeUIManager : MonoBehaviour
{
    public static BadgeUIManager Instance;

    [Header("自分のバッジパネル")]
    public List<Transform> localPanels;

    [Header("相手のバッジパネル")]
    public List<Transform> remotePanels;

    [Header("全バッジのスプライト一覧")]
    public List<Sprite> badgeSpriteList;

    private Dictionary<string, Sprite> spriteDict;

    public GameObject remotePanelParent;

    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (NetworkManager.singleton.numPlayers == 1)
        {
            remotePanelParent.SetActive(false);
        }
    }

    void Start()
    {
        spriteDict = badgeSpriteList.ToDictionary(sp => sp.name, sp => sp);
    }

    // ===== 自分のUI更新 =====
    public void UpdateLocalUI(IReadOnlyList<Badges> badgeList)
    {
        UpdateBadgeUI(localPanels, badgeList);
    }

    // ===== 相手のUI更新 =====
    public void UpdateRemoteUI(IReadOnlyList<Badges> badgeList)
    {
        UpdateBadgeUI(remotePanels, badgeList);
    }

    // ===== 共通処理 =====
    private void UpdateBadgeUI(List<Transform> panels, IReadOnlyList<Badges> badgeList)
    {
        if(panels.Count == 0)
        {
            return;
        }

        // パネル内の画像をまとめる
        List<Image> images = new List<Image>();
        foreach (var panel in panels)
        {
            images.AddRange(panel.GetComponentsInChildren<Image>().OrderBy(img => img.name));
        }

        // 表示するバッジを enum 順に並べる
        var ordered = System.Enum.GetValues(typeof(Badges))
                                 .Cast<Badges>()
                                 .Where(b => badgeList.Contains(b))
                                 .ToList();

        // UI反映
        for (int i = 0; i < images.Count; i++)
        {
            if (i < ordered.Count)
            {
                string name = ordered[i].ToString();
                if (spriteDict.TryGetValue(name, out Sprite sprite))
                {
                    images[i].enabled = true;
                    images[i].sprite = sprite;
                }
            }
            else
            {
                images[i].enabled = false;
                images[i].sprite = null;
            }
        }
    }
}
