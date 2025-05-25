using UnityEngine;

public class RenderingDictionary : MonoBehaviour
{
    public int indexToShow = 0; // 表示したい子オブジェクトのインデックス

    Transform[] children;
    Transform[] childObjects;

    void Start()
    {
        // 子オブジェクトをすべて取得（自分自身を除く）
        children = GetComponentsInChildren<Transform>();
        childObjects = System.Array.FindAll(children, t => t.parent == transform);
    }

    public void Update()
    {

        // 全ての子を非表示にし、指定のインデックスだけ表示
        for (int i = 0; i < childObjects.Length; i++)
        {
            bool isTarget = (i == indexToShow);
            childObjects[i].gameObject.SetActive(isTarget);
        }

        // 範囲外インデックスの警告
        if (indexToShow < 0 || indexToShow >= childObjects.Length)
        {
            Debug.LogWarning("インデックスが子オブジェクトの数を超えています。");
        }
    }

    public void UpIndex()
    {
        if (indexToShow + 1 >= childObjects.Length)
        {
            return;
        }

        indexToShow++;
    }

    public void DownIndex()
    {
        if (indexToShow < 1)
        {
            return;
        }

        indexToShow--;
    }
}
