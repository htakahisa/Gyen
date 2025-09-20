using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{


    public AudioClip footStep;
    public AudioClip land;
    public AudioClip shoot;
    public AudioClip yellow;
    public AudioClip laser;

    public LayerMask soundBlockLayer;


    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Command]
    public void CmdPlaySoundAtPoint(string name, Vector3 position, float volume)
    {
        AudioClip soundClip = null;

        if (name == "footStep")
        {
            soundClip = footStep;
        }
        if (name == "land")
        {
            soundClip = land;
        }
        if (name == "shoot")
        {
            soundClip = shoot;
        }
        if (name == "yellow")
        {
            soundClip = yellow;
        }
        if (name == "laser")
        {
            soundClip = laser;
        }

        // クライアントにも再生させる
        RpcPlaySoundAtPoint(name, position, volume);
    }

    [ClientRpc]
    private void RpcPlaySoundAtPoint(string name, Vector3 position, float volume)
    {
        Debug.Log("SoundIsPlayed");

        Vector3 dir = position - RoundManager.rm.GetMyPlayer().transform.position;

        if (Physics.Raycast(RoundManager.rm.GetMyPlayer().transform.position, dir, out RaycastHit hit, dir.magnitude, soundBlockLayer))
        {

             // 壁に遮られている
             Debug.Log("SoundWasBlocked!");
             return;
                        
           
        }

        AudioClip soundClip = null;

        if (name == "footStep")
        {
            soundClip = footStep;
        }
        if (name == "land")
        {
            soundClip = land;
        }
        if (name == "shoot")
        {
            soundClip = shoot;
        }
        if (name == "yellow")
        {
            soundClip = yellow;
        }
        if (name == "laser")
        {
            soundClip = laser;
        }


        AudioSource.PlayClipAtPoint(soundClip, position, volume);
        
    }


}
