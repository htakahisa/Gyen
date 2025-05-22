using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WeaponManager;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(LineRenderer))]

    public class ShootManager : NetworkBehaviour
    {

        public GameObject weaponPos;

        private GameObject _mainCamera;
        private Camera _CameraComponent;

        private WeaponManager weaponManager;

        private float lastAttackTime; // 最後に攻撃した時刻

        private ThirdPersonController tpc;

        public bool IsZooming;

        private Vector3 currentRecoilPosition;

        private Coroutine zoomCoroutine;

        public bool isBursting;

        public AudioManager audioManager;

        private bool hasLoaded = false;

        public bool canShoot = true;

        public LayerMask wallMask;

        // Start is called before the first frame update
        public override void OnStartAuthority()
        {
         
            weaponManager = GetComponent<WeaponManager>();
            weaponManager.SwitchWeapon(WeaponType.Liet);
            if (_mainCamera == null)
            {
                _CameraComponent = GetComponentInChildren<Camera>();
                _mainCamera = _CameraComponent.gameObject;

            }

        }


        public void StartGetTpc()
        {
            tpc = RoundManager.rm.GetMyPlayer().GetComponent<ThirdPersonController>();
        }


            // Update is called once per frame
        private void Update()
        {
            if (RoundManager.rm != null)
            {
                if (RoundManager.rm.hasLoaded && PlayerManager.hasLoaded && !hasLoaded)
                {
                    StartGetTpc();
                    hasLoaded = true;
                }
            }

            if (!isLocalPlayer) return;


            // 武器切り替えの例
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                weaponManager.SwitchWeapon(WeaponType.Hazard);
            }

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
                zoomCoroutine = StartCoroutine(Zoom());
            }
            if (Input.GetMouseButtonUp(1))
            {
                ResetZoom();
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
                        yield return new WaitForSeconds(currentWeapon.rate);
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
                if (Input.GetMouseButton(3))
                {   

                    Vector3 cheatdirection = new Vector3(0,0,0);
                    Transform myhead;
                    Transform enemyhead;

                    if (RoundManager.rm.GetOtherPlayer() != null)
                    {
                        myhead = RoundManager.rm.GetMyPlayer().GetComponentInChildren<Camera>().transform;
                        enemyhead = RoundManager.rm.GetOtherPlayer().GetComponentInChildren<Camera>().transform;
                        cheatdirection = enemyhead.position - myhead.position; 
                        
                    }  
                    else
                    {
                        myhead = RoundManager.rm.GetMyPlayer().GetComponentInChildren<Camera>().transform;
                        enemyhead = RoundManager.rm.GetBots()[UnityEngine.Random.Range(0, RoundManager.rm.GetBots().Count)].GetComponentInChildren<Camera>().transform;

                        cheatdirection = enemyhead.position - myhead.position;
                        
                    }

                    if (!Physics.Raycast(myhead.position, enemyhead.position - myhead.position, Vector3.Distance(myhead.position, enemyhead.position), wallMask))
                    {
                        Vector3 bodydirection = new Vector3(cheatdirection.x, 0, cheatdirection.z);

                        transform.rotation = Quaternion.LookRotation(bodydirection);

                        _mainCamera.transform.rotation = Quaternion.LookRotation(cheatdirection);
                    }
                    
                }

                    audioManager.CmdPlaySoundAtPoint("shoot", transform.TransformPoint(gameObject.GetComponent<CharacterController>().center), 0.06f);
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
                        GetComponent<ServerCheckShoot>().CmdGetShoot(gameObject, _mainCamera.transform.position, direction, currentWeapon.damage, currentWeapon.headDamage, weaponPos.transform.position);
                    }
                    StartCoroutine(RecoilCoroutine(0.1f, new Vector3(-currentWeapon.Yrecoil, currentWeapon.Xrecoil, 0f)));
                }
            }
        }

        public GameObject GetCamera()
        {
            return _mainCamera;
        }

        void OnDrawGizmos()
        {
            Vector3 direction = _mainCamera.transform.forward;
            // カメラの位置から指定方向にレイを描画
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_mainCamera.transform.position, direction.normalized * 10);
        }

        public bool CanShoot(bool Auto)
        {

            if (!canShoot) return false;

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
            float elapsedTime = 0f;
            if (IsZooming)
            {
                targetRecoil *= 0.5f;
            }

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                // スムーズなリコイル適用（イージングをかける）
                _mainCamera.transform.rotation *= Quaternion.Euler(targetRecoil.x / t, 0, 0);
                transform.Rotate(Vector3.up * targetRecoil.y * (UnityEngine.Random.value < 0.5f ? -1f : 1f));

                yield return new WaitForSeconds(0.01f);
            }


        }

        // 現在のリコイル値を取得
        public Vector3 GetCurrentRecoil()
        {
            return currentRecoilPosition;
        }

      


    }
}
