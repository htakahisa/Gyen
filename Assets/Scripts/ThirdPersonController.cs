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
        public float _speed;
        private float _animationBlend;
        private Quaternion _targetRotation;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float counterStrafeStrength = 1f; // 強めの減速（CS風）

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private float groundAcceleration = 14f;
        private float groundDeceleration = 18f;
        private float airControl = 0.3f; // 0〜1。0なら慣性固定
        private float maxAirSpeedMultiplier = 0.9f;
        public float recoverSpeed = 0.5f;  // 1秒あたりの回復量
        public float airSoundTime = 0.45f;

        public float statusSpeed = 1;

        [SyncVar(hook = nameof(OnSpeedChanged))]
        private float syncedSpeed;

        [SyncVar(hook = nameof(OnBlendChanged))]
        private float syncedBlend;

        [SyncVar(hook = nameof(OnCrouchChanged))]
        private bool syncedCrouch;

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
        private float botAssistStrength = 10000f;      // 補正の強さ（回転スピード）

        public List<Transform> enemies = new List<Transform>();   // 敵のキャッシュリスト
        public List<Transform> enemiesForBot = new List<Transform>();   // 敵のキャッシュリスト
        private Transform targetEnemy;

        private PlayerInput playerInput;

        private bool controllerEnabled;

        [SyncVar]
        public bool canMove = true;
        public PlayerActionLockManager lockManager;

        public float footStepTime;
        private float footStepInTimer;

        public bool jumpBot;

        public bool jumpRequested;


        public void SetMovementEnabled(bool enabled)
        {
            canMove = enabled;
        }

        public void Awake()
        {
            RoundManager.rm.recordGuys.Add(this);
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
            if (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE)
            {
                targets.Add(RoundManager.rm.GetMyPlayer());
            }
            else if (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND)
            {
                targets.Add(RoundManager.rm.GetMyPlayer());
            }
            else if (RoundManager.rm.currentMode == RoundManager.Mode.PRACTICE)
            {
                targets.Add(RoundManager.rm.GetMyPlayer());
            }
            else if (RoundManager.rm.currentMode == RoundManager.Mode.DOUBLETAP)
            {
                targets.Add(RoundManager.rm.GetOtherPlayer());
                foreach (var bot in RoundManager.rm.GetOtherBots())
                {
                    targets.Add(bot);
                    Debug.Log(bot);
                }
            }


            foreach (var target in targets)
            {
                if (target == null) continue;
                if (target.GetComponent<HpMaster>().isDead) continue;

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
            counterStrafeStrength = characterStats.defaultCounterStrafeStrength;
            recoverSpeed = characterStats.defaultRecoverSpeed;

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
                    if ((RoundManager.rm.currentMode == RoundManager.Mode.PRACTICE && isLocalPlayer) || (RoundManager.rm.currentMode == RoundManager.Mode.ONEVSONE && isLocalPlayer) || (RoundManager.rm.currentMode == RoundManager.Mode.DOUBLETAP && isLocalPlayer) || (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND && isLocalPlayer))
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
            if (isLocalPlayer || GetComponentInParent<BotManager>() != null)
            {
                _sensitivity = PlayerPrefs.GetFloat("Sensitivity");
                _hasAnimator = TryGetComponent(out _animator);
                // 一定速度で baseValue に戻す
                statusSpeed = Mathf.MoveTowards(
                    statusSpeed,
                    1,
                    recoverSpeed * Time.deltaTime
                );

            }
            if (GetComponentInParent<BotManager>() != null)
            {
                // **アニメーター更新**
                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, _speed);
                    _animator.SetFloat(_animIDMotionSpeed, _animationBlend);
                    _animator.SetBool("_animCrouching", isCrouching);
                }
                BotJumpAndGravity();
                if (RoundManager.rm.currentMode == RoundManager.Mode.DOUBLETAP)
                {
                    if (!_shootManager.hasFound)
                    {
                        if (GetComponentInParent<SpawnOwner>().WhoseThis() != null)
                        {
                            if (GetComponentInParent<SpawnOwner>().WhoseThis().GetComponent<ConductController>().conductorInstance != null)
                            {
                                float distance = Vector3.Distance(transform.position, GetComponentInParent<SpawnOwner>().WhoseThis().GetComponent<ConductController>().conductorInstance.transform.position);
                                if (distance > 1)
                                {
                                    Vector3 conductorPos = GetComponentInParent<SpawnOwner>().WhoseThis().GetComponent<ConductController>().conductorInstance.transform.position;
                                    Vector3 camToTarget = conductorPos - _mainCamera.transform.position;
                                    if (camToTarget.sqrMagnitude > 0.0001f)
                                    {
                                        // 水平方向の距離
                                        float flatDist = new Vector2(camToTarget.x, camToTarget.z).magnitude;

                                        // ピッチ角度 = atan2(高さ, 水平方向距離)
                                        float desiredPitch = Mathf.Atan2(camToTarget.y, flatDist) * Mathf.Rad2Deg;

                                        // ここで直接「絶対角度」として渡す
                                        GetComponent<ThirdPersonController>().CameraParticularRotaion(desiredPitch);
                                    }


                                    Vector3 bodyDir = new Vector3(camToTarget.x, 0, camToTarget.z);

                                    if (bodyDir.sqrMagnitude > 0.0001f)
                                    {
                                        Quaternion bodyRot = Quaternion.LookRotation(bodyDir, Vector3.up);
                                        parentOfPlayer.transform.rotation = Quaternion.Euler(0, bodyRot.eulerAngles.y, 0);
                                    }
                                    BotMove(0, 1, RoundManager.rm.currentBotMove == RoundManager.BotMove.WALK, false);
                                }
                                else
                                {
                                    BotMove(0, 0, false, GetComponentInParent<BotManager>().crouchRequired);
                                }
                            }
                            else
                            {
                                BotMove(0, 0, false, GetComponentInParent<BotManager>().crouchRequired);
                            }
                        }
                        else
                        {
                            BotMove(0, 0, false, GetComponentInParent<BotManager>().crouchRequired);
                        }
                    }
                    else
                    {
                        BotMove(0, 0, false, GetComponentInParent<BotManager>().crouchRequired);
                    }
                }
            }
            //if (Input.GetKeyDown(KeyCode.U))
            //{
            //    if (GetComponentInParent<ConductController>().conductorInstance != null)
            //    {
            //        Vector3 conductorPos = GetComponentInParent<ConductController>().conductorInstance.transform.position;
            //        Vector3 camToTarget = conductorPos - Camera.main.transform.position;
            //        if (camToTarget.sqrMagnitude > 0.0001f)
            //        {
            //            // 水平方向の距離
            //            float flatDist = new Vector2(camToTarget.x, camToTarget.z).magnitude;

            //            // ピッチ角度 = atan2(高さ, 水平方向距離)
            //            float desiredPitch = Mathf.Atan2(camToTarget.y, flatDist) * Mathf.Rad2Deg;

            //            // ここで直接「絶対角度」として渡す
            //            GetComponent<ThirdPersonController>().CameraParticularRotaion(desiredPitch);
            //        }


            //        Vector3 bodyDir = new Vector3(camToTarget.x, 0, camToTarget.z);

            //        if (bodyDir.sqrMagnitude > 0.0001f)
            //        {
            //            Quaternion bodyRot = Quaternion.LookRotation(bodyDir, Vector3.up);
            //            transformNetwork.yaw = bodyRot.eulerAngles.y;
            //            parentOfPlayer.transform.rotation = Quaternion.Euler(0, bodyRot.eulerAngles.y, 0);
            //        }
            //        Debug.Log("conduct");
            //    }
            //}
            if (!isLocalPlayer)
            {
                return;
            }


            JumpAndGravity();

            if (canMove)
            {
                Move();
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

        public void ReportAnimState(float speed, float blend, bool crouch)
        {

            CmdUpdateAnimState(speed, blend, crouch);
                            
        }

        [Command]
        private void CmdUpdateAnimState(float speed, float blend, bool crouch)
        {
            syncedSpeed = speed;
            syncedBlend = blend;
            syncedCrouch = crouch;
        }

        private void OnSpeedChanged(float oldValue, float newValue)
        {
            _animator.SetFloat(_animIDSpeed, newValue);
        }

        private void OnBlendChanged(float oldValue, float newValue)
        {
            _animator.SetFloat(_animIDMotionSpeed, newValue);
        }
        private void OnCrouchChanged(bool oldValue, bool newValue)
        {
            _animator.SetBool(_animCrouching, newValue);
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
                if (GetComponentInParent<SpawnOwner>().IsMine() || GetComponentInParent<SpawnOwner>().ownerNetId == 12345)
                {
                    // 補正対象の敵を検索
                    targetEnemy = BotFindBestEnemyTarget();
                    BotApplyAimAssist();
                }

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
            float amount = botAssistStrength * (angleDiff / botMaxAssistAngle);
            amount = Mathf.Clamp01(amount);
            Quaternion finalRot = Quaternion.RotateTowards(currentRot, targetRot, amount);

            Vector3 euler = finalRot.eulerAngles;
            transformNetwork.pitch = euler.x > 180f ? euler.x - 360f : euler.x;

            CameraParticularRotaion(-transformNetwork.pitch);
        }

        private void BotApplyAimAssist()
        {
            if (!GetComponentInParent<SpawnOwner>().IsMine() &&
                GetComponentInParent<SpawnOwner>().ownerNetId != 12345) return;

            BotRefreshEnemyTargets();

            Vector3 camPos = _mainCamera.transform.position;

            Transform best = null;
            float bestScore = float.MaxValue;
            float bestSpeed = 0f;
            float bestDist = 0f;
            Vector3 bestPoint = Vector3.zero;

            // ==============================
            // 初期化
            // ==============================
            if (!transformNetwork.initializedRotation)
            {
                Vector3 camAngles = _mainCamera.transform.localEulerAngles;
                transformNetwork.pitch = (camAngles.x > 180f) ? camAngles.x - 360f : camAngles.x;

                Vector3 bodyAngles = parentOfPlayer.transform.eulerAngles;
                transformNetwork.yaw = bodyAngles.y;

                transformNetwork.initializedRotation = true;
            }

            // ==============================
            // 目標探索
            // ==============================
            foreach (var enemy in enemiesForBot)
            {
                if (enemy == null) continue;

                float enemySpeed = 0f;
                var cc = enemy.GetComponent<CharacterController>();
                if (cc != null) enemySpeed = cc.velocity.magnitude;

                foreach (var col in enemy.GetComponentsInChildren<Collider>())
                {
                    Vector3 point = col.bounds.center;
                    Vector3 to = point - camPos;

                    if (Physics.Linecast(camPos, point, GroundLayers)) continue;

                    float angle = Vector3.Angle(_mainCamera.transform.forward, to);
                    if (angle > botMaxAssistAngle) continue;

                    float dist = to.magnitude;
                    float distancePenalty = dist * 0.03f;
                    float speedPenalty = enemySpeed * 0.4f;

                    float score = angle + dist * 0.015f + speedPenalty + distancePenalty;

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = col.transform;
                        bestSpeed = enemySpeed;
                        bestDist = dist;
                        bestPoint = point;
                    }
                }
            }

            if (best == null) return;

            // ==============================
            // Shake
            // ==============================
            {
                float shake = (bestSpeed * 0.015f) + (bestDist * 0.0008f);
                bestPoint += new Vector3(
                    Random.Range(-shake, shake),
                    Random.Range(-shake, shake),
                    0f
                );
            }

            Vector3 dir = bestPoint - camPos;
            Quaternion targetRot = Quaternion.LookRotation(dir);

            // ==============================
            // 🔥 高速補正：大ズレほど一気に戻す
            // ==============================
            float angleDiff = Quaternion.Angle(_mainCamera.transform.rotation, targetRot);

            // 60°以上 → 完全高速
            // 8° → 通常補正
            float fastLerp = Mathf.InverseLerp(60f, 8f, angleDiff);

            // ==============================
            // Yaw（横）は距離で弱る
            // ==============================
            float distSlow = Mathf.Clamp01(bestDist / 40f);
            float speedSlow = Mathf.Clamp01(bestSpeed / 8f);

            float yawBaseStrength =
                botAssistStrength * (1f - distSlow) * (1f - speedSlow);

            // 大ズレほど最大 18 まで強化
            float yawStrength =
                Mathf.Lerp(yawBaseStrength, 18f, fastLerp);

            // ==============================
            // Pitch（縦）は距離ペナルティなし
            // ==============================
            float pitchBaseStrength = botAssistStrength * 1.8f;

            // 大ズレほど最大 22 まで強化
            float pitchStrength =
                Mathf.Lerp(pitchBaseStrength, 22f, fastLerp);

            Vector3 targetAngles = targetRot.eulerAngles;
            float targetPitch = targetAngles.x > 180f ? targetAngles.x - 360f : targetAngles.x;
            float targetYaw = targetAngles.y;

            // ===== Pitch 更新（縦は強め固定 + 大ズレ高速）=====
            transformNetwork.pitch = Mathf.Lerp(transformNetwork.pitch, targetPitch, pitchStrength * Time.deltaTime);
            transformNetwork.pitch = Mathf.Clamp(transformNetwork.pitch, -90f, 90f);

            CameraParticularRotaion(transformNetwork.pitch);

            // ===== Yaw 更新（距離弱体化 + 大ズレ高速）=====
            transformNetwork.yaw = Mathf.LerpAngle(transformNetwork.yaw, targetYaw, yawStrength * Time.deltaTime);

            parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);
        }






        // 0.8倍にする処理
        public void ApplyPenalty()
        {
            statusSpeed *= 0.8f;
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


            Vector2 lookInput = playerInput.actions.FindAction("Look").ReadValue<Vector2>();


            // カメラ回転値の初期化（初回ループでのみ）
            if (!transformNetwork.initializedRotation)
            {
                Vector3 headAngles = head.localEulerAngles;
                transformNetwork.pitch = (headAngles.x > 180f) ? headAngles.x - 360f : headAngles.x;

                Vector3 bodyAngles = parentOfPlayer.transform.eulerAngles;
                transformNetwork.yaw = bodyAngles.y;

                transformNetwork.initializedRotation = true;
            }

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            if (mouseDelta.sqrMagnitude > 0.001f)
            {
                SwitchScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
            }

            float magnification = (currentControlScheme == "Keyboard&Mouse") ? 0.05f : 2f;

            float mouseX = lookInput.x * magnification;
            float mouseY = lookInput.y * magnification;

            // ==== pitch 計算（上向き下向き） ====
            transformNetwork.pitch -= mouseY * _sensitivity;
            transformNetwork.pitch = Mathf.Clamp(transformNetwork.pitch, -90f, 90f); // 元の仕様そのまま

            // head に適用
            head.localRotation = Quaternion.Euler(transformNetwork.pitch, 0f, 0f);

            // ==== yaw 計算（左右回転） ====
            transformNetwork.yaw += mouseX * _sensitivity * (_CameraComponent.fieldOfView / 74.03f);

            // プレイヤー root オブジェクトに適用
            parentOfPlayer.transform.rotation = Quaternion.Euler(0f, transformNetwork.yaw, 0f);
        }




        public void CameraParticularRotaion(float absolutePitch)
        {
            absolutePitch = Mathf.Clamp(absolutePitch, -90f, 90f);

            // 現在の回転を Quaternion で取得
            Quaternion rot = head.localRotation;

            // rot を Euler に変換して、X だけ書き換える
            Vector3 e = rot.eulerAngles;
            e.x = absolutePitch;

            // そして Euler から Quaternion を再構成
            head.localRotation = Quaternion.Euler(e);
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

            // カメラの現在の Pitch を [-180,180] で取得
            float xRotation = _mainCamera.transform.localEulerAngles.x;
            if (xRotation > 180f) xRotation -= 360f;

            while (Time.time <= endTime)
            {
                // -1 ～ +1 の揺れ
                float randomSign = (Random.value < 0.5f) ? -1f : 1f;

                // ===== Pitch（上下） =====
                xRotation -= stunLevel * randomSign * 0.001f;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                // カメラ反映
                CameraRecoil(xRotation);

                // ===== Yaw（左右） =====
                float currentYaw = parentOfPlayer.transform.eulerAngles.y;
                float yawDelta = randomSign * stunLevel * (_CameraComponent.fieldOfView / 74.03f);

                float newYaw = currentYaw + yawDelta;

                parentOfPlayer.transform.rotation = Quaternion.Euler(0f, newYaw, 0f);

                yield return null;
            }

            stunCoroutine = null;
        }


        private void Move()
        {
            if (!RoundManager.rm.hasMapLoad) return;
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            bool crouch = stateInfo.IsName("Crouch");
            bool sneak = _shootManager.IsZooming;

            // 入力
            moveInput = playerInput.actions.FindAction("Move").ReadValue<Vector2>();
            float horiMove = moveInput.x;
            float verMove = moveInput.y;
            bool isMove = (horiMove != 0 || verMove != 0);

            // しゃがみ・遷移中は慣性を消す（滑り出し防止）
            if ((_animator.IsInTransition(0) || crouch) && Grounded)
            {
                _lastMoveDirection = Vector3.zero;
                moveDirection = Vector3.zero;
                _speed = 0f;
                _animationBlend = 0f;

                ReportAnimState(_speed, _animationBlend, isCrouching);
                CmdReportSpeed(_speed);

                return;
            }

            // 入力方向
            Vector3 inputDir = (transform.right * horiMove + transform.forward * verMove);
            if (inputDir.sqrMagnitude > 1.0f)
                inputDir.Normalize();

            // 最大速度
            float maxSpeed = isWalking ? MoveSpeed : SprintSpeed;
            if (sneak) maxSpeed *= 0.5f;

            if (Grounded)
            {
                Vector3 wishDir = inputDir;
                float wishSpeed = maxSpeed;

                Vector3 current = new Vector3(_lastMoveDirection.x, 0, _lastMoveDirection.z);
                float currentSpeed = current.magnitude;

                bool countering = false;
                // ★ カウンターストレイフ：逆方向入力なら即停止（VALORANTに近い）
                if (wishDir.sqrMagnitude > 0.01f && Vector3.Dot(current, wishDir) < 0f)
                {
                    current = Vector3.zero;  // ← これが一番重要！
                    countering = true;
                }

                // 加速・減速
                Vector3 newMove = Vector3.zero;

                if (!countering)
                {
                    if (wishDir.sqrMagnitude > 0.01f)
                    {
                        newMove = Vector3.Lerp(
                            current,
                            wishDir * wishSpeed,
                            groundAcceleration * Time.deltaTime
                        );
                    }
                    else
                    {
                        newMove = Vector3.Lerp(
                            current,
                            Vector3.zero,
                            groundDeceleration * Time.deltaTime
                        );
                    }
                }

                _lastMoveDirection = newMove;
                moveDirection = _lastMoveDirection;
            }
            else
            {
                // ---- 空中移動 ----
                if (inputDir.sqrMagnitude > 0.01f)
                {
                    _lastMoveDirection = Vector3.Lerp(
                        _lastMoveDirection,
                        inputDir * maxSpeed,
                        airControl * Time.deltaTime * 10f
                    );
                }

                // 空中の水平速度上限
                Vector3 horizontal = new Vector3(_lastMoveDirection.x, 0, _lastMoveDirection.z);
                float horizontalMag = horizontal.magnitude;
                float maxAirSpeed = maxSpeed * maxAirSpeedMultiplier;

                if (horizontalMag > maxAirSpeed)
                {
                    horizontal = horizontal.normalized * maxAirSpeed;
                    _lastMoveDirection = new Vector3(horizontal.x, _lastMoveDirection.y, horizontal.z);
                }

                moveDirection = _lastMoveDirection;
            }

            // 実速度 = ベクトル長
            _speed = _lastMoveDirection.magnitude;

            if (isLocalPlayer)
                CmdReportSpeed(_speed);

            // 移動
            if (controller.enabled)
                controller.Move(moveDirection * Time.deltaTime);

            // ---- アニメーション ----
            if (_hasAnimator)
            {
                float targetAnim = _speed * 0.1f; // 実速度の1/10

                _animationBlend = Mathf.Lerp(
                    _animationBlend,
                    targetAnim,
                    Time.deltaTime * SpeedChangeRate
                );
                if (_animationBlend < 0.01f) _animationBlend = 0f;
            }

            if (isMove && Grounded && _speed >= 1 && footStepInTimer < footStepTime)
            {
                footStepInTimer += Time.deltaTime;
            }
            else
            {
                if (footStepInTimer >= 0)
                {
                    footStepInTimer -= Time.deltaTime * 2;
                }
            }

            if (!isWalking && isMove && Grounded && _speed >= 1)
            {
                if (footStepInTimer >= footStepTime)
                {
                    OnFootstep();
                    footStepInTimer = 0f;
                }
            }
            ReportAnimState(_speed, _animationBlend, isCrouching);

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
            if (!RoundManager.rm.hasMapLoad) return;

            if (Grounded)
            {
                if (_airTime >= airSoundTime)
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
            if (jumpRequested) return;

            StartCoroutine(JumpCoroutine());

        }


        public IEnumerator JumpCoroutine()
        {
            // ジャンプ開始
            jump = true;
            jumpRequested = true;
            yield return new WaitForSeconds(0.05f);
            jumpRequested = false;
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

        public void BotMove(float horiMove, float verMove, bool isWalk, bool isCrouch)
        {
            if (GetComponentInParent<SpawnOwner>() == null) return;
            if (GetComponentInParent<SpawnOwner>().ownerNetId != 12345 && !GetComponentInParent<SpawnOwner>().IsMine()) return;
            if (!canMove) return;
            isWalking = isWalk;
            isCrouching = isCrouch;
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            bool crouch = stateInfo.IsName("Crouch");
            bool sneak = _shootManager.IsZooming;

            bool isMove = (horiMove != 0 || verMove != 0);

            // しゃがみ・遷移中は慣性を消す（滑り出し防止）
            if (_animator.IsInTransition(0) || crouch || (_shootManager.hasFound && RoundManager.rm.currentMode == RoundManager.Mode.DOUBLETAP) && Grounded)
            {
                _lastMoveDirection = Vector3.zero;
                moveDirection = Vector3.zero;
                _speed = 0f;
                _animationBlend = 0f;

                CmdReportSpeed(_speed);
                ReportAnimState(_speed, _animationBlend, isCrouching);              

                return;
            }

            // 入力方向
            Vector3 inputDir = (transform.right * horiMove + transform.forward * verMove);
            if (inputDir.sqrMagnitude > 1.0f)
                inputDir.Normalize();

            // 最大速度
            float maxSpeed = isWalking ? MoveSpeed : SprintSpeed;
            maxSpeed *= statusSpeed;
            if (sneak) maxSpeed *= 0.5f;

            if (Grounded)
            {
                Vector3 wishDir = inputDir;
                float wishSpeed = maxSpeed;

                Vector3 current = new Vector3(_lastMoveDirection.x, 0, _lastMoveDirection.z);
                float currentSpeed = current.magnitude;

                bool countering = false;
                // ★ カウンターストレイフ：逆方向入力なら即停止（VALORANTに近い）
                if (wishDir.sqrMagnitude > 0.01f && Vector3.Dot(current, wishDir) < 0f)
                {
                    current = Vector3.zero;  // ← これが一番重要！
                    countering = true;
                }

                // 加速・減速
                Vector3 newMove = Vector3.zero;

                if (!countering)
                {
                    if (wishDir.sqrMagnitude > 0.01f)
                    {
                        newMove = Vector3.Lerp(
                            current,
                            wishDir * wishSpeed,
                            groundAcceleration * Time.deltaTime
                        );
                    }
                    else
                    {
                        newMove = Vector3.Lerp(
                            current,
                            Vector3.zero,
                            groundDeceleration * Time.deltaTime
                        );
                    }
                }

                _lastMoveDirection = newMove;
                moveDirection = _lastMoveDirection;
            }
            else
            {
                // ---- 空中移動 ----
                if (inputDir.sqrMagnitude > 0.01f)
                {
                    _lastMoveDirection = Vector3.Lerp(
                        _lastMoveDirection,
                        inputDir * maxSpeed,
                        airControl * Time.deltaTime * 10f
                    );
                }

                // 空中の水平速度上限
                Vector3 horizontal = new Vector3(_lastMoveDirection.x, 0, _lastMoveDirection.z);
                float horizontalMag = horizontal.magnitude;
                float maxAirSpeed = maxSpeed * maxAirSpeedMultiplier;

                if (horizontalMag > maxAirSpeed)
                {
                    horizontal = horizontal.normalized * maxAirSpeed;
                    _lastMoveDirection = new Vector3(horizontal.x, _lastMoveDirection.y, horizontal.z);
                }

                moveDirection = _lastMoveDirection;
            }

            // 実速度 = ベクトル長
            _speed = _lastMoveDirection.magnitude;

            CmdReportSpeed(_speed);

            // 移動
            if (controller.enabled)
                controller.Move(moveDirection * Time.deltaTime);

            // ---- アニメーション ----
            if (_hasAnimator)
            {
                float targetAnim = _speed * 0.1f; // 実速度の1/10

                _animationBlend = Mathf.Lerp(
                    _animationBlend,
                    targetAnim,
                    Time.deltaTime * SpeedChangeRate
                );
                if (_animationBlend < 0.01f) _animationBlend = 0f;
            }

            if (isMove && Grounded)
            {
                footStepInTimer += Time.deltaTime;
            }
            else
            {
                if (footStepInTimer >= 0)
                {
                    footStepInTimer -= Time.deltaTime;
                }
            }

            if (!isWalking)
            {
                if (footStepInTimer >= footStepTime)
                {
                    OnFootstep();
                    footStepInTimer = 0f;
                }
            }
            ReportAnimState(_speed, _animationBlend, isCrouching);
        }

        public void BotJumpAndGravity()
        {
            jump = jumpBot;
            
            if (Grounded)
            {
                if (_airTime >= airSoundTime)
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

                AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.FOOTSTEP, transform.TransformPoint(controller.center), FootstepAudioVolume, 30);
                    
                
            }
        }

        private void OnLand()
        {

            AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.LAND, transform.TransformPoint(controller.center), FootstepAudioVolume, 30);
            
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
        public void ResetPos(Vector3 pos, Vector3 rot)
        {

            ServerUpdateAllPositions(pos, rot);
            
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
        public void ServerUpdateAllPositions(Vector3 newPos, Vector3 newRot)
        {
            StartCoroutine(StopMove(newPos, newRot));
        }

        [Server]
        public IEnumerator StopMove(Vector3 newPos, Vector3 newRot)
        {
            NetworkIdentity ni = parentOfPlayer.GetComponent<NetworkIdentity>();
            var ct = ni.GetComponent<CharacterTransfromNetwork>();

            // ① Move 停止（全クライアント）
            RpcControllerEnabled(false);

            // 全クライアントで Controller OFF が反映されるまで少し待つ
            yield return new WaitForSeconds(0.05f);

            // ② 同期 OFF
            ct.SetSynchronize(false);

            // ③ サーバー主導で強制セット（RPCで全クライアントも補間無しで強制セットされる）
            ct.ForceSetPosition(newPos, newRot.y);

            // 位置が確実に反映されるまで待つ
            yield return new WaitForSeconds(0.05f);

            // ④ 同期 ON
            ct.SetSynchronize(true);

            // ⑤ Move 再開
            StartCoroutine(StartToMove());
        }


        public IEnumerator StartToMove()
        {
            lockManager.RemoveLock(PlayerAction.Move, "StopSynchronized");
            lockManager.RemoveLock(PlayerAction.Shoot, "StopSynchronized");

            // クライアント側で Controller が ON になる前に少し待つ
            yield return new WaitForSeconds(0.05f);

            // 必要ならサーバー側へ「完了した」と通知
            RoundManager.rm.CmdHasReset();

            // Move 再開
            RpcControllerEnabled(true);
        }


        [ClientRpc]
        public void RpcControllerEnabled(bool enabled)
        {
            controller.enabled = enabled;
            controllerEnabled = enabled;

            ServerControllerEnabled(controller.enabled);
            
        }



        [ServerCallback]
        public void ServerControllerEnabled(bool enabled)
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

        public void SetRecordCamera()
        {
            if (RoundManager.rm.currentMode == RoundManager.Mode.DUELLAND)
            {
                RoundManager.rm.GetComponent<MatchRecorder>().AddCamera(recordCamera);
                RoundManager.rm.GetComponent<MatchRecorder>().AddHiddenObject(recordCamera, parentOfPlayer);
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