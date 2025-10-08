using Mirror;
using UnityEngine;

public class WhatIfAuthority : NetworkBehaviour
{

    public Events particularEvent;

    public enum Events
    {
        SETACTIVEFALSE,
        SETACTIVETRUE,
        LAYERTOMINIMAP,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<SpawnOwner>() != null) {

            if (GetComponent<SpawnOwner>().IsMine())
            {
                Calls();
            }   

        }

        if (isLocalPlayer)
        {
            Calls();
        }

    }

    public void Calls()
    {
        switch (particularEvent)
        {
            case Events.SETACTIVEFALSE:
                gameObject.SetActive(false);
                break;

            case Events.SETACTIVETRUE:
                gameObject.SetActive(true);
                break;

            case Events.LAYERTOMINIMAP:
                gameObject.layer = 12;
                break;

        }
    }

}
