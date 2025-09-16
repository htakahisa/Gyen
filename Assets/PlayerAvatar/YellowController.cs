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

    public GameObject parentOfPlayer;


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
        if (!isLocalPlayer) return;
        if (abilityController.currentForm != AbilityController.PlayerForm.Bird) return;
        CharacterMove();
        CharacterRotation();

    }

    private void CharacterRotation()
    {

        if (!RoundManager.rm.hasLoaded)
        {
            return;
        }

        // マウスの水平移動量を取得
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 目標値をマウス移動に応じて変化させる
        targetValue = mouseX * _sensitivity;

        // 現在値を滑らかに目標値に近づける
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * smoothSpeed);

        transformNetwork.yaw += currentValue;
        parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);
        transformNetwork.CmdRotate(parentOfPlayer.transform.rotation);
        mainCamera.transform.localRotation *= Quaternion.Euler(-mouseY, 0f, 0f);
    }

    public void CharacterMove()
    {
        characterController.Move(transform.forward * MoveSpeed * Time.deltaTime);
        transformNetwork.CmdPos(parentOfPlayer.transform.position);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
        {
            characterController.Move(transform.up * UpSpeed * Time.deltaTime);
            transformNetwork.CmdPos(parentOfPlayer.transform.position);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftShift))
        {
            characterController.Move(-transform.up * UpSpeed * Time.deltaTime);
            transformNetwork.CmdPos(parentOfPlayer.transform.position);
        }

        audioTimer += Time.deltaTime;

        if (audioTimer >= audioInterval)
        {
            audioManager.CmdPlaySoundAtPoint("yellow", transform.TransformPoint(characterController.center), audioVolume);
            audioTimer = 0;
        }

    }
}