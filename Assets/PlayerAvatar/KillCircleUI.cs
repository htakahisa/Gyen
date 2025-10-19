using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using Mirror;

public class KillCircleUI : NetworkBehaviour
{
    public static KillCircleUI Instance;

    [Header("References")]
    [SerializeField] private Image circleEffect;

    private void Awake() => Instance = this;




    [TargetRpc]
    public void TargetPlayEffect(NetworkConnection target, string spriteName, bool isHeadshot)
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/" + spriteName);
        StartCoroutine(PlayEffect(sprite, isHeadshot));
    }

    public IEnumerator PlayEffect(Sprite iconSprite, bool isHeadshot)
    {
        circleEffect.sprite = iconSprite;

        // マテリアルは触らず、Image.color で色を変える
        circleEffect.color = new Color(1f, 1f, 1f, 1f);

        // スケール初期化
        circleEffect.transform.localScale = Vector3.zero;

        // DOTween シーケンス
        Sequence seq = DOTween.Sequence();
        seq.Append(circleEffect.transform.DOScale(2f, 0.25f).SetEase(Ease.OutBack));

        // 待機
        yield return new WaitForSeconds(1f);

        // フェードアウト
        seq.Join(circleEffect.DOFade(0f, 0.4f));

        // シーケンス終了後にスケールと色をリセットしたい場合
        seq.OnComplete(() =>
        {
            circleEffect.transform.localScale = Vector3.zero;
            circleEffect.color = Color.white;
        });
    }

}
