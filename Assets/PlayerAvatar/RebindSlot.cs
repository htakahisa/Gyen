using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class RebindSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Input System Action")]
    [Tooltip("操作を割り当てる InputActionReference")]
    public InputActionReference targetAction;

    [Tooltip("同じ Action 内で複数のバインドがある場合のインデックス")]
    public int bindingIndex = 0;

    [Header("UI表示用")]
    public TMP_Text displayText;

    private InputActionRebindingExtensions.RebindingOperation currentRebind;

    void Start()
    {
        // Awake時では InputSystem の初期化が終わっていない可能性があるので Start で実行
        UpdateDisplayText();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            StartRebind();
    }

    private void StartRebind()
    {
        if (targetAction == null || currentRebind != null)
            return;

        EventSystem.current.sendNavigationEvents = false;
        targetAction.action.Disable();
        displayText.text = "入力待ち...";

        // --- Rebind 開始 ---
        currentRebind = targetAction.action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse/position")
            .WithControlsExcluding("Mouse/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0f)
            .OnComplete(op => EndRebind())
            .OnCancel(op => CancelRebind());

        currentRebind.Start();

        // マウスホイールも検知できるように
        StartCoroutine(CheckMouseScroll());
    }

    private IEnumerator CheckMouseScroll()
    {
        while (currentRebind != null && currentRebind.started)
        {
            if (Mouse.current != null)
            {
                var scroll = Mouse.current.scroll.ReadValue();
                if (scroll.y > 0.1f)
                {
                    targetAction.action.ApplyBindingOverride(bindingIndex, "<Mouse>/scroll/up");
                    EndRebind();
                }
                else if (scroll.y < -0.1f)
                {
                    targetAction.action.ApplyBindingOverride(bindingIndex, "<Mouse>/scroll/down");
                    EndRebind();
                }
            }
            yield return null;
        }
    }

    private void EndRebind()
    {
        currentRebind?.Dispose();
        currentRebind = null;

        targetAction.action.Enable();
        EventSystem.current.sendNavigationEvents = true;

        // UI更新
        UpdateDisplayText();

        // RebindManagerに通知して全体保存
        var manager = FindObjectOfType<RebindManager>();
        if (manager != null)
            manager.SaveAll();
    }

    private void CancelRebind()
    {
        currentRebind?.Dispose();
        currentRebind = null;

        targetAction.action.Enable();
        EventSystem.current.sendNavigationEvents = true;

        UpdateDisplayText();
    }

    public void ResetBinding()
    {
        if (targetAction == null) return;

        targetAction.action.RemoveBindingOverride(bindingIndex);
        UpdateDisplayText();
    }

    // --- RebindManager から呼ばれるため public ---
    public void UpdateDisplayText()
    {
        if (displayText == null || targetAction == null)
        {
            if (displayText != null)
                displayText.text = "-";
            return;
        }

        if (bindingIndex >= targetAction.action.bindings.Count)
        {
            displayText.text = "不正なバインド";
            return;
        }

        var binding = targetAction.action.bindings[bindingIndex];
        string readable = string.IsNullOrEmpty(binding.effectivePath)
            ? "未設定"
            : InputControlPath.ToHumanReadableString(
                binding.effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);

        displayText.text = readable;
    }
}
