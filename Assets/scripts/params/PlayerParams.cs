using UnityEngine;


[CreateAssetMenu(fileName = "PlayerParams", menuName = "Player Params", order = 1)]
public sealed class PlayerParams : ScriptableObject {
    [Header("Physics Settings"), Range(1, 10)]
    public float jumpVelocity = 1f;
    [Range(0, 10)]
    public float movementVelocity = 8f;
    [Range(0, 10)]
    public float fallMultiplier = 2.4f;
    [Range(0, 10)]
    public float lowJumpMultiplier = 2f;

    [Header("Size Settings"), Range(.1f, 1f)]
    public float radius;
}
