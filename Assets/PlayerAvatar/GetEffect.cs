using Mirror;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;


public class GetEffect : NetworkBehaviour
{

    public UnityEvent effect;


    public bool hadGot = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Active()
    {
        if (hadGot)
        {
            return;
        }

        effect?.Invoke();

        hadGot = true;

        if (!NetworkClient.active || !Application.isPlaying)
            return;

        // 自分のプレイヤーオブジェクトを取得してCommandを呼ぶ
        var localPlayer = NetworkClient.connection.identity;
        localPlayer.GetComponent<ThirdPersonController>()?.RequestDestroy(netId);
    }

    [Server]
    public void ServerDestroy()
    {
        RpcDestroy();
    }

    [ClientRpc]
    public void RpcDestroy()
    {
        NetworkServer.Destroy(gameObject);
    }

    public void DebugCall()
    {
        Debug.Log("It called");
    }
    public void ChargeEnergy()
    {
        RoundManager.rm.GetMyPlayer().GetComponent<AbilityController>().energy ++;
    }



}
