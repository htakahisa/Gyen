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

        // --- 左右回転(Yaw) ---
        // マウス移動から目標値を計算
        targetValue = mouseX * _sensitivity;
        targetValue = Mathf.Clamp(targetValue, -90, 90);

        // 現在値を滑らかに補間
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * smoothSpeed);

        // プレイヤー本体を回転
        transformNetwork.yaw += currentValue;
        parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);
        //transformNetwork.CmdRotate(parentOfPlayer.transform.rotation);

        // --- 上下回転(Pitch) ---
        xRotation -= mouseY * _sensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // カメラに上下回転を適用（ *= ではなく = ）
        mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
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