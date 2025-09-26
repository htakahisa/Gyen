using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public bool canShoot = true;
        public bool shootInputAhead;

        public LayerMask wallMask;

        Coroutine recoilBounce;

        public CharacterTransfromNetwork transformNetwork;
        public GameObject parentOfPlayer;
        public GameObject hazardScope;


        // Start is called before the first frame update

        private void Awake()
        {
            weaponManager.SwitchWeapon(WeaponType.Liet);
        }

        public override void OnStartAuthority()
        {
         
           
            if (_mainCamera == null)
            {
                _CameraComponent = GetComponentInChildren<Camera>();
                _mainCamera = _CameraComponent.gameObject;

            }

        }


        public void StartGetTpc()
        {
            tpc = RoundManager.rm.GetMyPlayer().GetComponentInChildren<ThirdPersonController>();
        }


            // Update is called once per frame
        private void Update()
        {
            if (RoundManager.rm != null)
            {
                if (RoundManager.rm.hasLoaded && GetComponentInParent<PlayerManager>().hasLoaded && !hasLoaded)
                {
                    StartGetTpc();
                    hasLoaded = true;
                }
            }

            if (!isLocalPlayer) return;



            // 武器リロード
            if (Input.GetKeyDown(KeyCode.R))
            {
                weaponManager.Reload();
            }

            if (BuyPanel.buyPanel.isCursorLocked)
            {
                Shoot();
            }

            if (Input.GetMouseButtonDown(1))
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

            if (IsZooming && weaponManager.GetCurrentWeaponData().weaponName == "Hazard")
            {
                hazardScope.SetActive(true);
            }
            else
            {
                hazardScope.SetActive(false);
            }


        }

        private void Shoot()
        {
            WeaponData currentWeapon = weaponManager.GetCurrentWeaponData();

            if (currentWeapon != null && !isBursting)
            {
                if (IsZooming && currentWeapon.burst != 1)
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

        public IEnumerator BurstFire()
        {
            isBursting = true;
            WeaponData currentWeapon = weaponManager.GetCurrentWeaponData();

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
            WeaponData currentWeapon = weaponManager.GetCurrentWeaponData();
            if (currentWeapon != null)
            {
                if (currentWeapon.zoomable)
                {
                    while (currentWeapon.zoomRatio < _CameraComponent.fieldOfView)
                    {
                        _CameraComponent.fieldOfView -= 1;
                        yield return new WaitForSeconds(currentWeapon.zoomSpeed / (74.03f - currentWeapon.zoomRatio));
                    }
                    IsZooming = true;
                }
            }

           
            
        }

        public void ResetZoom()
        {
            WeaponData currentWeapon = weaponManager.GetCurrentWeaponData();
            if (currentWeapon != null)
            {
                if (currentWeapon.zoomable && zoomCoroutine != null)
                {
                    StopCoroutine(zoomCoroutine);
                    _CameraComponent.fieldOfView = 74.03f;
                    IsZooming = false;
                }
            }
        }


        private void ShootWeapon()
        {
            if (weaponManager.magazine >= 1 || RoundManager.rm.Mode == "Practice")
            {
                if (recoilBounce != null)
                {
                    StopCoroutine(recoilBounce);
                }
                
                if (Input.GetMouseButton(3))
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

                    // --- サーバー同期 ---
                    transformNetwork.CmdRotate(parentOfPlayer.transform.rotation);


                }

                AudioManager.Instance.CmdPlaySoundAtPoint(AudioManager.Sounds.SHOOT, transform.TransformPoint(GetComponentInParent<CharacterController>().center), 0.06f);
                if (weaponManager.magazine >= 1)
                {
                    weaponManager.magazine--;
                }
                lastAttackTime = Time.time;
                WeaponData currentWeapon = weaponManager.GetCurrentWeaponData();
                if (currentWeapon != null)
                {
                    Vector3 direction = _mainCamera.transform.forward;
                    if (!currentWeapon.isNeedZoom || IsZooming)
                    {
                        if (weaponManager.GetCurrentWeaponData().weaponName == "Hazard")
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
                if (!Input.GetMouseButton(0))
                {
                    shootInputAhead = false;
                }

                return false;
            }

            WeaponData currentWeapon = weaponManager.GetCurrentWeaponData();

            if (currentWeapon == null) return false;

            bool shoot;

            if (Auto)
            {
                shoot = Input.GetMouseButton(0);
            }
            else
            {
                shoot = Input.GetMouseButtonDown(0);
            }

            if (!shoot) return false;

            if (weaponManager.isReloading) return false;


            // 現在時刻と最後の攻撃時刻を比較
            float timeSinceLastAttack = Time.time - lastAttackTime;

            return timeSinceLastAttack >= currentWeapon.rate;
            
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
                transformNetwork.CmdRotate(parentOfPlayer.transform.rotation);
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
