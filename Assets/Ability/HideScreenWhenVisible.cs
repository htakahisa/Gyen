using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HideScreenWhenVisible : MonoBehaviour
{
[System.Serializable]
public class FlashTarget
{
    public Renderer renderer;
    public float flashDuration;
    [HideInInspector] public float flashTimer;
    [HideInInspector] public bool hasFlashed; // 追加

    public FlashTarget(Renderer rend, float duration)
    {
        renderer = rend;
        flashDuration = duration;
        flashTimer = 0f;
        hasFlashed = false; // 初期化
    }
}

    [Header("Camera & Targets")]
    public Camera sourceCamera;
    public List<FlashTarget> targets = new List<FlashTarget>();

    [Header("UI Overlay")]
    public Canvas overlayCanvas;
    public Image overlayImage;

    [Header("Fade Settings")]
    public bool fade = true;
    public float fadeDuration = 0.25f;

    private float overlayAlpha = 0f;
    public static HideScreenWhenVisible instance;


    void Awake()
    {
        instance = this;

        if (overlayCanvas == null || overlayImage == null)
        {
            CreateOverlayUI();
        }

        SetOverlayAlpha(0f);
    }

    void Update()
    {
        if (sourceCamera == null) sourceCamera = Camera.main;

        bool shouldFlash = false;

        foreach (var t in targets)
        {
            if (t.renderer == null) continue;

            // まだフラッシュしていない場合のみ、映っていたらフラッシュ開始
            if (!t.hasFlashed && IsVisibleFrom(t.renderer, sourceCamera))
            {
                t.flashTimer = t.flashDuration;
                t.hasFlashed = true; // 1度だけフラッシュ開始
            }

            // タイマー > 0 の間はフラッシュ継続
            if (t.flashTimer > 0f)
            {
                t.flashTimer -= Time.deltaTime;
                shouldFlash = true;
            }
            else
            {
                // タイマーが切れたらフラグをリセット
                t.hasFlashed = false;
            }
        }

        // アルファ制御
        float targetAlpha = shouldFlash ? 1f : 0f;
        if (fade)
        {
            overlayAlpha = Mathf.MoveTowards(overlayAlpha, targetAlpha,
                Time.deltaTime / Mathf.Max(0.0001f, fadeDuration));
        }
        else
        {
            overlayAlpha = targetAlpha;
        }

        SetOverlayAlpha(overlayAlpha);
    }


    /// <summary>
    /// スクリプトからターゲット追加可能
    /// </summary>
    public void AddTarget(Renderer rend, float duration)
    {
        if (rend != null)
        {
            targets.Add(new FlashTarget(rend, duration));
        }
    }

    private void SetOverlayAlpha(float a)
    {
        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            c.a = Mathf.Clamp01(a);
            overlayImage.color = c;
        }
    }

    private void CreateOverlayUI()
    {
        // Canvas を生成
        GameObject canvasGO = new GameObject("OverlayCanvas");
        canvasGO.transform.SetParent(transform, false);
        overlayCanvas = canvasGO.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Image を生成
        GameObject imageGO = new GameObject("OverlayImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        overlayImage = imageGO.AddComponent<Image>();

        // RectTransform を画面いっぱいに
        RectTransform rt = overlayImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        // スクリーンサイズに合わせる
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);

        // 黒（透明度は後で制御）
        overlayImage.color = new Color(0f, 0f, 0f, 0f);
    }

    /// <summary>
    /// 指定カメラに映っているか判定（遮蔽物あり）
    /// </summary>
    bool IsVisibleFrom(Renderer rend, Camera cam)
    {
        if (rend == null || cam == null) return false;

        Vector3 viewPos = cam.WorldToViewportPoint(rend.bounds.center);

        // カメラの前にない
        if (viewPos.z < 0f) return false;

        // ビューポート範囲内
        if (viewPos.x < 0f || viewPos.x > 1f || viewPos.y < 0f || viewPos.y > 1f)
            return false;

        // 遮蔽物チェック
        Vector3 dir = rend.bounds.center - cam.transform.position;
        if (Physics.Raycast(cam.transform.position, dir.normalized, out RaycastHit hit, dir.magnitude + 0.01f))
        {
            if (!hit.collider.transform.IsChildOf(rend.transform))
                return false; // 他のオブジェクトに隠れている
        }

        return true;
    }
}
