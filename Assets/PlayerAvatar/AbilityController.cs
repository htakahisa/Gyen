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
  

    public GameObject lime;

    public int energy = 0;
    
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
        if(RoundManager.rm.Mode == "Practice")
        {
            energy = 10000;
        }

        if (PlayerManager.canAbility)
        {
            Ability();
        }

    }

    public void Ability()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Yellow();

        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Lime();
        }
    }


    public void Lime()
    {
        //必要数1
        //if (energy >= 1)
        {
            Transform mainCamera = GetComponentInChildren<Camera>().transform;
            CmdLime(mainCamera.position, mainCamera.forward);
            energy--;
        }
    }


    [Command]
    public void CmdLime(Vector3 pos, Vector3 dir)
    {
        GameObject instance = Instantiate(lime, GetHitInForward(pos, dir), Quaternion.identity);
        NetworkServer.Spawn(instance);
        RoundManager.spawns.Add(instance);
    }

    public void Yellow()
    {
        if (currentForm == PlayerForm.Human)
        {
            SwitchForm(PlayerForm.Bird);
        }
        else if(currentForm == PlayerForm.Bird)
        {
            SwitchForm(PlayerForm.Human);
        }
        energy--;
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

        PlayerForm newForm = (PlayerForm)newFormInt;

        currentForm = newForm;

        if (newForm == PlayerForm.Human)
        {

            nowControlled = objectList.FirstOrDefault(obj => obj.name == "PlayerObject");

            if (isLocalPlayer)
            {
                _controller.radius = 0.2f;
                _controller.height = 2.3f;
                _controller.center = new Vector3(0, 1.15f, 0);

           
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

}
