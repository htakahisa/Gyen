using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class RebindSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Input System Action")]
    public InputActionReference targetAction;
    public int bindingIndex = 0;

    [Header("UIï\é¶óp")]
    public TMP_Text displayText;

    private InputActionRebindingExtensions.RebindingOperation currentRebind;
    private const string PREFS_KEY = "InputBindings_";

    void Start()
    {
        LoadBinding();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartRebind();
    }

    private void StartRebind()
    {
        if (currentRebind != null) return;
        if (targetAction == null) return;

        EventSystem.current.sendNavigationEvents = false;
        targetAction.action.Disable();
        displayText.text = "ì¸óÕë“Çø...";

        currentRebind = targetAction.action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse/position")
            .WithControlsExcluding("Mouse/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0f)
            .OnComplete(op => EndRebind())
            .OnCancel(op => EndRebind());

        currentRebind.Start();
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

        SaveBinding();
        UpdateDisplayText();
    }

    private void SaveBinding()
    {
        string json = targetAction.action.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(PREFS_KEY + targetAction.action.name, json);
        PlayerPrefs.Save();
    }

    public void LoadBinding()
    {
        if (targetAction == null) return;

        string key = PREFS_KEY + targetAction.action.name;
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            targetAction.action.LoadBindingOverridesFromJson(json);
        }

        UpdateDisplayText();
    }

    public void ResetBinding()
    {
        if (targetAction == null) return;

        targetAction.action.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PREFS_KEY + targetAction.action.name);
        UpdateDisplayText();
    }

    private void UpdateDisplayText()
    {
        if (displayText == null || targetAction == null)
        {
            if (displayText != null) displayText.text = "-";
            return;
        }

        var bind = targetAction.action.bindings[bindingIndex];
        displayText.text = string.IsNullOrEmpty(bind.effectivePath)
            ? "ñ¢ê›íË"
            : InputControlPath.ToHumanReadableString(bind.effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
}
