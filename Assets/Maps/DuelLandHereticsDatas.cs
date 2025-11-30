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
        public float foundDelayTime;


        public ObjectAndPosition(GameObject prefab, Vector3 position, Vector3 rotation, WeaponType weapon, float armer, int moveVector, float moveAsScriptTime, bool movingAsScript, float foundDelayTime)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
            this.weapon = weapon;
            this.armer = armer;
            this.moveVector = moveVector;
            this.moveAsScriptTime = moveAsScriptTime;
            this.movingAsScript = movingAsScript;
            this.foundDelayTime = foundDelayTime;
        }

    }
    [System.Serializable]
    public struct ObjectWithBot
    {
        public GameObject prefab;
        public Vector3 position;
        public Vector3 rotation;
        public int withIndex;
        public float delayTime;
        public float probability;

        public ObjectWithBot(GameObject prefab, Vector3 position, Vector3 rotation, int withIndex, float delayTime, float probability)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
            this.withIndex = withIndex;
            this.delayTime = delayTime;
            this.probability = probability;
        }

    }

    // InspectorÇ≈ê›íËâ¬î\Ç…Ç∑ÇÈ
    public List<ObjectAndPosition> gimmicks = new List<ObjectAndPosition>();
    public List<ObjectWithBot> gimmicksWithBot = new List<ObjectWithBot>();
    public List<ObjectAndPosition> standingGimmicks = new List<ObjectAndPosition>();
    public Vector3 attackerPos;
    public Vector3 attackerRot;
    public Vector3 spikePos;
    public GameObject mapPrefab;
    public string mapName;
    public List<float> waitForSecondsList = new List<float>();
}