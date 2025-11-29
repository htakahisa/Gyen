using UnityEngine;

[CreateAssetMenu(menuName = "GameConfig/CharacterStats")]
public class CharacterStats : ScriptableObject
{
    public float defaultMoveSpeed = 5f;
    public float defaultSprintSpeed = 5f;
    public float defaultJumpHeight = 1;
    public float defaultGravity = -10;
    public float defaultAssistRange = 30;
    public float defaultMaxAssistAngle = 6;
    public float defaultAssistStrength = 10;
    public float defaultFootStepTime = 0.4f;
    public float defaultGroundAcceleration = 5f;
    public float defaultGroundDeceleration = 5f;
    public float defaultAirControl = 0.3f;
    public float defaultMaxAirSpeedMultiplier = 0.9f;
    public float defaultCounterStrafeStrength = 2f;
}
