using UnityEngine;

[CreateAssetMenu(fileName = "MapDatas", menuName = "MapData/MapDatas")]
public class MapDatas : ScriptableObject
{
    public Vector3 attackerPos;
    public Vector3 diffenderPos;
    public Vector3 spikePos;
    public GameObject mapPrefab;
}