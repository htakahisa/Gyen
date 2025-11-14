using System.Collections.Generic;
using UnityEngine;
using static WeaponStatus;

[CreateAssetMenu(fileName = "DuelLandFanaticsDatas", menuName = "MapData/DuelLandFanaticsDatas")]
public class DuelLandFanaticsDatas : ScriptableObject
{

    [System.Serializable]
    public struct ObjectAndPosition
    {
        public GameObject prefab;
        public Vector3 position;
        public Vector3 rotation;
        public WeaponType weapon;
        public float armer;


        public ObjectAndPosition(GameObject prefab, Vector3 position, Vector3 rotation, WeaponType weapon, float armer)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
            this.weapon = weapon;
            this.armer = armer;

        }
    }

    // InspectorÇ≈ê›íËâ¬î\Ç…Ç∑ÇÈ
    public List<ObjectAndPosition> gimmicks = new List<ObjectAndPosition>();
    public Vector3 defenderPos;
    public Vector3 spikePos;
    public GameObject mapPrefab;
    public string mapName;
}