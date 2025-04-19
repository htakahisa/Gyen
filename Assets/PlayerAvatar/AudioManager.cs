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
        // �T�[�o�[�ŉ����Đ�
        AudioSource.PlayClipAtPoint(soundClip, position, volume);

        // �N���C�A���g�ɂ��Đ�������
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

        // �T�[�o�[�ȊO�̃N���C�A���g�ŉ����Đ�
        if (!isServer)
        {
            AudioSource.PlayClipAtPoint(soundClip, position, volume);
        }
    }
}
