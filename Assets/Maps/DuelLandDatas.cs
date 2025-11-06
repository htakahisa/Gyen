using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DuelLandDatas", menuName = "MapData/DuelLandDatas")]
public class DuelLandDatas : ScriptableObject
{
    [System.Serializable]
    public struct ObjectAndPosition
    {
        public GameObject prefab;
        public Vector3 position;
        public float yaw;

        public ObjectAndPosition(GameObject prefab, Vector3 position, float yaw = 0)
        {
            this.prefab = prefab;
            this.position = position;
            this.yaw = yaw;
        }
    }

    // InspectorÇ≈ê›íËâ¬î\Ç…Ç∑ÇÈ
    public List<ObjectAndPosition> gimmicks = new List<ObjectAndPosition>();
    public Vector3 diffenderPos;
    public Vector3 spikePos;
    public GameObject mapPrefab;
}