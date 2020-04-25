using UnityEngine;


[CreateAssetMenu(fileName = "PlayerParams", menuName = "Player Params", order = 1)]
public sealed class PlayerParams : ScriptableObject {
    [Header("Physics Settings"), Range(1, 30)]
    public float jumpForceScale = 7f;
    [Range(0, 30)]
    public float deadJumpForceScale = 27f;
    [Range(0, 10)]
    public float movementVelocity = 8f;

    [Header("Roll Animation Settings"), Range(-90f, 0f)]
    public float minRollAngle = -90f;

    [Range(0f, 90f)]
    public float maxRollAngle = 22f;

    [Range(0f, 1f)]
    public float angularVelocityScaler = .01f;

    [Range(-180f, 0f)]
    public float minAngularVelocity = -2.8f;

    [Range(0f, 180f)]
    public float maxAngularVelocity = 45f;

    [Range(0f, 2f)]
    public float negativeAngularVelocityScaler = 1.1f;
}
