using Mirror;
using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{


    public class ThirdPersonController : NetworkBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 3.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

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

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.25f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.29f;

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
        public LayerMask OrbMask;

        [SyncVar]
        [SerializeField]
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
        private int _animDance;
        private int _animCrouching;
        private int _animReloading;
        private int _animZooming;

        public Transform middleBrow;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif
        private Animator _animator;
        public CharacterController controller;
        private GameObject _mainCamera;
        private GameObject _UiCamera;

        private AudioListener _audioListener;

        private Camera _CameraComponent;
        private Camera _UiCameraComponent;

        private ShootManager _shootManager;
        private HpMaster _hpMaster;

        private bool _hasAnimator;

        public float xRotation = 0f; // 上下回転の累積値

        private float _airTime;

        private float _dashSound = 0.4f;

        private Vector3 _lastMoveDirection = Vector3.zero;

        private float _sensitivity = 1f;

        public AudioManager audioManager;

        public bool canGetOrb = true;
        public GameObject Aiscream;

        GameObject BgmObject;
        public GameObject parentOfPlayer;

        public CharacterTransfromNetwork transformNetwork;

        public Coroutine stunCoroutine;

        public override void OnStartAuthority() 
        {
            if (_mainCamera == null)
            {
                _CameraComponent = GetComponentInChildren<Camera>();
                _mainCamera = _CameraComponent.gameObject;
                _UiCameraComponent = _mainCamera.transform.GetChild(0).GetComponent<Camera>();
                _UiCamera = _UiCameraComponent.gameObject;
                Canvas canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
                canvas.worldCamera = _CameraComponent;
                _shootManager = GetComponent<ShootManager>();
                _hpMaster = GetComponentInParent<HpMaster>();
            }

            myBody.layer = 7;
            _CameraComponent.enabled = true;
            _UiCameraComponent.enabled = true;
        }

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
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

            GroundedCheck();

            if (_mainCamera != null)
            {
                _mainCamera.transform.position = middleBrow.position;
            }

            if (_audioListener == null)
            {
                _CameraComponent = GetComponentInChildren<Camera>();
                _mainCamera = _CameraComponent.gameObject;
                _audioListener = _mainCamera?.GetComponent<AudioListener>();
            }
            else
            {

                    if (RoundManager.rm.Mode == "Practice" || (RoundManager.rm.Mode == "1VS1" && isLocalPlayer))
                    {
                        _audioListener.enabled = true;
                    }
                    else
                    {
                        _audioListener.enabled = false;
                    }

            }

            if (!isLocalPlayer)
            {
                return;
            }

            _sensitivity = PlayerPrefs.GetFloat("Sensitivity");
            _hasAnimator = TryGetComponent(out _animator);

            if (GetComponentInParent<PlayerManager>().canMove)
            {
                JumpAndGravity();

                Move();
            }

            if (canGetOrb)
            {
                GetOrb();
            }

            if(Input.GetKeyDown(KeyCode.P))
            {
                if (_animator.GetInteger(_animDance) == 0)
                {
                    CmdCallDance();
                }
                else
                {
                    CmdCallEndDance();
                }
            }

            

        }




        [Command]
        public void  CmdCallDance()
        {
            NetworkServer.Destroy(BgmObject);
            TargetDance();
            BgmObject = Instantiate(Aiscream, transform.position, Quaternion.identity);
            NetworkServer.Spawn(BgmObject);
        }

        [Command]
        public void CmdCallEndDance()
        {
            NetworkServer.Destroy(BgmObject);
            TargetEndDance();
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
            _animDance = Animator.StringToHash("Dance");
            _animCrouching = Animator.StringToHash("Crouching");
            _animReloading = Animator.StringToHash("Reloading");
            _animZooming = Animator.StringToHash("Zooming");
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
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // 上下方向の回転を蓄積してClamp
            xRotation -= mouseY * _sensitivity;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            if (mouseX != 0 || mouseY != 0)
            {
                // カメラに上下回転を適用
                _mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

                // --- 身体の左右回転 (Yaw) ---
                transformNetwork.yaw += mouseX * _sensitivity * (_CameraComponent.fieldOfView / 74.03f);
                parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);
                transformNetwork.CmdRotate(parentOfPlayer.transform.rotation);
            }

        }
        public void CameraParticularRotaion(float pitch)
        {
            // ピッチをClamp
            xRotation = Mathf.Clamp(-pitch, -90f, 90f);

            // 体のYawと合成
            _mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0f);
        }


        public void CameraRecoil(float recoil)
        {

            // 上下方向の回転を蓄積してClamp
            xRotation -= recoil;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // カメラに上下回転を適用
            _mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            

        }

        [TargetRpc]
        public void TargetCameraStun(NetworkConnection target, float stunLevel, float duration)
        {
            if(stunCoroutine == null)
            stunCoroutine = StartCoroutine(StunCoroutine(stunLevel, duration));
        }

        public IEnumerator StunCoroutine(float stunLevel, float duration)
        {
            float endTime = Time.time + duration;
            float xRotation = _mainCamera.transform.localRotation.eulerAngles.x;

            while (Time.time <= endTime)
            {
                float random = (Random.Range(0, 2) == 0) ? 1f : -1f;

                // カメラの上下回転
                xRotation -= stunLevel * random;
                _mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

                // 身体の左右回転 (Yaw)
                transformNetwork.yaw += random * stunLevel * (_CameraComponent.fieldOfView / 74.03f);
                parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);
                transformNetwork.CmdRotate(parentOfPlayer.transform.rotation);

                yield return null; // 毎フレーム繰り返す
            }

            stunCoroutine = null;

        }

        private void Move()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0); // 0 = Base Layer
            bool walk = Input.GetKey(KeyCode.LeftControl);
            bool crouch = stateInfo.IsName("Idle Crouching") || stateInfo.IsName("Idle CrouchingAiming");

            bool sneak = _shootManager.IsZooming;

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

                        if (sneak)
                        {
                            targetSpeed *= 0.5f;
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


            CmdReportSpeed(_speed);

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


            //Debug.Log("p" + moveDirection);
            // **CharacterControllerで移動**
            controller.Move(moveDirection * Time.deltaTime);
            transformNetwork.CmdPos(transform.position);

            // **アニメーター更新**
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, _speed);
                _animator.SetBool(_animCrouching, Input.GetKey(KeyCode.LeftShift));
            }



        }

        public void ResetLastMove()
        {
            _lastMoveDirection = new Vector3(0,0,0);
        }


        private void GetOrb()
        {

            if(Input.GetKey(KeyCode.F)){

                try
                {
                    foreach (var orbs in GameObject.FindGameObjectsWithTag("Orb"))
                    {
                        Vector3 direction = orbs.transform.position - _mainCamera.transform.position;

                        if (Physics.Raycast(_mainCamera.transform.position, direction, out RaycastHit hit, 100, OrbMask))
                        {   
                            if (hit.distance <= 1.5f)
                            {
                                hit.collider.gameObject.GetComponent<GetEffect>().Active();
                            }   
                        }
                        
                    }
                }
                catch
                {

                }
            }

        }

        private void JumpAndGravity()
        {
            bool jump = Input.GetKeyDown(KeyCode.Space);

            if (Grounded)
            {
                // 空中に0.5秒以上いた場合にOnLand()を呼び出す
                if (_airTime >= 0.5f)
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

        public void Reloading() 
        {
            if (!isLocalPlayer) return;
            _animator.SetBool(_animReloading, true);
        }
        public void EndReloading()
        {
            if (!isLocalPlayer) return;
            _animator.SetBool(_animReloading, false);
        }

        [Command (requiresAuthority = false)]
        public void CmdChangeGunType(string type)
        {
            RpcChangeGunType(type);
        }

        [ClientRpc]
        public void RpcChangeGunType(string type)
        {
            if (!isLocalPlayer) return;
            if (!_hasAnimator) return;
            if (type == "Rifle")
            {
                // "RifleLayer" というレイヤーの重みを1にして有効化
                _animator.SetLayerWeight(_animator.GetLayerIndex("RifleLayer"), 1f);

                // "PistolLayer" の重みを0にして無効化
                _animator.SetLayerWeight(_animator.GetLayerIndex("PistolLayer"), 0f);
            }
            if (type == "Pistol")
            {
                // "RifleLayer" というレイヤーの重みを1にして有効化
                _animator.SetLayerWeight(_animator.GetLayerIndex("RifleLayer"), 0f);

                // "PistolLayer" の重みを0にして無効化
                _animator.SetLayerWeight(_animator.GetLayerIndex("PistolLayer"), 1f);
            }
        }

        public void BotMove(float horiMove, bool isWalk, bool isCrouch)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0); // 0 = Base Layer
            bool crouch = stateInfo.IsName("Idle Crouching") || stateInfo.IsName("Idle CrouchingAiming");
            bool isMove = (horiMove != 0);

            _animator.SetBool(_animCrouching, isCrouch);

            // 立ち上がりアニメーション中なら移動禁止
            if (!_animator.IsInTransition(0) && !crouch)
            {


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
            }
            else
            {
                _speed = 0;
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


            //Debug.Log("b" + moveDirection);
            // **CharacterControllerで移動**
            controller.Move(moveDirection * Time.deltaTime);
            transformNetwork.ServerPos(parentOfPlayer.transform.position);

            // **アニメーター更新**
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, _speed);
            }
        }

        public void BotJumpAndGravity(bool jump)
        {

            if (Grounded)
            {
                transform.rotation = Quaternion.identity;

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

                float mouseX = 2;
                float mouseY = 2;

                xRotation = 0;
                // 上下方向の回転（カメラの俯仰）
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 上下の回転角度を制限


                if (mouseX != 0 || mouseY != 0)
                {
                    if (_mainCamera != null)
                    {
                        // カメラに上下回転を適用
                        _mainCamera.transform.localRotation *= Quaternion.Euler(xRotation, 0f, 0f);

                    }
                    if (_CameraComponent != null)
                    {
                        // プレイヤー身体に左右回転を適用
                        transformNetwork.yaw += mouseX * _sensitivity * (_CameraComponent.fieldOfView / 74.03f);
                        parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);
                        transformNetwork.CmdRotate(parentOfPlayer.transform.rotation);
                    }
                    else
                    {
                        Debug.Log("The mainCamera attached this object is not detected!");
                    }

                }

                // 空中にいる時間を加算
                _airTime += Time.deltaTime;

                // reset the jump timeout timer
                //_jumpTimeoutDelta = JumpTimeout;

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

                AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.FOOTSTEP, transform.TransformPoint(controller.center), FootstepAudioVolume);
                    
                
            }
        }

        private void OnLand()
        {

            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.LAND, transform.TransformPoint(controller.center), FootstepAudioVolume);
            
        }

        public GameObject GetCamera()
        {
            return _mainCamera;
        }

        [Command]
        void CmdReportSpeed(float clientSpeed)
        {
            _speed = clientSpeed;  // サーバー上で保持＆SyncVarにより全クライアントに反映される
        }

        public float GetSpeed()
        {
            return _speed;  // 呼び出したクライアント上で最後に受信した値（SyncVar）
        }


        [Server]
        public void ResetPos(Vector3 pos)
        {
            
            ServerUpdateAllPositions(pos);
            
        }

        [ClientRpc]
        public void RpcDance()
        {
            GetComponentInParent<PlayerManager>().canMove = false;
            GetComponentInParent<PlayerManager>().canAbility = false;
            canGetOrb = false;
            _shootManager.canShoot = false;
            _hpMaster.isInvincible = true;


            _animator.SetInteger(_animDance, 1);

        }

        [ClientRpc]
        public void RpcEndDance()
        {
            GetComponentInParent<PlayerManager>().canMove = true;
            GetComponentInParent<PlayerManager>().canAbility = true;
            canGetOrb = true;
            _shootManager.canShoot = true;
            _hpMaster.isInvincible = false;


            _animator.SetInteger(_animDance, 0);

        }

        [TargetRpc]
        public void TargetDance()
        {
            GetComponentInParent<PlayerManager>().canMove = false;
            GetComponentInParent<PlayerManager>().canAbility = false;
            canGetOrb = false;
            _shootManager.canShoot = false;
            _hpMaster.CmdInvincible(true);


            _animator.SetInteger(_animDance, 1);

        }

        [TargetRpc]
        public void TargetEndDance()
        {
            GetComponentInParent<PlayerManager>().canMove = true;
            GetComponentInParent<PlayerManager>().canAbility = true;
            canGetOrb = true;
            _shootManager.canShoot = true;
            _hpMaster.CmdInvincible(false);


            _animator.SetInteger(_animDance, 0);

        }


        [Server]
        public void ServerUpdateAllPositions(Vector3 newPos)
        {
            NetworkIdentity identity = parentOfPlayer.GetComponent<NetworkIdentity>();
            NetworkConnection conn = identity.connectionToClient;

            controller.enabled = false;
            parentOfPlayer.transform.position = newPos;
            controller.enabled = true;
            GetComponentInParent<CharacterTransfromNetwork>().isSynchronize = true;
            parentOfPlayer.GetComponent<CharacterTransfromNetwork>().TargetRequestPos(conn, newPos);
            StartCoroutine(StartToMove());

        }

        public IEnumerator StartToMove()
        {
            yield return new WaitForSeconds(0.5f);
            GetComponentInParent<PlayerManager>().canMove = true;
            GetComponentInParent<ShootManager>().canShoot = true;
        }


        public void RequestDestroy(uint sceneObjNetId)
        {
            if (isLocalPlayer)
            {
                CmdRequestDestroy(sceneObjNetId);
            }
        }

        // クライアントがサーバーに対して削除を要求するCommand
        [Command]
        public void CmdRequestDestroy(uint sceneObjNetId)
        {
            if (NetworkServer.spawned.TryGetValue(sceneObjNetId, out NetworkIdentity targetIdentity))
            {
                var destroy = targetIdentity.GetComponent<GetEffect>();
                if (destroy != null)
                {
                    destroy.ServerDestroy();
                }
            }
        }
    }
}