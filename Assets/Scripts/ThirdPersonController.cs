using Mirror;
using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]

    public class ThirdPersonController : NetworkBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 2.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        public GameObject myBody;

        public LayerMask Invisible;

        // player
        private float _speed;
        private float _animationBlend;
        private Quaternion _targetRotation;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif
        private Animator _animator;
        private CharacterController _controller;
        private GameObject _mainCamera;
        private GameObject _UiCamera;

        private AudioListener _audioListener;

        private Camera _CameraComponent;
        private Camera _UiCameraComponent;

        private bool _hasAnimator;

        private float xRotation = 0f; // 上下回転の累積値

        private float _airTime;

        private float _dashSound = 0.4f;

        private Vector3 _lastMoveDirection = Vector3.zero;


        public AudioManager audioManager;


        public override void OnStartAuthority() 
        {
            if (_mainCamera == null)
            {
                _CameraComponent = GetComponentInChildren<Camera>();
                _mainCamera = _CameraComponent.gameObject;
                _UiCameraComponent = _mainCamera.transform.GetChild(0).GetComponent<Camera>();
                _UiCamera = _UiCameraComponent.gameObject;
                _audioListener = _mainCamera.GetComponent<AudioListener>();
                Canvas canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
                canvas.worldCamera = _CameraComponent;              
            }

            myBody.layer = 7;
            _CameraComponent.enabled = true;
            _UiCameraComponent.enabled = true;
        }

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();

            Move();
            Ability();

            if (RoundManager.rm.Mode == "Practice" && RoundManager.rm.CurrentPhase == RoundManager.Phase.BATTLE)
            {
                _audioListener.enabled = true;
            }
            else
            {
                _audioListener.enabled = false;
            }

        }

        private void LateUpdate()
        {
            if (!isLocalPlayer) return;
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
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

        private void CameraRotation()
        {
            // マウス入力を取得
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            xRotation = 0;
            // 上下方向の回転（カメラの俯仰）
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 上下の回転角度を制限


            if (mouseX != 0 || mouseY != 0)
            {
                // カメラに上下回転を適用
                _mainCamera.transform.localRotation *= Quaternion.Euler(xRotation, 0f, 0f);

                // プレイヤー身体に左右回転を適用
                transform.Rotate(Vector3.up * mouseX);
                
            }

        }

        private void Move()
        {

                bool walk = Input.GetKey(KeyCode.LeftShift);

            // **Input.GetAxis ではなく、Input.GetKey で即時判定**
            float horiMove =
                (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ? -1.0f :
                (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ? 1.0f : 0.0f;

            float verMove =
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? 1.0f :
                (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ? -1.0f : 0.0f;

            bool isMove = (horiMove != 0 || verMove != 0);



            // **地上にいる場合のみ移動方向と速度を更新**
            if (Grounded)
            {
                // **目標速度を設定（入力がない場合は即座に0）**
                float targetSpeed = isMove ? (walk ? MoveSpeed : SprintSpeed) : 0.0f;

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



            // **移動ベクトルを計算（空中では最後の移動方向を維持）**
            Vector3 moveDirection = _lastMoveDirection * _speed;
            moveDirection.y = _verticalVelocity; // 重力やジャンプのY軸速度を維持


            // **ダッシュ音の処理**
            if (_speed > 0)
            {
                _dashSound -= Time.deltaTime;
                if (_dashSound <= 0 && !walk)
                {
                    OnFootstep();
                    _dashSound = 0.4f;
                }
            }
            else if (_dashSound <= 0.4f)
            {
                _dashSound += Time.deltaTime;
            }


            Debug.Log("p" + moveDirection);
            // **CharacterControllerで移動**
            _controller.Move(moveDirection * Time.deltaTime);

            // **アニメーター更新**
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, _speed);
            }
        }

        private void Ability()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                AbilityController.ac.Lime();
            }
        }

        private void JumpAndGravity()
        {
            bool jump = Input.GetKey(KeyCode.Space);

            if (Grounded)
            {
                // 空中に0.7秒以上いた場合にOnLand()を呼び出す
                if (_airTime >= 0.7f)
                {
                    OnLand();
                }
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
                    _verticalVelocity = -2f;
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

        public void BotMove(float horiMove, bool isWalk)
        {

            bool isMove = (horiMove != 0);

            // **地上にいる場合のみ移動方向と速度を更新**
            if (Grounded)
            {
                // **目標速度を設定（入力がない場合は即座に0）**
                float targetSpeed = isMove ? (isWalk ? MoveSpeed : SprintSpeed) : 0.0f;

                // **Lerpを使わず、即座に速度を適用**
                _speed = targetSpeed;
                _animationBlend = targetSpeed;

                // **移動方向計算（正規化 + 速度適用）**
                Vector3 newMoveDirection = (transform.right * horiMove).normalized;
                if (newMoveDirection != Vector3.zero)
                {
                    _lastMoveDirection = newMoveDirection;
                }
            }

            // **移動ベクトルを計算（空中では最後の移動方向を維持）**
            Vector3 moveDirection = _lastMoveDirection * _speed;
            moveDirection.y = _verticalVelocity; // 重力やジャンプのY軸速度を維持

            

            // **ダッシュ音の処理**
            if (_speed > 0)
            {
                _dashSound -= Time.deltaTime;
                if (_dashSound <= 0)
                {
                    OnFootstep();
                    _dashSound = 0.4f;
                }
            }
            else if (_dashSound <= 0.4f)
            {
                _dashSound += Time.deltaTime;
            }


            Debug.Log("b" + moveDirection);
            // **CharacterControllerで移動**
            _controller.Move(moveDirection * Time.deltaTime);

            // **アニメーター更新**
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, _speed);
            }
        }

        public void BotStop()
        {
            _animator.SetFloat(_animIDSpeed, 0);
            _animator.SetFloat(_animIDMotionSpeed, 0);
        }


        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }




 

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep()
        {
            if (Grounded)
            {

                audioManager.CmdPlaySoundAtPoint("footStep", transform.TransformPoint(_controller.center), FootstepAudioVolume);
                    
                
            }
        }

        private void OnLand()
        {

            audioManager.CmdPlaySoundAtPoint("land", transform.TransformPoint(_controller.center), FootstepAudioVolume);
            
        }

        public GameObject GetCamera()
        {
            return _mainCamera;
        }

        public float GetSpeed()
        {
            return _speed;
        }

        [Server]
        public void ResetPos(Vector3 pos)
        {
            
            RpcUpdateAllPositions(pos);
            
        }

        [ClientRpc]
        void RpcUpdateAllPositions(Vector3 newPos)
        {
            _controller.enabled = false;
            transform.position = newPos;
            _controller.enabled = true;
        }
    }
}