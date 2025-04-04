using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static WeaponManager;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ShootManager : NetworkBehaviour
    {

        private StarterAssetsInputs _input;

        private GameObject _mainCamera;
        private Camera _CameraComponent;

        private WeaponManager weaponManager;

        private float lastAttackTime; // ç≈å„Ç…çUåÇÇµÇΩéûçè


        // Start is called before the first frame update
        public override void OnStartAuthority()
        {
            if (_mainCamera == null)
            {
                _input = GetComponent<StarterAssetsInputs>();
                _CameraComponent = GetComponentInChildren<Camera>();
                _mainCamera = _CameraComponent.gameObject;
                weaponManager = GetComponent<WeaponManager>();
                weaponManager.SwitchWeapon(WeaponType.Lover);
            }
        }

            // Update is called once per frame
        private void Update()
        {
            if (!isLocalPlayer) return;


            // ïêäÌêÿÇËë÷Ç¶ÇÃó·
            if (_input.changeMain)
            {
                weaponManager.SwitchWeapon(WeaponType.Lover);
                _input.changeMain = false;
            }
            Shoot();

        
        }

        private void Shoot()
        {
            WeaponData currentWeapon = weaponManager.GetCurrentWeaponData();
            if (currentWeapon != null)
            {
                if (CanShoot())
                {
                    ShootWeapon();
                   
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
                GetComponent<ServerCheckShoot>().CmdGetShoot(gameObject, _mainCamera.transform.position, direction, currentWeapon.damage, currentWeapon.headDamage);

            }
        }

        public GameObject GetCamera()
        {
            return _mainCamera;
        }

        void OnDrawGizmos()
        {
            Vector3 direction = _mainCamera.transform.forward;
            // ÉJÉÅÉâÇÃà íuÇ©ÇÁéwíËï˚å¸Ç…ÉåÉCÇï`âÊ
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_mainCamera.transform.position, direction.normalized * 10);
        }

        public bool CanShoot()
        {
            WeaponData currentWeapon = weaponManager.GetCurrentWeaponData();
            if (currentWeapon == null || !_input.shoot) return false;
            
                // åªç›éûçèÇ∆ç≈å„ÇÃçUåÇéûçèÇî‰är
                float timeSinceLastAttack = Time.time - lastAttackTime;

            if (!currentWeapon.isAuto)
            {
                _input.shoot = false;
            }

            return timeSinceLastAttack >= currentWeapon.rate;
            
        }
    }
}
