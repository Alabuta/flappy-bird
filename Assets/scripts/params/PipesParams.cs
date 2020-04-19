using UnityEngine;

[CreateAssetMenu(fileName = "PipesParams", menuName = "Pipe Params", order = 1)]
public sealed class PipesParams : ScriptableObject {
    [Header("Spawn Settings"), Range(1, 10)]
    public int number = 5;

    [Range(0, 20)]
    public float offset = 8f;

    public Vector3 startPoint = new Vector3(16f, 0f, 5f);
}
