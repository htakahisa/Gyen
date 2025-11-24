using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityGLTF;
using static WeaponManager;

namespace StarterAssets
{
    [RequireComponent(typeof(LineRenderer))]

    public class ShootManager : NetworkBehaviour
    {

        public GameObject weaponPos;

        private GameObject _mainCamera;
        private Camera _CameraComponent;

        public WeaponManager weaponManager;

        private float lastAttackTime; // 最後に攻撃した時刻

        private ThirdPersonController tpc;

        public bool IsZooming;

        private Vector3 currentRecoilPosition;

        private Coroutine zoomCoroutine;

        public bool isBursting;

        public AudioManager audioManager;

        private bool hasLoaded = false;

        [SyncVar]
        public bool canShoot = true;
        public bool shootInputAhead;

        public LayerMask wallMask;

        Coroutine recoilBounce;

        public CharacterTransfromNetwork transformNetwork;
        public GameObject parentOfPlayer;
        public GameObject hazardScope;
        private PlayerInputActions inputActions;
        private bool autoFireInput;
        private bool currentFireState;
        private bool lastFireState = false;

        public bool hasFound;
        Coroutine foundDelayCoroutine;

        private Animator _animator;


        private void Start()
        {
            _animator = GetComponentInParent<Animator>();
        }



        // Start is called before the first frame update

        public void SetShootingEnabled(bool enabled)
        {
            canShoot = enabled;
        }

        private void Awake()
        {

            if (_mainCamera == null)
            {
                _CameraComponent = GetComponentInChildren<Camera>();
                _mainCamera = _CameraComponent.gameObject;

            }
        }


        public override void OnStartAuthority()
        {
         

            if (isLocalPlayer && GetComponentInParent<BotManager>() == null)
            {
                inputActions = new PlayerInputActions();
                enabled = true;
                inputActions.Player.Enable();

                inputActions.Player.Fire.performed += _ => autoFireInput = true;
                inputActions.Player.Fire.canceled += _ => autoFireInput = false;

                inputActions.Player.Reload.performed += _ => weaponManager.CmdReload();

                inputActions.Player.Zoom.performed += _ => TryZoom();
            }

        }


        public void StartGetTpc()
        {
            tpc = RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>();
        }


            // Update is called once per frame
        private void Update()
        {

            if (!isLocalPlayer && RoundManager.rm.currentMode != RoundManager.Mode.PRACTICE) return;

            if (RoundManager.rm.hasLoaded && GetComponentInParent<PlayerManager>().hasLoaded && !hasLoaded)
            {
                    StartGetTpc();
                    hasLoaded = true;
            }




            if (BuyPanel.buyPanel.isCursorLocked && isLocalPlayer)
            {
                Shoot();
            }

        

            if (weaponManager.GetCurrentWeaponStats() != null)
            {
                if (weaponManager.GetCurrentWeaponStats().weaponName == "Hazard")
                {
                    if (IsZooming)
                    {
                        hazardScope.SetActive(true);
                        weaponManager.HideWeapon();
                    }
                    else
                    {
                        hazardScope.SetActive(false);
                        weaponManager.ReHideWeapon();
                    }
                }
            }


        }

        public void TryZoom()
        {

            if (BuyPanel.buyPanel.isCursorLocked)
            {
                    if (!IsZooming)
                    {
                        zoomCoroutine = StartCoroutine(Zoom());
                    }
                    else
                    {
                        ResetZoom();

                    }
            }
            
        }

        private void Shoot()
        {
            WeaponDatabase currentWeapon = weaponManager.GetCurrentWeaponStats();

            if (currentWeapon != null && !isBursting)
            {
                if (IsZooming && currentWeapon.burst != 0)
                {
                    if (CanShoot(false))
                    {
                        StartCoroutine(BurstFire());
                    }
                }
                else if (CanShoot(currentWeapon.isAuto))
                {
                    ShootWeapon();
                }
            }



        }

        public void BotShoot()
        {
            WeaponDatabase currentWeapon = weaponManager.GetCurrentWeaponStats();

            if (currentWeapon != null && !isBursting)
            {
                if (IsZooming && currentWeapon.burst != 0)
                {
                    if (BotCanShoot())
                    {
                        StartCoroutine(BurstFire());
                    }
                }
                else if (BotCanShoot())
                {
                    ShootWeapon();
                }
            }



        }

        public IEnumerator BurstFire()
        {
            isBursting = true;
            WeaponDatabase currentWeapon = weaponManager.GetCurrentWeaponStats();

            for (int i = 0; i < currentWeapon.burst; i++)
            {
                    ShootWeapon();

                    // 最後の射撃後は待機しない
                    if (i < currentWeapon.burst - 1)
                    {
                        yield return new WaitForSeconds(currentWeapon.burstRate);
                    }
                
            }

            yield return new WaitForSeconds(currentWeapon.rate * 2);
            isBursting = false;
        }

