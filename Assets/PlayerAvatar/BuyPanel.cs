using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuyPanel : NetworkBehaviour
{
    private GameObject player;

    public GameObject panel;

    public bool isCursorLocked = true;

    public static BuyPanel buyPanel;

    private bool hasLoaded = false;

    private PlayerInputActions inputActions;
    [Header("Cursor Settings")]
    public RectTransform cursorUI;    // Canvas上のImage
    public float cursorSpeed = 1000f; // ピクセル/秒
    private Vector2 cursorPos;

    // Start is called before the first frame update
    public void Awake()
    {
        buyPanel = this;
        panel.SetActive(false);
        // 初期状態でカーソルをロックし、非表示にする
        LockCursor();

        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.Menu.performed += _ => OpenPanel();
        inputActions.Player.Select.performed += _ => OnClick();
    }

    

    void StartGetPlayer()
    {
        player = RoundManager.rm.GetMyPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if (RoundManager.rm != null)
        {
            if (RoundManager.rm.hasLoaded && RoundManager.rm.GetMyPlayer().GetComponent<PlayerManager>().hasLoaded && !hasLoaded)
            {
                StartGetPlayer();
                hasLoaded = true;
            }
        }

        if (RoundManager.rm.CurrentPhase != RoundManager.Phase.BUY)
        {
            panel.SetActive(false);
            LockCursor();
        }

        Vector2 input = inputActions.Player.Look.ReadValue<Vector2>();

        if (RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>().currentControlScheme == "Keyboard&Mouse")
        {
            cursorSpeed = 100f;
        }
        else if (RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>().currentControlScheme == "Gamepad")
        {
            cursorSpeed = 1000f;
        }


        // 位置更新
        cursorPos += input * cursorSpeed * Time.deltaTime;

        // 画面内に制限
        cursorPos.x = Mathf.Clamp(cursorPos.x, 0, Screen.width);
        cursorPos.y = Mathf.Clamp(cursorPos.y, 0, Screen.height);

        // Canvas 上のカーソル位置に反映
        if (cursorUI != null)
        {
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cursorUI.parent as RectTransform,
                cursorPos,
                null, // カメラがCanvasに割り当てられている場合は指定
                out anchoredPos
            );
            cursorUI.anchoredPosition = anchoredPos;
        }

        // UI Raycast を更新してUI選択を追従させる
        UpdateUIRaycast();

    }


    private void UpdateUIRaycast()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = cursorPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // 選択可能なUIがあれば最初のものを選択
        if (results.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(results[0].gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void OnClick()
    {

        StartCoroutine(InvertColor());
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = cursorPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var hit in results)
        {
            ExecuteEvents.Execute(hit.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
        }
    }

    public IEnumerator InvertColor()
    {
        Color original = cursorUI.GetComponent<Image>().color;
        Color inverted = new Color(1f - original.r, 1f - original.g, 1f - original.b, original.a);
        cursorUI.GetComponent<Image>().color = inverted;
        
        yield return new WaitForSeconds(0.1f);

        original = cursorUI.GetComponent<Image>().color;
        inverted = new Color(1f - original.r, 1f - original.g, 1f - original.b, original.a);
        cursorUI.GetComponent<Image>().color = inverted;

    }

    public void OpenPanel() {


        panel.SetActive(!panel.activeSelf);
        // 初期位置を画面中央に
        cursorPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (isCursorLocked)
        {
                UnlockCursor();
        }
        else
        {
                LockCursor();
            
        }
    }
        

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        isCursorLocked = true;
        cursorUI.gameObject.SetActive(false);
    }

    void UnlockCursor()
    {
        isCursorLocked = false;
        cursorUI.gameObject.SetActive(true);
        
    }

    // シーン切り替え時などにカーソル状態がリセットされないようにする
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            if (isCursorLocked)
            {
                LockCursor();
            }
        }
    }
}
