using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class RebindItem : MonoBehaviour
{
    [Header("Input Settings")]
    public InputActionReference targetAction;
    [Tooltip("キーボード用バインドインデックス")]
    public int keyboardBindingIndex = 0;
    [Tooltip("ゲームパッド用バインドインデックス")]
    public int gamepadBindingIndex = 1;

    [Header("UI")]
    public TMP_Text keyboardText;
    public TMP_Text gamepadText;

    private InputActionRebindingExtensions.RebindingOperation currentRebind;

    void Start()
    {
        UpdateDisplayTexts();
    }

    // =========================
    // キーボード用リバインド
    // =========================
    public void StartKeyboardRebinding()
    {
        StartRebinding(keyboardBindingIndex, keyboardText);
    }

    // =========================
    // ゲームパッド用リバインド
    // =========================
    public void StartGamepadRebinding()
    {
        StartRebinding(gamepadBindingIndex, gamepadText);
    }

    // 共通のリバインド処理
    private void StartRebinding(int bindingIndex, TMP_Text targetText)
    {
        if (currentRebind != null)
            return;

        targetText.text = "押してください...";
        targetAction.action.Disable();

        currentRebind = targetAction.action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse/delta")
            .WithControlsExcluding("Mouse/position")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0f)
            .OnComplete(op => EndRebinding(op))
            .OnCancel(op => CancelRebinding(op));

        currentRebind.Start();

        // スクロール対応（マウスホイールもキーにできる）
        StartCoroutine(CheckMouseScroll(bindingIndex));
    }

    // マウススクロール対応
    private IEnumerator CheckMouseScroll(int bindingIndex)
    {
        while (currentRebind != null && currentRebind.started)
        {
            if (Mouse.current != null)
            {
                var scroll = Mouse.current.scroll.ReadValue();
                if (scroll.y > 0.1f)
                {
                    targetAction.action.ApplyBindingOverride(bindingIndex, "<Mouse>/scroll/up");
                    currentRebind.Complete();
                }
                else if (scroll.y < -0.1f)
                {
                    targetAction.action.ApplyBindingOverride(bindingIndex, "<Mouse>/scroll/down");
                    currentRebind.Complete();
                }
            }
            yield return null;
        }
    }

    private void EndRebinding(InputActionRebindingExtensions.RebindingOperation operation)
    {
        operation.Dispose();
        currentRebind = null;

        targetAction.action.Enable();
        UpdateDisplayTexts();
    }

    private void CancelRebinding(InputActionRebindingExtensions.RebindingOperation operation)
    {
        operation.Dispose();
        currentRebind = null;

        targetAction.action.Enable();
        UpdateDisplayTexts();
    }

    private void UpdateDisplayTexts()
    {
        if (keyboardText != null)
            keyboardText.text = targetAction.action.bindings[keyboardBindingIndex].ToDisplayString();

        if (gamepadText != null)
            gamepadText.text = targetAction.action.bindings[gamepadBindingIndex].ToDisplayString();
    }

    // 保存
    public string SaveBindingsToJson()
    {
        return targetAction.action.SaveBindingOverridesAsJson();
    }

    // 読み込み
    public void LoadBindingsFromJson(string json)
    {
        targetAction.action.LoadBindingOverridesFromJson(json);
        UpdateDisplayTexts();
    }
}