        private IEnumerator Zoom()
        {
            WeaponDatabase currentWeapon = weaponManager.GetCurrentWeaponStats();
            if (currentWeapon == null || !currentWeapon.zoomable)
                yield break;

            float duration = currentWeapon.zoomSpeed;        // ← ズームにかけたい秒数（例: 0.25f）
            float startFOV = _CameraComponent.fieldOfView;   // 現在のFOV
            float endFOV = currentWeapon.zoomRatio;          // ズーム後のFOV
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // フレームレートに依存しない滑らかなズーム補間
                _CameraComponent.fieldOfView = Mathf.Lerp(startFOV, endFOV, t);

                yield return null;  // 次のフレームまで待つ（フレームレートに依存しない）
            }

            _CameraComponent.fieldOfView = endFOV;
            IsZooming = true;
        }

        public void ResetZoom()
        {
            WeaponDatabase currentWeapon = weaponManager.GetCurrentWeaponStats();
            if (currentWeapon != null)
            {
                if (currentWeapon.zoomable && zoomCoroutine != null)
                {
                    hazardScope.SetActive(false);
                    weaponManager.ReHideWeapon();
                    StopCoroutine(zoomCoroutine);
                    _CameraComponent.fieldOfView = 74.03f;
                    IsZooming = false;
                }
            }
        }


