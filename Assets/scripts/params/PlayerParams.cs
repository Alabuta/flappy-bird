using UnityEngine;


[CreateAssetMenu(fileName = "PlayerParams", menuName = "Player Params", order = 1)]
public sealed class PlayerParams : ScriptableObject {
    [Header("Physics Settings"), Range(1, 30)]
    public float jumpForceScale = 6f;
    [Range(0, 30)]
    public float deadJumpForceScale = 27f;
    [Range(0, 10)]
    public float movementVelocity = 8f;

    [Header("Roll Animation Settings"), Range(-Mathf.PI / 2f, 0f)]
    public float minRollAngle = -Mathf.PI / 2f;

    [Range(0f, Mathf.PI / 2f)]
    public float maxRollAngle = Mathf.PI / 8f;

    [Range(0f, 1f)]
    public float angularVelocityScaler = .01f;

    [Range(-Mathf.PI, 0f)]
    public float minAngularVelocity = -Mathf.PI / 64f;

    [Range(0f, Mathf.PI)]
    public float maxAngularVelocity = Mathf.PI / 4f;

    [Range(0f, 2f)]
    public float negativeAngularVelocityScaler = 1.1f;
}
