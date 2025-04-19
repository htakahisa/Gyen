using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{


    public AudioClip footStep;
    public AudioClip land;
    public AudioClip shoot;

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
        // サーバーで音を再生
        AudioSource.PlayClipAtPoint(soundClip, position, volume);

        // クライアントにも再生させる
        RpcPlaySoundAtPoint(name, position, volume);
    }

    [ClientRpc]
    private void RpcPlaySoundAtPoint(string name, Vector3 position, float volume)
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

        // サーバー以外のクライアントで音を再生
        if (!isServer)
        {
            AudioSource.PlayClipAtPoint(soundClip, position, volume);
        }
    }
}
