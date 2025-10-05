using UnityEngine;

public class OfflineMove : MonoBehaviour
{

    public CharacterController controller;

    private bool _hasAnimator;

    public float xRotation = 0f; // 上下回転の累積値

    private float _airTime;

    private float _dashSound = 0.4f;

    private Vector3 _lastMoveDirection = Vector3.zero;

    private float _sensitivity = 1f;
    public Animator _animator;
    private float _speed;
    private float _animationBlend;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.25f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.29f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.8f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -18.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 3.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 6f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private int _animCrouching;

    public GameObject _mainCamera;
    public GameObject parentOfPlayer;
    private float offlineYaw;
    public Camera _CameraComponent;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animCrouching = Animator.StringToHash("Crouching");
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        CameraRotation();
        JumpAndGravity();
        GroundedCheck();
    }

    private void Move()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0); // 0 = Base Layer
        bool walk = Input.GetKey(KeyCode.LeftControl);
        bool crouch = stateInfo.IsName("Idle Crouching") || stateInfo.IsName("Idle CrouchingAiming");

        // **Input.GetAxis ではなく、Input.GetKey で即時判定**
        float horiMove = Input.GetAxisRaw("Horizontal");

        float verMove = Input.GetAxisRaw("Vertical");

        bool isMove = (horiMove != 0 || verMove != 0);



        if (!_animator.IsInTransition(0) && !crouch)
        {

            // **地上にいる場合のみ移動方向と速度を更新**
            if (Grounded)
            {
                float targetSpeed = 0f;

                // **目標速度を設定（入力がない場合は即座に0）**

                if (isMove)
                {


                    if (walk)
                    {
                        targetSpeed = MoveSpeed;
                    }
                    else
                    {
                        targetSpeed = SprintSpeed;
                    }



                }
                else
                {
                    targetSpeed = 0f;
                }


                // **Lerpを使わず、即座に速度を適用**
                _speed = targetSpeed;
                _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
                if (_animationBlend < 0.01f) _animationBlend = 0f;

                // **移動方向計算（正規化 + 速度適用）**
                Vector3 newMoveDirection = (transform.right * horiMove + transform.forward * verMove).normalized;
                if (newMoveDirection != Vector3.zero)
                {
                    _lastMoveDirection = newMoveDirection;
                }


            }
        }
        else
        {
            _speed = 0;
        }


        // **移動ベクトルを計算（空中では最後の移動方向を維持）**
        Vector3 moveDirection = _lastMoveDirection * _speed;
        moveDirection.y = _verticalVelocity; // 重力やジャンプのY軸速度を維持



        //Debug.Log("p" + moveDirection);
        // **CharacterControllerで移動**
        controller.Move(moveDirection * Time.deltaTime);

        // **アニメーター更新**
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, _speed);
            _animator.SetBool(_animCrouching, Input.GetKey(KeyCode.LeftShift));
        }



    }
    private void JumpAndGravity()
    {
        bool jump = Input.GetKeyDown(KeyCode.Space);

        if (Grounded)
        {
            _airTime = 0f; // 空中時間をリセット

            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = Gravity;
            }

            // Jump
            if (jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
            jump = false;
        }
        else
        {
            // 空中にいる時間を加算
            _airTime += Time.deltaTime;

            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void CameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 上下方向の回転を蓄積してClamp
        xRotation -= mouseY * _sensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (mouseX != 0 || mouseY != 0)
        {
            // カメラに上下回転を適用
            _mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            offlineYaw += mouseX * _sensitivity * (_CameraComponent.fieldOfView / 74.03f);
            parentOfPlayer.transform.rotation = Quaternion.Euler(0f, offlineYaw, 0f);
            
        }

    }
    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }
}
