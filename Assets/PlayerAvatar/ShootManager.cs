using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WeaponManager;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]

    public class ShootManager : NetworkBehaviour
    {


        private GameObject _mainCamera;
        private Camera _CameraComponent;

        private WeaponManager weaponManager;

        private float lastAttackTime; // 最後に攻撃した時刻

        private ThirdPersonController tpc;

        public bool IsZooming;

        private Vector3 currentRecoilPosition;

        private Coroutine zoomCoroutine;

        private bool isBursting;

        // Start is called before the first frame update
        public override void OnStartAuthority()
        {
            if (_mainCamera == null)
            {
                _CameraComponent = GetComponentInChildren<Camera>();
                _mainCamera = _CameraComponent.gameObject;

            }

            weaponManager = GetComponent<WeaponManager>();
            weaponManager.SwitchWeapon(WeaponType.Liet);
            Invoke("StartGetTpc", 2f);

        }

        public void StartGetTpc()
        {
            tpc = RoundManager.rm.GetMyPlayer().GetComponent<ThirdPersonController>();
        }


            // Update is called once per frame
        private void Update()
        {
            if (!isLocalPlayer) return;


            // 武器切り替えの例
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                weaponManager.SwitchWeapon(WeaponType.Hazard);
            }

            Shoot();

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

        private IEnumerator BurstFire()
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
                        yield return new WaitForSeconds(currentWeapon.zoomSpeed / (80 - currentWeapon.zoomRatio));
                    }
                }
            }

            IsZooming = true;
            
        }

        public void ResetZoom()
        {
            WeaponData currentWeapon = weaponManager.GetCurrentWeaponData();
            if (currentWeapon != null)
            {
                if (currentWeapon.zoomable && zoomCoroutine != null)
                {
                    StopCoroutine(zoomCoroutine);
                    _CameraComponent.fieldOfView = 80;
                    IsZooming = false;
                }
            }
        }


        private void ShootWeapon()
        {
            lastAttackTime = Time.time;
            WeaponData currentWeapon = weaponManager.GetCurrentWeaponData();
            if (currentWeapon != null)
            {
                Vector3 direction = _mainCamera.transform.forward;
                if (!currentWeapon.isNeedZoom || IsZooming)
                {
                    GetComponent<ServerCheckShoot>().CmdGetShoot(gameObject, _mainCamera.transform.position, direction, currentWeapon.damage, currentWeapon.headDamage);
                }
                StartCoroutine(RecoilCoroutine(0.1f, new Vector3(-currentWeapon.Yrecoil, currentWeapon.Xrecoil, 0f)));
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
                _mainCamera.transform.rotation *= Quaternion.Euler(targetRecoil.x, 0, 0);
                transform.Rotate(Vector3.up * targetRecoil.y * (UnityEngine.Random.value < 0.5f ? -1f : 1f));

                yield return null;
            }


        }

        // 現在のリコイル値を取得
        public Vector3 GetCurrentRecoil()
        {
            return currentRecoilPosition;
        }


    }
}
