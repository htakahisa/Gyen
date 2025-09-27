using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{
    public static AudioManager Instance;

    public AudioClip footStep;
    public AudioClip land;
    public AudioClip shoot;
    public AudioClip yellow;
    public AudioClip laser;
    public AudioClip hitBlood;
    public AudioClip horusDestroyed;
    public AudioClip ITweakDestroyed;
    public AudioClip IMirrorDestroyed;
    public AudioClip ITweaksLaser;
    public AudioClip LoverFinisher;
    public AudioClip StarFinisher;
    public AudioClip IceFinisher;
    public AudioClip MeteorFinisher;

    public LayerMask soundBlockLayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Start is called before the first frame update
    public enum Sounds
    {
        FOOTSTEP,
        LAND,
        SHOOT,
        YELLOW,
        LASER,
        HITBLOOD,
        HORUSDESTROYED,
        ITWEAKSDESTROYED,
        IMIRRORDESTROYED,
        ITWEAKSLASER,
        LOVERFINISHER,
        STARFINISHER,
        ICEFINISHER,
        METEORFINISHER,
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    [Command(requiresAuthority = false)]
    public void CmdPlaySoundAtPoint(Sounds name, Vector3 position, float volume)
    {

        RpcPlaySoundAtPoint(name, position, volume);
    }


    [Command(requiresAuthority = false)]
    public void CmdStopBGM()
    {
        RpcStopBGM();
    }

    [ClientRpc]
    public void RpcStopBGM()
    {
        GetComponent<AudioSource>().Stop();
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

            case Sounds.HORUSDESTROYED:
                soundClip = horusDestroyed;
                break;

            case Sounds.ITWEAKSDESTROYED:
                soundClip = ITweakDestroyed;
                break;

            case Sounds.IMIRRORDESTROYED:
                soundClip = IMirrorDestroyed;
                break;

            case Sounds.ITWEAKSLASER:
                soundClip = ITweaksLaser;
                break;

            case Sounds.LOVERFINISHER:
                soundClip = LoverFinisher;
                break;

            case Sounds.STARFINISHER:
                soundClip = StarFinisher;
                break;

            default:
                switch (name)
                {
                    case Sounds.ICEFINISHER:
                        soundClip = IceFinisher;
                        break;

                    case Sounds.METEORFINISHER:
                        soundClip = MeteorFinisher;
                        break;


                }
                GetComponent<AudioSource>().resource = soundClip;
                GetComponent<AudioSource>().Play();
                return;

        }
        AudioSource.PlayClipAtPoint(soundClip, position, volume);

 
        
    }


}
