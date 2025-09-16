using Mirror;
using UnityEngine;

public class YellowManager : NetworkBehaviour
{

    public float time;
    public AbilityController abilityController;
    public Camera birdCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartAuthority()
    {
        birdCamera.enabled = true;
    }

    public void StartChange()
    {
        Invoke("RpcRemove", time);
    }


    [ClientRpc]
    public void RpcRemove()
    {
        if (abilityController.currentForm == AbilityController.PlayerForm.Bird)
        {
            abilityController.SwitchForm(AbilityController.PlayerForm.Human);
        }
    }
}
