using Mirror;
using UnityEngine;


public class YellowController : NetworkBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 3.0f;
    public float TurnSpeed = 0.3f;

    public CharacterController characterController;


    public float smoothSpeed = 5f;     // 変化の滑らかさ（大きいほど速く追従）

    private float targetValue = 0f;    // 目標値
    private float currentValue = 0f;   // 現在値（滑らかに変化する）
    private float _sensitivity = 1f;

    public GameObject mainCamera;




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

        // プレイヤー身体に左右回転を適用
        RoundManager.rm.GetMyPlayer().transform.Rotate(Vector3.up * currentValue);
        mainCamera.transform.localRotation *= Quaternion.Euler(-mouseY, 0f, 0f);
    }

    public void CharacterMove()
    {
        characterController.Move(transform.forward * MoveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
        {
            characterController.Move(transform.up * MoveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftShift))
        {
            characterController.Move(-transform.up * MoveSpeed * Time.deltaTime);
        }

    }
}