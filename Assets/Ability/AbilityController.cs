using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityController : NetworkBehaviour
{

    public LayerMask wallLayer;

    [SerializeField] private List<GameObject> objectList; 
    public GameObject nowControlled;
    public GameObject nowGeometry;
    public GameObject nowCamera;
    private CharacterController _controller;
    public ShootManager shootManager;
    public SkillManager skillManager;
    public PlayerActionLockManager lockManager;

    [SyncVar]
    public bool canUse = true;

    [SyncVar]
    public PlayerForm currentForm = PlayerForm.Human;

    public LayerMask ground;

    public void SetAbilityEnabled(bool enabled)
    {
        canUse = enabled;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartAuthority()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        _controller = GetComponent<CharacterController>();

        SwitchForm(PlayerForm.Human);
        


    }
    

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (canUse)
        {
            Ability();
        }


    }

    public void Ability()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            skillManager.UseSkill1();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            skillManager.UseSkill2();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            skillManager.UseSkill3();
        }
    }






    public Vector3 GetHitInForward(Vector3 pos, Vector3 dir)
    {

        Physics.Raycast(pos, dir, out RaycastHit hit, 100, wallLayer);

        Vector3 offsetDirection = -dir;
        float offsetDistance = 0.3f; // ������O�̋����i�K�v�ɉ����Ē����j

        return hit.point + offsetDirection * offsetDistance;

    }

    public void SwitchForm(PlayerForm newForm)
    {


        CmdSwitchForm((int)newForm);
        
    }

    [Command(requiresAuthority = false)]
    public void CmdSwitchForm(int newFormInt)
    {
        RpcSwitchForm(newFormInt);
        
    }


    [ClientRpc]
    public void RpcSwitchForm(int newFormInt)
    {

        shootManager.ResetZoom();
        PlayerForm newForm = (PlayerForm)newFormInt;

        if(currentForm != newForm)
        {
            StartCoroutine(CorrectCharacterPosition(gameObject));
        }
        currentForm = newForm;

        if (newForm == PlayerForm.Human)
        {

            GetComponent<HpMaster>().isInvincible = false;
            nowControlled = objectList.FirstOrDefault(obj => obj.name == "PlayerObject");

            if (isLocalPlayer)
            {
                _controller.radius = 0.35f;
                _controller.height = 2.1f;
                _controller.center = new Vector3(0, 1.1f, 0);

            }


           
        }
        else if (newForm == PlayerForm.Bird)
        {
            GetComponentInChildren<ThirdPersonController>().CmdResetSpeed();
            GetComponentInChildren<ShootManager>().ResetZoom();
            GetComponentInChildren<WeaponManager>().CmdStopReload();
            GetComponent<HpMaster>().isInvincible = true;
            nowControlled = objectList.FirstOrDefault(obj => obj.name == "YellowObject");
            if (isLocalPlayer)
            {
                _controller.radius = 0.7f;
                _controller.height = 0.2f;
                _controller.center = new Vector3(0, 1.9f, -0.14f);
            }
            YellowManager yellowManager = nowControlled.GetComponentInChildren<YellowManager>();
            yellowManager.StartChange();
        }
        nowGeometry = nowControlled.GetComponent<FormManager>().character;
        nowCamera = nowControlled.GetComponent<FormManager>().camera;

        StartCoroutine(DisableNowControlled());
    }

    private IEnumerator CorrectCharacterPosition(GameObject character)
    {

        lockManager.AddLock(PlayerAction.Move, "CorrectPos");
        // Raycastで地面の高さを取得
        Vector3 pos = character.transform.position;
        Ray ray = new Ray(pos + Vector3.up * 2f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f, ground))
        {
            
            GetComponent<CharacterTransfromNetwork>().isSynchronize = false;
            GetComponent<CharacterController>().enabled = false;

            pos.y += 1.5f; // 少し上にオフセットして埋まり防止
            Debug.Log(GetComponent<CharacterController>().enabled +""+ GetComponent<CharacterTransfromNetwork>().isSynchronize);
            character.transform.position = pos;
            GetComponent<CharacterTransfromNetwork>().isSynchronize = true;
            //GetComponent<CharacterTransfromNetwork>().CmdPos(transform.position);

            
            

        }

        yield return new WaitForSeconds(0.1f);

        GetComponent<CharacterController>().enabled = true;

        lockManager.RemoveLock(PlayerAction.Move, "CorrectPos");




    }


    public IEnumerator DisableNowControlled()
    {
        if (lockManager.GetLocks()[PlayerAction.Move].Contains("CorrectPos"))
        {
            yield return new WaitWhile(() => lockManager.GetLocks()[PlayerAction.Move].Contains("CorrectPos"));
        }

        foreach (var obj in objectList)
        {
            if (obj == nowControlled && obj != null)
            {
                obj.GetComponent<FormManager>().character.SetActive(true);
                obj.GetComponent<FormManager>().camera.SetActive(true);
            }
            else
            {
                obj.GetComponent<FormManager>().character.SetActive(false);
                obj.GetComponent<FormManager>().camera.SetActive(false);
            }
        }
    }

    public enum PlayerForm
    {
        Human,
        Bird
    }

    public void BeHuman()
    {
        if (currentForm == PlayerForm.Bird)
        {
            SwitchForm(PlayerForm.Human);
        }
    }


    public void BeBird()
    {
        if (currentForm == PlayerForm.Human)
        {
            SwitchForm(PlayerForm.Bird);
        }
    }

}
