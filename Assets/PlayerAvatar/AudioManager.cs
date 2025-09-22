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
    public AudioClip hitBlood;

    public LayerMask soundBlockLayer;


    // Start is called before the first frame update
    public enum Sounds
    {
        FOOTSTEP,
        LAND,
        SHOOT,
        YELLOW,
        LASER,
        HITBLOOD,
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Command]
    public void CmdPlaySoundAtPoint(Sounds name, Vector3 position, float volume)
    {

        RpcPlaySoundAtPoint(name, position, volume);
    }

    [ClientRpc]
    private void RpcPlaySoundAtPoint(Sounds name, Vector3 position, float volume)
    {
        Debug.Log("SoundIsPlayed");

        Vector3 dir = position - RoundManager.rm.GetMyPlayer().transform.position;

        if (Physics.Raycast(RoundManager.rm.GetMyPlayer().transform.position, dir, out RaycastHit hit, dir.magnitude, soundBlockLayer))
        {

             // •Ç‚ÉŽÕ‚ç‚ê‚Ä‚¢‚é
             Debug.Log("SoundWasBlocked!");
             return;
                        
           
        }

        AudioClip soundClip = null;

        switch (name)
        {
            case Sounds.FOOTSTEP:
                soundClip = footStep;
                break;

            case Sounds.LAND:
                soundClip = land;
                break;

            case Sounds.SHOOT:
                soundClip = shoot;
                break;

            case Sounds.YELLOW:
                soundClip = yellow;
                break;

            case Sounds.LASER:
                soundClip = laser;
                break;

            case Sounds.HITBLOOD:
                soundClip = hitBlood;
                break;
        }


        AudioSource.PlayClipAtPoint(soundClip, position, volume);
        
    }


}
