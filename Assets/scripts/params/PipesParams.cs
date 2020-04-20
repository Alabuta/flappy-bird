using UnityEngine;

[CreateAssetMenu(fileName = "PipesParams", menuName = "Pipe Params", order = 1)]
public sealed class PipesParams : ScriptableObject {
    [Header("Spawn Settings"), Range(1, 10)]
    public int number = 5;

    [Range(0, 20)]
    public float offset = 8f;

    [Range(0, 5)]
    public float pipesVerticalGapMin = 2f;
    [Range(0, 5)]
    public float pipesVerticalGapMax = 5f;

    public Vector3 randomOffset = new Vector3(1f, 10f, 0f);

    public Vector3 startPoint = new Vector3(16f, 0f, 5f);
}
