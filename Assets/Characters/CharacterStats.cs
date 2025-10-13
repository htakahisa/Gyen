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
}
