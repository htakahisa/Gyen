using Mirror;
using StarterAssets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityController : NetworkBehaviour
{

    public LayerMask wallLayer;

    [SerializeField] private List<GameObject> objectList; // 管理するリスト
    public GameObject nowControlled;
    public GameObject nowGeometry;
    public GameObject nowCamera;
    private CharacterController _controller;
    public ShootManager shootManager;
    public SkillManager skillManager;

    


    [SyncVar]
    public PlayerForm currentForm = PlayerForm.Human;

    public LayerMask ground;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartAuthority()
    {
        _controller = GetComponent<CharacterController>();
        SwitchForm(PlayerForm.Human);
     
    }
    

    // Update is called once per frame
    void Update()
    {

        if (PlayerManager.canAbility)
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
    }






    public Vector3 GetHitInForward(Vector3 pos, Vector3 dir)
    {

        Physics.Raycast(pos, dir, out RaycastHit hit, 100, wallLayer);

        Vector3 offsetDirection = -dir;
        float offsetDistance = 0.3f; // 少し手前の距離（必要に応じて調整）

        return hit.point + offsetDirection * offsetDistance;

    }

    public void SwitchForm(PlayerForm newForm)
    {


        CmdSwitchForm((int)newForm);
        
    }

    [Command]
    public void CmdSwitchForm(int newFormInt)
    {
        RpcSwitchForm(newFormInt);
        
    }


    [ClientRpc]
    public void RpcSwitchForm(int newFormInt)
    {

        shootManager.ResetZoom();
        PlayerForm newForm = (PlayerForm)newFormInt;

        currentForm = newForm;

        if (newForm == PlayerForm.Human)
        {

            nowControlled = objectList.FirstOrDefault(obj => obj.name == "PlayerObject");

            if (isLocalPlayer)
            {
                _controller.radius = 0.2f;
                _controller.height = 2.1f;
                _controller.center = new Vector3(0, 1.1f, 0);

           
                while (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), -transform.up, out RaycastHit hit, 2f, ground))
                {
                    _controller.Move(Vector3.up);
                    Debug.Log(transform.position.y);
                }
            }




        }
        else if (newForm == PlayerForm.Bird)
        {
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
        nowGeometry = nowControlled.GetComponent<FormManager>().geometry;
        nowCamera = nowControlled.GetComponent<FormManager>().camera;

        DisableNowControlled();
    }

    public void DisableNowControlled()
    {

        Debug.Log(transform.position.y);

        foreach (var obj in objectList)
        {
            if (obj == nowControlled && obj != null)
            {
                obj.GetComponent<FormManager>().geometry.SetActive(true);
                obj.GetComponent<FormManager>().camera.SetActive(true);
            }
            else
            {
                obj.GetComponent<FormManager>().geometry.SetActive(false);
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
