using System.Collections.Generic;
using UnityEngine;

public class KeyRebindUIManager : MonoBehaviour
{
    [SerializeField] private List<RebindItem> rebindItems = new List<RebindItem>();

    private const string PlayerPrefsKey = "KeyBindings";

    void Start()
    {
        LoadAllBindings();
    }

    public void SaveAllBindings()
    {
        var list = new List<string>();
        foreach (var item in rebindItems)
            list.Add(item.SaveBindingsToJson());

        var wrapper = new Wrapper { bindings = list };
        var json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();

        Debug.Log("キー設定を保存しました");
    }

    public void LoadAllBindings()
    {
        var json = PlayerPrefs.GetString(PlayerPrefsKey, "");
        if (string.IsNullOrEmpty(json)) return;

        var wrapper = JsonUtility.FromJson<Wrapper>(json);
        for (int i = 0; i < wrapper.bindings.Count && i < rebindItems.Count; i++)
        {
            rebindItems[i].LoadBindingsFromJson(wrapper.bindings[i]);
        }

        Debug.Log("キー設定を読み込みました");
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<string> bindings;
    }
}
