using Mirror;
using UnityEngine;

public class FinisherManager : NetworkBehaviour
{
    public WeaponFinisher[] finishers;
    public static FinisherManager instance;

    private void Awake()
    {
        instance = this;
    }


    // ÉâÉEÉìÉhèIóπéûÇ…åƒÇ‘
    [Server]
    public void PlayPlayerFinisher(WeaponDatabase weaponData, GameObject losePlayer)
    {
        GameObject prefab = GetFinisher(weaponData.weaponName);
        Vector3 pos = losePlayer.transform.position + losePlayer.transform.up;

        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
            NetworkServer.Spawn(obj);
            RoundManager.spawns.Add(obj);
            AudioManager.Instance.CmdPlaySoundAtPoint(GetSound(weaponData.weaponName), pos, 1);
        }
    }

    public GameObject GetFinisher(string weaponID)
    {
        foreach (var f in finishers)
        {
            if (f.weaponID == weaponID)
                return f.finisherPrefab;
        }
        return null;
    }
    public AudioManager.Sounds GetSound(string weaponID)
    {
        foreach (var f in finishers)
        {
            if (f.weaponID == weaponID)
                return f.soundEffect;
        }
        return AudioManager.Sounds.LOVERFINISHER;
    }

}

[System.Serializable]
public struct WeaponFinisher
{
    public string weaponID;
    public GameObject finisherPrefab;
    public AudioManager.Sounds soundEffect;
}
