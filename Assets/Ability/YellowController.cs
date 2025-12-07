using Mirror;
using UnityEngine;


public class YellowController : NetworkBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 4.0f;
    public float UpSpeed = 4.0f;
    public float TurnSpeed = 0.3f;

    public CharacterController characterController;
    public AudioManager audioManager;

    public float smoothSpeed = 4f;     // 変化の滑らかさ（大きいほど速く追従）

    private float targetValue = 0f;    // 目標値
    private float currentValue = 0f;   // 現在値（滑らかに変化する）
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


    public override void OnStartAuthority()
    {
        gameObject.layer = 7;
        _sensitivity = PlayerPrefs.GetFloat("Sensitivity");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame

    private void LateUpdate()
    {

        if (isLocalPlayer)
        {
            if (abilityController.currentForm != AbilityController.PlayerForm.Bird) return;
            CharacterMove();
            CharacterRotation();
        }

        if ((RoundManager.rm.currentMode == RoundManager.Mode.PRACTICE) || (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE && isLocalPlayer) || (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND && isLocalPlayer))
        {
            audioListener.enabled = true;
        }
        else
        {
            audioListener.enabled = false;
        }


    }


    private void CharacterRotation()
    {
        if (!RoundManager.rm.hasLoaded) return;

        // マウス入力
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // ==========================================================
        // ★ Yaw（左右）: 360°無限回転 → Transform から毎回取得
        // ==========================================================
        float currentYaw = parentOfPlayer.transform.eulerAngles.y;

        float targetYawDelta = mouseX * _sensitivity * 10;
        float smoothedYawDelta = Mathf.Lerp(0, targetYawDelta, Time.deltaTime * smoothSpeed);

        float newYaw = currentYaw + smoothedYawDelta;

        parentOfPlayer.transform.rotation = Quaternion.Euler(0, newYaw, 0);


        // ==========================================================
        // ★ Pitch（上下）: -90〜90° → localEulerAngles から取得
        // ==========================================================
        float currentPitch = mainCamera.transform.localEulerAngles.x;

        // Unity の角度仕様補正（181〜360° → -179〜0°）
        if (currentPitch > 180f) currentPitch -= 360f;

        float targetPitchDelta = mouseY * _sensitivity * 10;
        float smoothedPitchDelta = Mathf.Lerp(0, targetPitchDelta, Time.deltaTime * smoothSpeed);

        float newPitch = Mathf.Clamp(currentPitch - smoothedPitchDelta, -90f, 90f);

        mainCamera.transform.localRotation = Quaternion.Euler(newPitch, 0, 0);
    }



    public void CharacterMove()
    {
        if (characterController == null) return;
        characterController.Move(transform.forward * MoveSpeed * Time.deltaTime);
        //transformNetwork.CmdPos(parentOfPlayer.transform.position);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
        {
            characterController.Move(transform.up * UpSpeed * Time.deltaTime);
            //transformNetwork.CmdPos(parentOfPlayer.transform.position);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftShift))
        {
            characterController.Move(-transform.up * UpSpeed * Time.deltaTime);
            //transformNetwork.CmdPos(parentOfPlayer.transform.position);
        }

        audioTimer += Time.deltaTime;

        if (audioTimer >= audioInterval)
        {
            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.YELLOW, transform.TransformPoint(characterController.center), audioVolume, 15);
            audioTimer = 0;
        }

    }
}