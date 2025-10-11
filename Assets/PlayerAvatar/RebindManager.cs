using UnityEngine;
using UnityEngine.InputSystem;

public class RebindManager : MonoBehaviour
{
    [Header("Input Action Asset全体")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("個別スロット")]
    [SerializeField] private RebindSlot[] slots;

    private const string PREFS_KEY = "AllBindings";

    void Awake()
    {
        // 起動時にロード
        LoadAll();
    }

    public void SaveAll()
    {
        if (inputActions == null) return;

        string json = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("全キー設定を保存しました");
    }

    public void LoadAll()
    {
        if (inputActions == null) return;

        if (PlayerPrefs.HasKey(PREFS_KEY))
        {
            string json = PlayerPrefs.GetString(PREFS_KEY);
            inputActions.LoadBindingOverridesFromJson(json);
            Debug.Log("バインドをPlayerPrefsからロードしました");
        }
        else
        {
            Debug.Log("保存データなし：デフォルト設定を使用します");
        }

        // --- 🔥 UI更新を強制的に呼ぶ ---
        foreach (var slot in slots)
        {
            if (slot != null)
                slot.UpdateDisplayText();
        }

        Debug.Log("全キー設定を読み込み、スロットを更新しました");
    }

    public void ResetAll()
    {
        if (inputActions == null) return;

        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PREFS_KEY);

        foreach (var slot in slots)
        {
            if (slot != null)
                slot.UpdateDisplayText();
        }

        Debug.Log("全キー設定をリセットしました");
    }
}

