using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{


    public class ThirdPersonController : NetworkBehaviour
    {

        public CharacterStats characterStats;
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        private float MoveSpeed = 3.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        private float SprintSpeed = 6f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        private float JumpHeight = 1.8f;

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


        

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        public GameObject myBody;

        public LayerMask MyBodyLayer;
        public LayerMask Invisible;
        public LayerMask OrbMask;

        [SyncVar]
        private float _speed;
        private float _animationBlend;
        private Quaternion _targetRotation;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private float groundAcceleration = 14f;
        private float groundDeceleration = 18f;
        private float airControl = 0.3f; // 0〜1。0なら慣性固定
        private float maxAirSpeedMultiplier = 0.9f;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animDance;
        private int _animCrouching;
        private int _animReloading;
        private int _animPraying;

        public Transform middleBrow;
        public Transform head;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif
        private Animator _animator;
        public CharacterController controller;
        private GameObject _mainCamera;
        private GameObject _UiCamera;

        private AudioListener _audioListener;

        private Camera _CameraComponent;
        private Camera _UiCameraComponent;
        public Camera recordCamera;

        private ShootManager _shootManager;
        private HpMaster _hpMaster;

        private bool _hasAnimator;

        public float xRotation = 0f; // 上下回転の累積値

        private float _airTime;

        private float _dashSound = 0.4f;

        private Vector3 _lastMoveDirection = Vector3.zero;
        private Vector3 moveDirection;

        private float _sensitivity = 1f;

        public AudioManager audioManager;

        public bool canGetOrb = true;
        public GameObject Aiscream;

        GameObject BgmObject;
        public GameObject parentOfPlayer;

        public CharacterTransfromNetwork transformNetwork;

        private PlayerInputActions inputActions;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool jump;
        private bool isCrouching = false;
        private bool isWalking = false;

        public Coroutine stunCoroutine;
        public string currentControlScheme = "Keyboard&Mouse"; // 初期値

        [Header("Aim Assist")]
        [Header("エイムアシスト設定")]
        private float assistRange = 70f;         // 敵を補正対象とする最大距離
        private float maxAssistAngle = 6f;       // 補正が入る最大角度（広すぎるとズレを補正してしまう）
        private float assistStrength = 10f;      // 補正の強さ（回転スピード）

        private float botAssistRangee = 200f;       // 敵を補正対象とする最大距離
        private float botMaxAssistAngle = 400f;       // 補正が入る最大角度（広すぎるとズレを補正してしまう）
        private float botAssistStrength = 240f;      // 補正の強さ（回転スピード）

        public List<Transform> enemies = new List<Transform>();   // 敵のキャッシュリスト
        public List<Transform> enemiesForBot = new List<Transform>();   // 敵のキャッシュリスト
        private Transform targetEnemy;

        private float currentPitch = 0f;    // カメラの上下角度
        private PlayerInput playerInput;

        private bool controllerEnabled;

        [SyncVar]
        public bool canMove = true;
        public PlayerActionLockManager lockManager;

        public float footStepTime;
        private float footStepInTimer;

        public void SetMovementEnabled(bool enabled)
        {
            canMove = enabled;
        }

        public void Awake()
        {

            _CameraComponent = GetComponentInChildren<Camera>();
             _mainCamera = _CameraComponent.gameObject;
            _UiCameraComponent = _mainCamera.transform.GetChild(0).GetComponent<Camera>();
            _UiCamera = _UiCameraComponent.gameObject;
            Canvas canvas = GameObject.FindGameObjectWithTag("Canvas")?.GetComponent<Canvas>();
            _shootManager = GetComponent<ShootManager>();
            _hpMaster = GetComponentInParent<HpMaster>();
        }

        public override void OnStartAuthority()
        {
            if (isLocalPlayer && GetComponentInParent<BotManager>() == null)
            {
                StartCoroutine(InitialSetInput());
                myBody.layer = 13;
                _CameraComponent.enabled = true;
                _UiCameraComponent.enabled = true;
            }
        }

        public IEnumerator InitialSetInput()
        {
            if (!isLocalPlayer) yield break;

            yield return new WaitForSeconds(0.1f);

            playerInput = GetComponentInParent<PlayerInput>();
            var jump = playerInput.actions.FindAction("Jump");
            var interact = playerInput.actions.FindAction("Interact");
            var crouch = playerInput.actions.FindAction("Crouch");
            var walk = playerInput.actions.FindAction("Walk");

            // 起動時に初期デバイスを判定
            StartCoroutine(InitializeControlScheme());

            // デバイス入力の変更を検知
            InputSystem.onEvent += OnInputEvent;
            InputSystem.onDeviceChange += OnDeviceChange;


            jump.Enable();
            interact.Enable();
            crouch.Enable();
            walk.Enable();

            // ジャンプ：ボタン
            jump.performed += OnJump;

            // インタラクト：ボタン
            interact.performed += _ => GetOrb();

            // しゃがみ：長押しでON、離したらOFF
            crouch.performed += _ => isCrouching = true;
            crouch.canceled += _ => isCrouching = false;

            // 歩き：長押しでON、離したらOFF
            walk.performed += _ => isWalking = true;
            walk.canceled += _ => isWalking = false;
        }

        public void RefreshEnemyTargets()
        {
            enemies.Clear();

            List<GameObject> targets = new List<GameObject>();
            targets.Add(RoundManager.rm.GetOtherPlayer());
            if (targets == null || targets.Count == 0 || targets.Contains(null))
            {
                // RoundManager から全ボット取得
                targets = RoundManager.rm.GetBots();
            }
                
            

            foreach (var target in targets)
            {
                if (target == null) continue;

                // 子オブジェクトから Body と Head を探す
                Transform[] children = target.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child.CompareTag("Body") || child.CompareTag("Head"))
                    {
                        enemies.Add(child);
                        Debug.Log(child);
                    }
                }
            }
        }
        public void BotRefreshEnemyTargets()
        {
            enemiesForBot.Clear();

            List<GameObject> targets = new List<GameObject>();
            targets.Add(RoundManager.rm.GetMyPlayer());



            foreach (var target in targets)
            {
                if (target == null) continue;

                // 子オブジェクトから Body と Head を探す
                Transform[] children = target.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child.CompareTag("Body") || child.CompareTag("Head"))
                    {
                        enemiesForBot.Add(child);
                        Debug.Log(child);
                    }
                }
            }
        }



        private void Start()
        {
            playerInput = GetComponentInParent<PlayerInput>();
            _hasAnimator = TryGetComponent(out _animator);
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            MoveSpeed = characterStats.defaultMoveSpeed;
            SprintSpeed = characterStats.defaultSprintSpeed;
            JumpHeight = characterStats.defaultJumpHeight;
            Gravity = characterStats.defaultGravity;
            assistRange = characterStats.defaultAssistRange;
            maxAssistAngle = characterStats.defaultMaxAssistAngle;
            assistStrength = characterStats.defaultAssistStrength;
            footStepTime = characterStats.defaultFootStepTime;
            groundAcceleration = characterStats.defaultGroundAcceleration;
            groundDeceleration = characterStats.defaultGroundDeceleration;
            airControl = characterStats.defaultAirControl;
            maxAirSpeedMultiplier = characterStats.defaultMaxAirSpeedMultiplier;

    }
        private IEnumerator InitializeControlScheme()
        {

            yield return null;

            if (Gamepad.all.Count > 0 && Gamepad.all.Count == 0)
            {
                // 最初の接続されているGamepadを使用
                SwitchScheme("Gamepad", Gamepad.all[0]);
            }
            else
            {
                // キーボードとマウス
                SwitchScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
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
                Debug.Log("プレイヤーインプットがまだ準備できてないので、一応気をつけてください。無理やり使うとエラーになります");
            }
        }

        // 入力イベントを監視して、最後に操作したデバイスを判定
        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (playerInput == null) return;
            if (!isLocalPlayer) return;

            // 無効なデバイスやUI入力は無視
            if (!device.added || device is Pointer) return;

            if (device is Keyboard || device is Mouse)
            {
                SwitchScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
            }
            else if (device is Gamepad)
            {
                SwitchScheme("Gamepad", device);
            }
        }

        // デバイスが追加・切断された場合の対応
        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (!isLocalPlayer) return;

            if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
            {
                // 現在のデバイスが切断された場合、他のデバイスに切替
                InitializeControlScheme();
            }
        }

        // 外部で現在のスキームを取得する場合
        public string GetCurrentScheme()
        {
            return currentControlScheme;
        }
    

    private void Update()
    {

            GroundedCheck();

            head.position = middleBrow.position;
            

            if (_audioListener == null)
            {
                _CameraComponent = GetComponentInChildren<Camera>();
                _mainCamera = _CameraComponent.gameObject;
                _audioListener = _mainCamera?.GetComponent<AudioListener>();
            }
            else
            {
                if (RoundManager.rm != null)
                {
                    if ((RoundManager.rm.currentMode == RoundManager.Mode.PRACTICE && isLocalPlayer) || (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE && isLocalPlayer) || (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND && isLocalPlayer))
                    {
                        _audioListener.enabled = true;
                    }
                    else
                    {
                        _audioListener.enabled = false;
                    }
                }

            }

            if (GetComponentInParent<NetworkIdentity>() == null)
            {
                return;
            }
            if (!isLocalPlayer)
            {
                return;
            }
            
            _sensitivity = PlayerPrefs.GetFloat("Sensitivity");
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            if (canMove)
            {

                Move();
            }            
            else
            {
                // **アニメーター更新**
                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, _animationBlend);
                    _animator.SetFloat(_animIDMotionSpeed, _speed);
                    _animator.SetBool(_animCrouching, isCrouching);
                }
            }

            if (canGetOrb)
            {
                GetOrb();
            }

            if (Input.GetKeyDown(KeyCode.P))
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
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            bool pray = stateInfo.IsName("Pray");
            if (pray || _animator.GetBool(_animPraying))
            {
                CmdDefusing();
            }
            else
            {
                CmdEndDefusing();
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

            if (isLocalPlayer)
            {
                CameraRotation();

                if (currentControlScheme == "Gamepad")
                {

                    // 補正対象の敵を検索
                    targetEnemy = FindBestEnemyTarget();



                    ApplyAimAssist();

                }
            }
            if (GetComponentInParent<BotManager>() != null)
            {
                // 補正対象の敵を検索
                targetEnemy = FindBestEnemyTarget();
                BotApplyAimAssist();
            }
        }


        private Transform FindBestEnemyTarget()
        {
            Transform bestTarget = null;
            float bestAngle = maxAssistAngle;

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                Vector3 dirToEnemy = (enemy.position - _mainCamera.transform.position).normalized;
                float distance = Vector3.Distance(_mainCamera.transform.position, enemy.position);
                if (distance > assistRange) continue;

                float angle = Vector3.Angle(_mainCamera.transform.forward, dirToEnemy);

                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    bestTarget = enemy;
                }
            }

            return bestTarget;
        }
        private Transform BotFindBestEnemyTarget()
        {
            Transform bestTarget = null;
            float bestAngle = maxAssistAngle;

            foreach (var enemy in enemiesForBot)
            {
                if (enemy == null) continue;

                Vector3 dirToEnemy = (enemy.position - _mainCamera.transform.position).normalized;
                float distance = Vector3.Distance(_mainCamera.transform.position, enemy.position);
                if (distance > botAssistRangee) continue;

                float angle = Vector3.Angle(_mainCamera.transform.forward, dirToEnemy);

                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    bestTarget = enemy;
                }
            }

            return bestTarget;
        }


        private void ApplyAimAssist()
        {
            RefreshEnemyTargets();

            Vector3 camPos = Camera.main.transform.position;
            Vector3 camFwd = Camera.main.transform.forward;

            Transform bestTargetPart = null;
            Vector3 bestTargetPoint = Vector3.zero;
            float bestScore = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                foreach (var col in enemy.GetComponentsInChildren<Collider>())
                {
                    Vector3 point = col.bounds.center;
                    Vector3 toPart = point - camPos;

                    if (Physics.Linecast(camPos, point, GroundLayers)) continue;

                    float angle = Vector3.Angle(camFwd, toPart);
                    float distance = toPart.magnitude;
                    float score = angle + distance * 0.02f;

                    if (score < bestScore && angle < maxAssistAngle)
                    {
                        bestScore = score;
                        bestTargetPart = col.transform;
                        bestTargetPoint = point;
                    }
                }
            }

            if (bestTargetPart == null) return;

            Vector2 lookInput = playerInput.actions.FindAction("Look").ReadValue<Vector2>();
            bool hasInput = lookInput.sqrMagnitude > 0.0025f;

            Quaternion currentRot = Camera.main.transform.rotation;
            Vector3 toTarget = bestTargetPoint - camPos;
            Quaternion targetRot = Quaternion.LookRotation(toTarget);

            float angleDiff = Quaternion.Angle(currentRot, targetRot);
            float step = assistStrength * Time.deltaTime * angleDiff * (hasInput ? 0.3f : 1f);
            Quaternion finalRot = Quaternion.RotateTowards(currentRot, targetRot, step);

            Vector3 euler = finalRot.eulerAngles;
            currentPitch = euler.x > 180f ? euler.x - 360f : euler.x;
            transformNetwork.yaw = euler.y;

            CameraParticularRotaion(-currentPitch);
            parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);
        }

        private void BotApplyAimAssist()
        {
            BotRefreshEnemyTargets();

            Vector3 camPos = _mainCamera.transform.position;
            Vector3 camFwd = _mainCamera.transform.forward;

            Transform bestTargetPart = null;
            Vector3 bestTargetPoint = Vector3.zero;
            float bestScore = float.MaxValue;

            foreach (var enemy in enemiesForBot)
            {
                if (enemy == null) continue;

                foreach (var col in enemy.GetComponentsInChildren<Collider>())
                {
                    Vector3 point = col.bounds.center;
                    Vector3 toPart = point - camPos;

                    if (Physics.Linecast(camPos, point, GroundLayers)) continue;

                    float angle = Vector3.Angle(camFwd, toPart);
                    float distance = toPart.magnitude;
                    float score = angle + distance * 0.02f;

                    if (score < bestScore && angle < botMaxAssistAngle)
                    {
                        bestScore = score;
                        bestTargetPart = col.transform;
                        bestTargetPoint = point;
                    }
                }
            }

            if (bestTargetPart == null) return;

            Quaternion currentRot = _mainCamera.transform.rotation;
            Vector3 toTarget = bestTargetPoint - camPos;
            Quaternion targetRot = Quaternion.LookRotation(toTarget);

            float angleDiff = Quaternion.Angle(currentRot, targetRot);
            float step = botAssistStrength * Time.deltaTime * (angleDiff * 0.1f);
            Quaternion finalRot = Quaternion.RotateTowards(currentRot, targetRot, step);

            Vector3 euler = finalRot.eulerAngles;
            currentPitch = euler.x > 180f ? euler.x - 360f : euler.x;
            transformNetwork.yaw = euler.y;

            CameraParticularRotaion(-currentPitch);
            parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);
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
            _animPraying = Animator.StringToHash("Praying");
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
            lookInput = playerInput.actions.FindAction("Look").ReadValue<Vector2>();

            Vector2 look = Mouse.current.delta.ReadValue();

            if (look.sqrMagnitude > 0.001f)
            {
                SwitchScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
            }
            float magnification = 1;

            if (currentControlScheme == "Keyboard&Mouse")
            {
                magnification = 0.05f;
            }
            if (currentControlScheme == "Gamepad")
            {
                magnification = 2f;
            }
            // マウス or スティック両対応
            float mouseX = lookInput.x * magnification;
            float mouseY = lookInput.y * magnification;

            // 上下方向の回転を蓄積してClamp
            xRotation -= mouseY * _sensitivity;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);


                // カメラに上下回転を適用
                head.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

                    // --- 身体の左右回転 (Yaw) ---
                transformNetwork.yaw += mouseX * _sensitivity * (_CameraComponent.fieldOfView / 74.03f);
                parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);
                //transformNetwork.CmdRotate(parentOfPlayer.transform.rotation);
                //transformNetwork.CmdRotateCamera(_mainCamera.transform.localRotation);

            

        }
        public void CameraParticularRotaion(float pitch)
        {
            // ピッチをClamp
            xRotation = Mathf.Clamp(-pitch, -90f, 90f);

            // 体のYawと合成
            head.localRotation = Quaternion.Euler(xRotation, 0, 0f);
            //transformNetwork.CmdRotateCamera(_mainCamera.transform.localRotation);
        }


        public void CameraRecoil(float recoil)
        {

            // 上下方向の回転を蓄積してClamp
            xRotation -= recoil;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // カメラに上下回転を適用
            head.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            

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
                xRotation -= stunLevel * random * 0.001f;
                CameraRecoil(xRotation);

                // 身体の左右回転 (Yaw)
                transformNetwork.yaw += random * stunLevel * (_CameraComponent.fieldOfView / 74.03f);
                parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);

                yield return null; // 毎フレーム繰り返す
            }

            stunCoroutine = null;

        }

        private void Move()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0); // 0 = Base Layer

            bool crouch = stateInfo.IsName("Crouch");

            bool sneak = _shootManager.IsZooming;

            // Move アクションから入力を取得
            moveInput = playerInput.actions.FindAction("Move").ReadValue<Vector2>();
            float horiMove = moveInput.x;   // 横方向（A/Dキー、左スティック左右）
            float verMove = moveInput.y;    // 縦方向（W/Sキー、左スティック上下）

            bool isMove = (horiMove != 0 || verMove != 0);



            if (!_animator.IsInTransition(0) && !crouch)
            {


                    float targetSpeed = 0f;

                    // **目標速度を設定（入力がない場合は即座に0）**

                    if (isMove || !Grounded)
                    {

                        if (Grounded)
                        {
                            footStepInTimer += Time.deltaTime;
                        }

                        if (isWalking)
                        {
                            targetSpeed = MoveSpeed;
                        }
                        else
                        {                 
                            targetSpeed = SprintSpeed;
                            if (footStepInTimer >= footStepTime)
                            {
                                OnFootstep();
                                footStepInTimer = 0;
                            }
                        }

                        if (sneak)
                        {
                            targetSpeed *= 0.5f;
                        }
       

                    }
                    else
                    {
                        if (footStepInTimer >= 0)
                        {
                            footStepInTimer -= Time.deltaTime;
                        }
                        targetSpeed = 0f;
                    }


                    // **Lerpを使わず、即座に速度を適用**
                    _speed = targetSpeed;
                    _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
                    if (_animationBlend < 0.01f) _animationBlend = 0f;





            }
            else
            {
                _speed = 0;
            }

            if (isLocalPlayer)
            {
                CmdReportSpeed(_speed);
            }

            // **移動方向計算（正規化 + 速度適用）**
            Vector3 inputDir = (transform.right * horiMove + transform.forward * verMove).normalized;

            // 入力がある場合のみ正規化
            if (inputDir.sqrMagnitude > 0.01f)
                inputDir.Normalize();

            if (Grounded)
            {
                // ---- 地上移動 ----
                if (inputDir.sqrMagnitude > 0.01f)
                {
                    // 徐々に加速（急に速くならない）
                    _lastMoveDirection = Vector3.Lerp(_lastMoveDirection, inputDir, groundAcceleration * Time.deltaTime);
                }
                else
                {
                    // 徐々に減速（惰性を抑える）
                    _lastMoveDirection = Vector3.Lerp(_lastMoveDirection, Vector3.zero, groundDeceleration * Time.deltaTime);
                }

                moveDirection = _lastMoveDirection * _speed;
            }
            else
            {
                // ---- 空中移動 ----
                if (inputDir.sqrMagnitude > 0.01f)
                {
                    // 慣性方向を少しずつ入力方向に寄せる
                    _lastMoveDirection = Vector3.Lerp(_lastMoveDirection, inputDir, airControl * Time.deltaTime * 10f);
                }


                // 水平速度の上限制限（空中で加速しすぎ防止）
                Vector3 horizontal = new Vector3(_lastMoveDirection.x, 0, _lastMoveDirection.z);
                float horizontalMag = horizontal.magnitude;
                float maxAirSpeed = _speed * maxAirSpeedMultiplier;
                if (horizontalMag > maxAirSpeed)
                {
                    horizontal = horizontal.normalized * maxAirSpeed;
                    _lastMoveDirection = new Vector3(horizontal.x, _lastMoveDirection.y, horizontal.z);
                }

                // 慣性を維持した方向に進む
                moveDirection = _lastMoveDirection * _speed;
            }


            //Debug.Log("p" + moveDirection);
            // **CharacterControllerで移動**
            if (controller.enabled == true)
            {
                controller.Move(moveDirection * Time.deltaTime);
            }
         

            // **アニメーター更新**
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, _speed / SprintSpeed);
                _animator.SetBool(_animCrouching, isCrouching);
            }



        }



        private void GetOrb()
        {


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

        private void JumpAndGravity()
        {

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

            // **移動ベクトルを計算（空中では最後の移動方向を維持）**
            Vector3 moveDirection = new Vector3(0, 0, 0);
            moveDirection.y = _verticalVelocity; // 重力やジャンプのY軸速度を維持

            if (controller.enabled == true)
            {
                controller.Move(moveDirection * Time.deltaTime);

            }

        }
        private void OnJump(InputAction.CallbackContext context)
        {
            if (!Grounded) return;
            if (!canMove) return;

            // ジャンプ開始
            jump = true;
        }


        public void Reloading() 
        {
            if (!isLocalPlayer) return;
            if (!_shootManager.canShoot) return;
            _animator.SetBool(_animReloading, true);
        }
        public void EndReloading()
        {
            if (!isLocalPlayer) return;
            _animator.SetBool(_animReloading, false);
        }

        [ClientRpc]
        public void RpcPraying()
        {
            if (!isLocalPlayer) return;
            if (!_hasAnimator) return;
            _animator.SetBool(_animPraying, true);
        }

        [ClientRpc]
        public void RpcEndPraying()
        {
            if (!isLocalPlayer) return;
            if (!_hasAnimator) return;
            _animator.SetBool(_animPraying, false);
        }


        [Command]
        public void CmdDefusing()
        {
            lockManager.AddLock(PlayerAction.Move, "Defuse");
            lockManager.AddLock(PlayerAction.Shoot, "Defuse");
            lockManager.AddLock(PlayerAction.Ability, "Defuse");
        }

        [Command]
        public void CmdEndDefusing()
        {
            lockManager.RemoveLock(PlayerAction.Move, "Defuse");
            lockManager.RemoveLock(PlayerAction.Shoot, "Defuse");
            lockManager.RemoveLock(PlayerAction.Ability, "Defuse");
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
            if (!_hasAnimator) return;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0); // 0 = Base Layer
            
            bool isMove = (horiMove != 0);

            isCrouching = isCrouch;
            bool crouch = stateInfo.IsName("Crouch");


            if (!_animator.IsInTransition(0) && !crouch)
            {


                float targetSpeed = 0f;

                // **目標速度を設定（入力がない場合は即座に0）**

                if (isMove || !Grounded)
                {


                    if (isWalk)
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





            }
            else
            {
                _speed = 0;
            }

            if (isLocalPlayer)
            {
                CmdReportSpeed(_speed);
            }

            // **移動方向計算（正規化 + 速度適用）**
            Vector3 inputDir = (transform.right * horiMove).normalized;

            // 入力がある場合のみ正規化
            if (inputDir.sqrMagnitude > 0.01f)
                inputDir.Normalize();

            if (Grounded)
            {
                // ---- 地上移動 ----
                if (inputDir.sqrMagnitude > 0.01f)
                {
                    // 徐々に加速（急に速くならない）
                    _lastMoveDirection = Vector3.Lerp(_lastMoveDirection, inputDir, groundAcceleration * Time.deltaTime);
                }
                else
                {
                    // 徐々に減速（惰性を抑える）
                    _lastMoveDirection = Vector3.Lerp(_lastMoveDirection, Vector3.zero, groundDeceleration * Time.deltaTime);
                }

                moveDirection = _lastMoveDirection * _speed;
            }
            else
            {
                // ---- 空中移動 ----
                if (inputDir.sqrMagnitude > 0.01f)
                {
                    // 慣性方向を少しずつ入力方向に寄せる
                    _lastMoveDirection = Vector3.Lerp(_lastMoveDirection, inputDir, airControl * Time.deltaTime * 10f);
                }


                // 水平速度の上限制限（空中で加速しすぎ防止）
                Vector3 horizontal = new Vector3(_lastMoveDirection.x, 0, _lastMoveDirection.z);
                float horizontalMag = horizontal.magnitude;
                float maxAirSpeed = _speed * maxAirSpeedMultiplier;
                if (horizontalMag > maxAirSpeed)
                {
                    horizontal = horizontal.normalized * maxAirSpeed;
                    _lastMoveDirection = new Vector3(horizontal.x, _lastMoveDirection.y, horizontal.z);
                }

                // 慣性を維持した方向に進む
                moveDirection = _lastMoveDirection * _speed;
            }


            //Debug.Log("p" + moveDirection);
            // **CharacterControllerで移動**
            if (controller.enabled == true)
            {
                controller.Move(moveDirection * Time.deltaTime);
            }


            // **アニメーター更新**
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, _speed / SprintSpeed);
                _animator.SetBool(_animCrouching, isCrouching);
            }
        }

        public void BotJumpAndGravity(bool jump)
        {

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

            // **移動ベクトルを計算（空中では最後の移動方向を維持）**
            Vector3 moveDirection = new Vector3(0, 0, 0);
            moveDirection.y = _verticalVelocity; // 重力やジャンプのY軸速度を維持

            if (controller.enabled == true)
            {
                controller.Move(moveDirection * Time.deltaTime);

            }
        }


        public void BotStop()
        {
            _speed = 0;
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
                if (!isLocalPlayer) return;
                AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.FOOTSTEP, transform.TransformPoint(controller.center), FootstepAudioVolume, 15);
                    
                
            }
        }

        private void OnLand()
        {
            if (!isLocalPlayer) return;
            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.LAND, transform.TransformPoint(controller.center), FootstepAudioVolume, 15);
            
        }

        [ClientRpc]
        public void RpcResetAllParameters()
        {
            if (!_hasAnimator) return;
            foreach (var param in _animator.parameters)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        _animator.SetBool(param.name, param.defaultBool);
                        break;
                    case AnimatorControllerParameterType.Float:
                        _animator.SetFloat(param.name, param.defaultFloat);
                        break;
                    case AnimatorControllerParameterType.Int:
                        _animator.SetInteger(param.name, param.defaultInt);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        _animator.ResetTrigger(param.name);
                        break;
                }
            }
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

        [Command(requiresAuthority = false)]
        public void CmdResetSpeed()
        {
            RpcResetSpeed();
        }


        [ClientRpc]
        public void RpcResetSpeed()
        {
            _speed = 0f;
            _animationBlend = 0f;
            _verticalVelocity = 0f;
            _lastMoveDirection = Vector3.zero;
        }

        [Server]
        public void ResetPos(Vector3 pos)
        {

            ServerUpdateAllPositions(pos);
            
        }

        [ClientRpc]
        public void RpcDance()
        {
            lockManager.AddLock(PlayerAction.Move, "Dance");
            lockManager.AddLock(PlayerAction.Ability, "Dance");
            canGetOrb = false;
            lockManager.AddLock(PlayerAction.Shoot, "Dance");
            _hpMaster.isInvincible = true;


            _animator.SetInteger(_animDance, 1);

        }

        [ClientRpc]
        public void RpcEndDance()
        {
            lockManager.RemoveLock(PlayerAction.Move, "Dance");
            lockManager.RemoveLock(PlayerAction.Ability, "Dance");
            canGetOrb = true;
            lockManager.RemoveLock(PlayerAction.Shoot, "Dance");
            _hpMaster.isInvincible = false;


            _animator.SetInteger(_animDance, 0);

        }

        [TargetRpc]
        public void TargetDance()
        {
            lockManager.AddLock(PlayerAction.Move, "Dance");
            lockManager.AddLock(PlayerAction.Ability, "Dance");
            canGetOrb = false;
            lockManager.AddLock(PlayerAction.Shoot, "Dance");
            _hpMaster.CmdInvincible(true);


            _animator.SetInteger(_animDance, 1);

        }

        [TargetRpc]
        public void TargetEndDance()
        {
            lockManager.RemoveLock(PlayerAction.Move, "Dance");
            lockManager.RemoveLock(PlayerAction.Ability, "Dance");
            canGetOrb = true;
            lockManager.RemoveLock(PlayerAction.Shoot, "Dance");
            _hpMaster.CmdInvincible(false);


            _animator.SetInteger(_animDance, 0);

        }


        [Server]
        public void ServerUpdateAllPositions(Vector3 newPos)
        {
            StartCoroutine(StopMove(newPos));
        }

        public IEnumerator StopMove(Vector3 newPos)
        {
            NetworkIdentity identity = parentOfPlayer.GetComponent<NetworkIdentity>();
            NetworkConnection conn = identity.connectionToClient;

            RpcControllerEnabled(false);
            yield return new WaitWhile(() => controllerEnabled == true);

            identity.GetComponent<CharacterTransfromNetwork>().ResetPosition(newPos, Quaternion.identity);
           
            StartCoroutine(StartToMove());

        }

        public IEnumerator StartToMove()
        {
            lockManager.RemoveLock(PlayerAction.Move, "StopSynchronized");
            lockManager.RemoveLock(PlayerAction.Shoot, "StopSynchronized");

            ThirdPersonController tpc = RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>();
            Debug.Log(tpc);

            if  (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE) 
            {
                //GetMyPlayerは常にこれを呼んでいるサーバーにとってなので注意。どうせ両方のプレイヤーの状態を確認するためこれでも通る。
                if (RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>() == null)
                {
                    yield return new WaitWhile(() => RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>() == null);
                }
                if (RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>().controllerEnabled == true)
                {
                    yield return new WaitWhile(() => RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>().controllerEnabled == true);
                }
                if (RoundManager.rm.GetOtherPlayer() != null)
                {
                    if (RoundManager.rm.GetOtherPlayer().GetComponentInChildren<ThirdPersonController>() == null)
                    {
                        yield return new WaitWhile(() => RoundManager.rm.GetOtherPlayer().GetComponentInChildren<ThirdPersonController>() == null);
                    }
                    if (RoundManager.rm.GetOtherPlayer().GetComponentInChildren<ThirdPersonController>().controllerEnabled == true)
                    {
                        yield return new WaitWhile(() => RoundManager.rm.GetOtherPlayer().GetComponentInChildren<ThirdPersonController>().controllerEnabled == true);
                    }
                }


                RoundManager.rm.CmdHasReset();
                RpcControllerEnabled(true);

            }
            else
            {
                //GetMyPlayerは常にこれを呼んでいるサーバーにとってなので注意。どうせ両方のプレイヤーの状態を確認するためこれでも通る。
                if (RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>() == null)
                {
                    yield return new WaitWhile(() => RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>() == null);
                }
                if (RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>().controllerEnabled == true)
                {
                    yield return new WaitWhile(() => RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>().controllerEnabled == true);
                }
                RoundManager.rm.CmdHasReset();
                RpcControllerEnabled(true);
            }
        }

        [ClientRpc]
        public void RpcControllerEnabled(bool enabled)
        {
            controller.enabled = enabled;
            controllerEnabled = enabled;

            NetworkIdentity identity = parentOfPlayer.GetComponent<NetworkIdentity>();
            NetworkConnection conn = identity.connectionToClient;
            TargetRequestControllerEnabled(conn);
        }

        [TargetRpc]
        public void TargetRequestControllerEnabled(NetworkConnection conn)
        {
            CmdControllerEnabled(controller.enabled);
        }

        [Command]
        public void CmdControllerEnabled(bool enabled)
        {
            controllerEnabled = enabled;
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