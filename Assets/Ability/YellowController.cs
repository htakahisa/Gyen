using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class YellowController : NetworkBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 4.0f;
    public float UpSpeed = 4.0f;
    public float TurnSpeed = 0.3f;

    public CharacterController characterController;
    public AudioManager audioManager;

    public float smoothSpeed = 4f;     // カメラ回転スムーズ
    private float _sensitivity = 1f;

    public GameObject mainCamera;
    public AbilityController abilityController;
    public CharacterTransfromNetwork transformNetwork;

    public float audioVolume;
    public float audioInterval;
    public float audioTimer;

    private float xRotation = 0f;
    public GameObject parentOfPlayer;

    public AudioListener audioListener;
    private PlayerInput playerInput;

    public string currentControlScheme = "Keyboard&Mouse";

    // ★★★ 回転スムージング用 ★★★
    private float yawCurrent = 0f;
    private float pitchCurrent = 0f;

    private float yawVelocity = 0f;
    private float pitchVelocity = 0f;

    private bool rotationInitialized = false;

    public override void OnStartAuthority()
    {
        gameObject.layer = 7;
        _sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f);
    }


    private void Awake()
    {
        if (isLocalPlayer)
            StartCoroutine(InitialSetInput());
    }


    public IEnumerator InitialSetInput()
    {
        if (!isLocalPlayer) yield break;
        yield return new WaitForSeconds(0.05f);
        playerInput = GetComponentInParent<PlayerInput>();
    }



    private void LateUpdate()
    {
        if (isLocalPlayer)
        {
            if (abilityController.currentForm != AbilityController.PlayerForm.Bird) return;

            CharacterMove();
            CharacterRotation();
        }

        // AudioListener 有効化設定
        if ((RoundManager.rm.currentMode == RoundManager.Mode.PRACTICE) ||
            (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE && isLocalPlayer) ||
            (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND && isLocalPlayer))
        {
            audioListener.enabled = true;
        }
        else
        {
            audioListener.enabled = false;
        }
    }



    private void SwitchScheme(string schemeName, params InputDevice[] devices)
    {
        if (playerInput != null && playerInput.user.valid)
        {
            if (playerInput.currentControlScheme != schemeName)
            {
                playerInput.SwitchCurrentControlScheme(schemeName, devices);
                currentControlScheme = schemeName;
                Debug.Log($"Switched to: {schemeName}");
            }
        }
        else
        {
            Debug.Log("プレイヤーインプットがまだ準備できてません");
        }
    }



    private void CharacterRotation()
    {
        if (!RoundManager.rm.hasLoaded) return;
        if (playerInput == null) return;

        // ------------------------------------------------------
        // ★ 初回だけ、現在の角度を取り込む（他クラスが設定した角度を尊重）
        // ------------------------------------------------------
        if (!rotationInitialized)
        {
            // --- Yaw（水平） ---
            float startYaw = parentOfPlayer.transform.eulerAngles.y;
            if (startYaw > 180f) startYaw -= 360f;   // 正規化
            yawCurrent = startYaw;

            // --- Pitch（上下） ---
            float startPitch = mainCamera.transform.localEulerAngles.x;
            if (startPitch > 180f) startPitch -= 360f; // 正規化 (-180〜180)
            pitchCurrent = Mathf.Clamp(startPitch, -90f, 90f);

            rotationInitialized = true;
        }

        // ------------------------------------------------------
        // Look Input
        // ------------------------------------------------------
        Vector2 lookInput = playerInput.actions["Look"].ReadValue<Vector2>();

        if (lookInput.sqrMagnitude > 0.01f)
        {
            if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
                SwitchScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
            else if (Gamepad.current != null)
                SwitchScheme("Gamepad", Gamepad.current);
        }

        float magnification = (currentControlScheme == "Keyboard&Mouse") ? 1f : 100f;
        float mouseX = lookInput.x * _sensitivity * magnification;
        float mouseY = lookInput.y * _sensitivity * magnification;

        // ------------------------------------------------------
        // Yaw（水平回転）
        // ------------------------------------------------------
        float yawTarget = yawCurrent + mouseX;

        // ★ ここも正規化してズレを防ぐ
        if (yawTarget > 180f) yawTarget -= 360f;
        else if (yawTarget < -180f) yawTarget += 360f;

        yawCurrent = Mathf.SmoothDamp(
            yawCurrent,
            yawTarget,
            ref yawVelocity,
            1f / smoothSpeed
        );

        parentOfPlayer.transform.rotation = Quaternion.Euler(0f, yawCurrent, 0f);

        // ------------------------------------------------------
        // Pitch（上下回転）
        // ------------------------------------------------------
        float pitchTarget = Mathf.Clamp(pitchCurrent - mouseY, -90f, 90f);

        pitchCurrent = Mathf.SmoothDamp(
            pitchCurrent,
            pitchTarget,
            ref pitchVelocity,
            1f / smoothSpeed
        );

        mainCamera.transform.localRotation = Quaternion.Euler(pitchCurrent, 0f, 0f);
    }





    // ==========================================================
    // 移動（あなたの元コードそのまま）
    // ==========================================================
    public void CharacterMove()
    {
        if (characterController == null) return;

        characterController.Move(transform.forward * MoveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
        {
            characterController.Move(transform.up * UpSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftShift))
        {
            characterController.Move(-transform.up * UpSpeed * Time.deltaTime);
        }

        audioTimer += Time.deltaTime;

        if (audioTimer >= audioInterval)
        {
            AudioManager.Instance.CmdPlaySoundAtPoint(
                AudioManager.Sounds.YELLOW,
                transform.TransformPoint(characterController.center),
                audioVolume,
                15
            );
            audioTimer = 0;
        }
    }
}
