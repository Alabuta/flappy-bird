using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uvAnimation : MonoBehaviour
{
    public Vector2 step = new Vector2(.5f, 0f);

    Vector2 uvRange;
    Vector4 spriteCorners;

    Material material;

    float timer = 0f;

    void Start()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var sprite = spriteRenderer.sprite;

        material = spriteRenderer.material;

        var min = Vector2.one;
        var max = Vector2.zero;

        foreach (var uv2 in sprite.uv) {
            min = Vector2.Min(min, uv2);
            max = Vector2.Max(max, uv2);
        }

        uvRange = max - min;

        spriteCorners = new Vector4(min.x, min.y, max.x, max.y);
    }

    void Update()
    {
        timer += Time.deltaTime;

        var offset = step * timer;
        var clampedOffset = offset - new Vector2(Mathf.Floor(offset.x), Mathf.Floor(offset.y));

        material.SetVector("_UVScroll", clampedOffset * uvRange);
        material.SetVector("_SpriteCorners", spriteCorners);
    }
}