        private void ShootWeapon()
        {
            if (weaponManager.magazine >= 1 || RoundManager.rm.currentMode == RoundManager.Mode.PRACTICE)
            {
                if (recoilBounce != null)
                {
                    StopCoroutine(recoilBounce);
                }
                
                if (Input.GetMouseButton(3) && GetComponentInParent<BotManager>() == null)
                {   

                    Transform myhead;
                    Transform enemyhead;

                    if (RoundManager.rm.GetOtherPlayer() != null)
                    {
                        myhead = RoundManager.rm.GetMyPlayer().GetComponentInChildren<Camera>().transform;
                        enemyhead = RoundManager.rm.GetOtherPlayer().GetComponentInChildren<Camera>().transform;
                        
                    }  
                    else
                    {
                        myhead = RoundManager.rm.GetMyPlayer().GetComponentInChildren<Camera>().transform;
                        enemyhead = RoundManager.rm.GetBots()[UnityEngine.Random.Range(0, RoundManager.rm.GetBots().Count)].GetComponentInChildren<Camera>().transform;

                    }


                    // カメラ位置からターゲット方向ベクトル
                    Vector3 camPos = Camera.main.transform.position;
                    Vector3 camToTarget = enemyhead.position - camPos;

                    if (camToTarget.sqrMagnitude > 0.0001f)
                    {
                        // 水平方向の距離
                        float flatDist = new Vector2(camToTarget.x, camToTarget.z).magnitude;

                        // ピッチ角度 = atan2(高さ, 水平方向距離)
                        float desiredPitch = Mathf.Atan2(camToTarget.y, flatDist) * Mathf.Rad2Deg;

                        // ここで直接「絶対角度」として渡す
                        GetComponent<ThirdPersonController>().CameraParticularRotaion(desiredPitch);
                    }


                    Vector3 targetDir = enemyhead.position - myhead.position;
                    Vector3 bodyDir = new Vector3(targetDir.x, 0, targetDir.z);

                    if (bodyDir.sqrMagnitude > 0.0001f)
                    {
                        Quaternion bodyRot = Quaternion.LookRotation(bodyDir, Vector3.up);
                        transformNetwork.yaw = bodyRot.eulerAngles.y;
                        parentOfPlayer.transform.rotation = Quaternion.Euler(0, bodyRot.eulerAngles.y, 0);
                    }


                }

                AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.SHOOT, transform.TransformPoint(GetComponentInParent<CharacterController>().center), 0.06f, 15);
                if (weaponManager.magazine >= 1)
                {
                    weaponManager.magazine--;
                }
                lastAttackTime = Time.time;
                WeaponDatabase currentWeapon = weaponManager.GetCurrentWeaponStats();
                if (currentWeapon != null)
                {
                 

                    Vector3 direction = _mainCamera.transform.forward;
                    if (!currentWeapon.isNeedZoom || IsZooming)
                    {
                        if (weaponManager.GetCurrentWeaponStats().weaponName == "Hazard")
                        {
                            ResetZoom();
                        }

                        GetComponent<ServerCheckShoot>().CmdGetShoot(transform.parent.parent.gameObject, _mainCamera.transform.position, direction, currentWeapon.damage, currentWeapon.headDamage, weaponPos.transform.position);
                    }

                    StartCoroutine(RecoilCoroutine(0.1f, new Vector3(-currentWeapon.Xrecoil, currentWeapon.Yrecoil, 0f)));

                    recoilBounce = StartCoroutine(Recoilbounce(0.1f, new Vector3(0, currentWeapon.Yrecoil, 0f)));
                    
                }
            }
        }

        public GameObject GetCamera()
        {
            return _mainCamera;
        }

        void OnDrawGizmos()
        {
            if (_mainCamera == null)
            {
                return;
            }
            Vector3 direction = _mainCamera.transform.forward;
            // カメラの位置から指定方向にレイを描画
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_mainCamera.transform.position, direction.normalized * 10);
        }

        public bool CanShoot(bool Auto)
        {

            if (!canShoot) return false;

            if (shootInputAhead)
            {
                if (!autoFireInput)
                {
                    shootInputAhead = false;
                }

                return false;
            }

            WeaponDatabase currentWeapon = weaponManager.GetCurrentWeaponStats();

            if (currentWeapon == null) return false;

            bool shoot;

            if (Auto)
            {
                shoot = autoFireInput;
            }
            else
            {
                // ボタンが押されているか
                bool currentFireState = inputActions.Player.Fire.ReadValue<float>() > 0.5f;

                // 押された瞬間だけ true
                bool pressedThisFrame = currentFireState && !lastFireState;

                // 状態を更新
                lastFireState = currentFireState;
                shoot = pressedThisFrame;
            }

            if (!shoot) return false;

            if (weaponManager.isReloading) return false;


            // 現在時刻と最後の攻撃時刻を比較
            float timeSinceLastAttack = Time.time - lastAttackTime;

            return timeSinceLastAttack >= currentWeapon.rate;
            
        }
        public bool BotCanShoot()
        {

            if (!hasFound) return false;

            if (!canShoot) return false;

            WeaponDatabase currentWeapon = weaponManager.GetCurrentWeaponStats();

            if (currentWeapon == null) return false;


            if (weaponManager.isReloading) return false;


            // 現在時刻と最後の攻撃時刻を比較
            float timeSinceLastAttack = Time.time - lastAttackTime;

            return timeSinceLastAttack >= currentWeapon.rate;

        }


        public void StartFoundDelay(float foundDelayTime)
        {
            // すでにコルーチンが動いていれば何もしない
            if (foundDelayCoroutine != null) return;

            foundDelayCoroutine = StartCoroutine(FoundDelayCoroutine(foundDelayTime));
        }

        public void StopFoundDelay()
        {
            // コルーチンが動いていれば停止
            if (foundDelayCoroutine != null)
            {
                StopCoroutine(foundDelayCoroutine);
                foundDelayCoroutine = null;
            }

            hasFound = false; // 敵が見えなくなったのでフラグリセット
        }

        private IEnumerator FoundDelayCoroutine(float foundDelayTime)
        {
            var tpc = GetComponentInChildren<ThirdPersonController>();
            tpc.BotRefreshEnemyTargets();

            while (true)
            {
                // 敵が見えない場合はコルーチンを終了してフラグを戻す
                if (!CanSeeAnyEnemy(tpc))
                {
                    hasFound = false;
                    foundDelayCoroutine = null;
                    yield break;
                }

                // 敵が見える場合はフラグを true にして処理を進める
                hasFound = true;

                // 遅延処理
                yield return new WaitForSeconds(foundDelayTime);

                // フラグは保持したままループして次フレームもチェック
                yield return null;
            }
        }

        private bool CanSeeAnyEnemy(ThirdPersonController tpc)
        {
            Vector3 camPos = _mainCamera.transform.position;

            foreach (var target in tpc.enemiesForBot)
            {
                var cols = target.GetComponentsInChildren<Collider>();

                foreach (var col in cols)
                {
                    Vector3 pos = col.bounds.center;

                    if (!Physics.Linecast(camPos, pos, wallMask))
                    {
                        return true; // 1つでも見えれば true
                    }
                }
            }

            return false; // 全部壁越しなら false
        }






        // リコイル処理のコルーチン
        public IEnumerator RecoilCoroutine(float duration, Vector3 targetRecoil)
        {
            if (IsZooming)
            {
                targetRecoil *= 0.5f;
            }

            float xRandomRot = UnityEngine.Random.Range(-targetRecoil.x, targetRecoil.x);

            for (int count = 0; count < 10; count++)
            {
                transformNetwork.yaw += xRandomRot / (10 - count);
                GetComponent<ThirdPersonController>().CameraRecoil(targetRecoil.y / (10 - count));
                parentOfPlayer.transform.rotation = Quaternion.Euler(0, transformNetwork.yaw, 0);
                //transformNetwork.CmdRotate(parentOfPlayer.transform.rotation);
                yield return new WaitForSeconds(duration / 9);
            }
        }

        private IEnumerator Recoilbounce(float duration, Vector3 targetRecoil)
        {

            yield return new WaitForSeconds((0.3f - duration) * 1.5f);

            StartCoroutine(RecoilCoroutine(duration, new Vector3(0, -targetRecoil.y, 0)));
            
        }

        // 現在のリコイル値を取得
        public Vector3 GetCurrentRecoil()
        {
            return currentRecoilPosition;
        }

      


    }
}
