using UnityEngine;

public class RebindManager : MonoBehaviour
{
    [SerializeField] private RebindSlot[] slots;

    void Start()
    {
        foreach (var slot in slots)
            slot.LoadBinding();
    }

    public void SaveAll()
    {
        foreach (var slot in slots)
            slot.SendMessage("SaveBinding", SendMessageOptions.DontRequireReceiver);
        Debug.Log("全キー設定を保存しました");
    }

    public void LoadAll()
    {
        foreach (var slot in slots)
            slot.LoadBinding();
        Debug.Log("全キー設定を読み込みました");
    }

    public void ResetAll()
    {
        foreach (var slot in slots)
            slot.ResetBinding();
        Debug.Log("全キー設定をリセットしました");
    }
}
