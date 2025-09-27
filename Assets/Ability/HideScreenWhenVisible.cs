using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

public class HideScreenWhenVisible : NetworkBehaviour
{
    [System.Serializable]
    public class FlashTargetData
    {
        public uint netId;          // ネットワークID
        public float flashDuration;

        [HideInInspector] public float flashTimer;
        [HideInInspector] public bool hasFlashed;

        // デフォルトコンストラクタ（必須）
        public FlashTargetData()
        {
            netId = 0;
            flashDuration = 0f;
            flashTimer = 0f;
            hasFlashed = false;
        }

        // 便利コンストラクタ（任意）
        public FlashTargetData(uint netId, float duration)
        {
            this.netId = netId;
            this.flashDuration = duration;
            this.flashTimer = 0f;
            this.hasFlashed = false;
        }
    }


    [Header("Camera & Targets")]
    public Camera sourceCamera;

    public SyncList<FlashTargetData> targets = new SyncList<FlashTargetData>();

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
            // ネットワーク上のオブジェクトからRendererを取得
            Renderer rend = null;
            if (NetworkClient.spawned.TryGetValue(t.netId, out NetworkIdentity identity))
            {
                rend = identity.GetComponentInChildren<Renderer>();
            }

            // まだフラッシュしていない場合のみ、映っていたらフラッシュ開始
            if (!t.hasFlashed && IsVisibleFrom(rend, sourceCamera))
            {
                t.flashTimer = t.flashDuration;
                t.hasFlashed = true;
            }

            // タイマー > 0 の間はフラッシュ継続
            if (t.flashTimer > 0f)
            {
                t.flashTimer -= Time.deltaTime;
                shouldFlash = true;
            }
            else
            {
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
    /// ターゲット追加（Rendererを持つオブジェクト）
    /// </summary>
    public void AddTarget(GameObject obj, float duration)
    {
        if (obj == null) return;

        NetworkIdentity ni = obj.GetComponent<NetworkIdentity>();
        if (ni != null)
        {
            targets.Add(new FlashTargetData(ni.netId, duration));
        }
        else
        {
            Debug.LogWarning("AddTarget: オブジェクトにNetworkIdentityが必要です");
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
        GameObject canvasGO = new GameObject("OverlayCanvas");
        canvasGO.transform.SetParent(transform, false);
        overlayCanvas = canvasGO.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject imageGO = new GameObject("OverlayImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        overlayImage = imageGO.AddComponent<Image>();

        RectTransform rt = overlayImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);

        overlayImage.color = new Color(0f, 0f, 0f, 0f);
    }

    /// <summary>
    /// 指定カメラに映っているか判定（遮蔽物あり）
    /// </summary>
    bool IsVisibleFrom(Renderer rend, Camera cam)
    {
        if (rend == null || cam == null) return false;

        Vector3 viewPos = cam.WorldToViewportPoint(rend.bounds.center);

        if (viewPos.z < 0f) return false;
        if (viewPos.x < 0f || viewPos.x > 1f || viewPos.y < 0f || viewPos.y > 1f)
            return false;

        Vector3 dir = rend.bounds.center - cam.transform.position;
        if (Physics.Raycast(cam.transform.position, dir.normalized, out RaycastHit hit, dir.magnitude + 0.01f))
        {
            if (!hit.collider.transform.IsChildOf(rend.transform))
                return false;
        }

        return true;
    }
}
