using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YellowManager : NetworkBehaviour
{

    public float time;
    public AbilityController abilityController;
    public Camera birdCamera;

    private Coroutine removeCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartAuthority()
    {
        birdCamera.enabled = true;
    }

    private void Update()
    {
        if (removeCoroutine != null && abilityController.currentForm != AbilityController.PlayerForm.Bird)
        {
            StopCoroutine(removeCoroutine);
        }
    }

    public void StartChange()
    {
        removeCoroutine = StartCoroutine(RemoveCotourine());
    }

    public IEnumerator RemoveCotourine()
    {
        yield return new WaitForSeconds(time);
        RpcRemove();
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
