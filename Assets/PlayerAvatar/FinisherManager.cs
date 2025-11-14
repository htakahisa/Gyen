using Mirror;
using UnityEngine;

public class FinisherManager : NetworkBehaviour
{
    public WeaponFinisher[] finishers;
    public static FinisherManager instance;
    public Sprite normalIcon;
    public Sprite headIcon;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {

    }

    // ラウンド終了時に呼ぶ
    [Server]
    public void PlayPlayerFinisher(WeaponDatabase weaponData, GameObject losePlayer, bool headshot)
    {
        GameObject prefab = GetFinisher(weaponData.weaponName);
        Vector3 pos = losePlayer.transform.position + losePlayer.transform.up;

        Sprite icon = normalIcon;

        if (headshot)
        {
            icon = headIcon; // 現在の武器アイコン
        }
        else
        {
            icon = normalIcon; // 現在の武器アイコン
        }

        GameObject winnerPlayer;
        if(losePlayer == RoundManager.rm.GetMyPlayer())
        {
            winnerPlayer = RoundManager.rm.GetOtherPlayer();
        }
        else
        {
            winnerPlayer = RoundManager.rm.GetMyPlayer();
        }

        NetworkConnection conn = winnerPlayer.GetComponent<NetworkIdentity>().connectionToClient;

        KillCircleUI.Instance.TargetPlayEffect(conn, icon.name, headshot);

        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
            NetworkServer.Spawn(obj);
            RoundManager.spawns.Add(obj);
            AudioManager.Instance.CmdPlaySoundAtPoint(GetSound(weaponData.weaponName), pos, 0.3f, 30);
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
