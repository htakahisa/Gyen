using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeManager : MonoBehaviour
{
    public GameObject player;

    // Start is called before the first frame update
    void Awake()
    {
        SpawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnPlayer()
    {
        // プレイヤーをインスタンス化
        GameObject playerPrefab = Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity);
    }
}
