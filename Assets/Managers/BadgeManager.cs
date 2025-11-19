using System.Collections.Generic;
using UnityEngine;

public class BadgeManager : MonoBehaviour
{

    public SortedSet<Badges> allBadges = new SortedSet<Badges>(
    (Badges[])System.Enum.GetValues(typeof(Badges))
    );

    public SortedSet<Badges> havingBadges = new SortedSet<Badges>();


    public enum Badges
    {
        FanaticsSeeker,
        FanaticsElite,
        FanaticsTheDual,
        FanaticsCheater,
        FanaticsTwo,
        HereticsSeeker,
        HereticsElite,
        HereticsTheDual,
        HereticsCheater,
        HereticsTwo,
    }


    private const string Key = "BadgeList";

    private void Start()
    {
        havingBadges = LoadBadges();

        var sync = GetComponent<BadgeSync>();
        if (sync != null) 
        {
            foreach (var badge in havingBadges) 
            {
                sync.CmdAddBadge(badge); 
            }
        }
        // ï˚ñ@1: foreachÇ≈èoóÕ
        foreach (var badge in havingBadges)
        {
            Debug.Log(badge);
        }
    }


    public void ConfirmBadge(DuelLandFanaticsDatas duelLandFanaticsDatas)
    {
        string[] datas = duelLandFanaticsDatas.mapName.Split("_");
        string map = datas[0];
        string side = datas[1];
        string level = datas[2];

        
        if (level == "Seeker")
        {
            havingBadges.Add(Badges.FanaticsSeeker);
            SaveBadges(havingBadges);
        }
        if (level == "Elite")
        {
            havingBadges.Add(Badges.FanaticsElite);
            SaveBadges(havingBadges);
        }
        if (level == "TheDual")
        {
            havingBadges.Add(Badges.FanaticsTheDual);
            SaveBadges(havingBadges);
        }
        if (level == "Cheater")
        {
            havingBadges.Add(Badges.FanaticsCheater);
            SaveBadges(havingBadges);
        }
        if (level == "Two")
        {
            havingBadges.Add(Badges.FanaticsTwo);
            SaveBadges(havingBadges);
        }
    }

    public void ConfirmBadge(DuelLandHereticsDatas duelLandHereticsDatas)
    {
        string[] datas = duelLandHereticsDatas.mapName.Split("_");
        string map = datas[0];
        string side = datas[1];
        string level = datas[2];


        if (level == "Seeker")
        {
            havingBadges.Add(Badges.HereticsSeeker);
            SaveBadges(havingBadges);
        }
        if (level == "Elite")
        {
            havingBadges.Add(Badges.HereticsElite);
            SaveBadges(havingBadges);
        }
        if (level == "TheDual")
        {
            havingBadges.Add(Badges.HereticsTheDual);
            SaveBadges(havingBadges);
        }
        if (level == "Cheater")
        {
            havingBadges.Add(Badges.HereticsCheater);
            SaveBadges(havingBadges);
        }
        if (level == "Two")
        {
            havingBadges.Add(Badges.HereticsTwo);
            SaveBadges(havingBadges);
        }


    }


    // ï€ë∂
    public static void SaveBadges(SortedSet<Badges> list)
    {
        // enum ñºÇ SortedSet<string> Ç…ì¸ÇÍÇÈ
        SortedSet<string> names = new SortedSet<string>();
        foreach (var e in list)
        {
            names.Add(e.ToString());
        }

        string json = JsonUtility.ToJson(new Wrapper { items = new List<string>(names) });

        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
    }

    // ì«Ç›çûÇ›
    public static SortedSet<Badges> LoadBadges()
    {
        if (!PlayerPrefs.HasKey(Key))
            return new SortedSet<Badges>();

        string json = PlayerPrefs.GetString(Key);
        Wrapper wrapper = JsonUtility.FromJson<Wrapper>(json);

        SortedSet<Badges> result = new SortedSet<Badges>();
        foreach (var name in wrapper.items)
        {
            if (System.Enum.TryParse(name, out Badges badge))
                result.Add(badge);
        }

        return result;
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<string> items;
    }
}
