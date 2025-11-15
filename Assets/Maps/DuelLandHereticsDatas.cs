using System.Collections.Generic;
using UnityEngine;
using static WeaponStatus;

[CreateAssetMenu(fileName = "DuelLandHereticsDatas", menuName = "MapData/DuelLandHereticsDatas")]
public class DuelLandHereticsDatas : ScriptableObject
{

    [System.Serializable]
    public struct ObjectAndPosition
    {
        public GameObject prefab;
        public Vector3 position;
        public Vector3 rotation;
        public WeaponType weapon;
        public float armer;
        public int moveVector;
        public float moveAsScriptTime;
        public bool movingAsScript;
        public float waitForStart;


        public ObjectAndPosition(GameObject prefab, Vector3 position, Vector3 rotation, WeaponType weapon, float armer, int moveVector, float moveAsScriptTime, bool movingAsScript, float waitForStart)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
            this.weapon = weapon;
            this.armer = armer;
            this.moveVector = moveVector;
            this.moveAsScriptTime = moveAsScriptTime;
            this.movingAsScript = movingAsScript;
            this.waitForStart = waitForStart;
        }
    }

    // InspectorÇ≈ê›íËâ¬î\Ç…Ç∑ÇÈ
    public List<ObjectAndPosition> gimmicks = new List<ObjectAndPosition>();
    public Vector3 attackerPos;
    public Vector3 attackerRot;
    public Vector3 spikePos;
    public GameObject mapPrefab;
    public string mapName;
}