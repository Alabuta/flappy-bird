using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVScroller : MonoBehaviour {
    public Vector2 velocity = new Vector2(0f, 0f);

    Vector2 offset;
    Vector2 unitsPerUV;
    Vector2 uvRange;
    Vector4 spriteCorners;

    Material material;

    void Start()
    {
        offset = Vector2.zero;

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

        unitsPerUV = new Vector2(sprite.bounds.extents.x, sprite.bounds.extents.y) / uvRange;

        spriteCorners = new Vector4(min.x, min.y, max.x, max.y);
    }

    void Update()
    {
        offset += velocity * Time.deltaTime / unitsPerUV;

        var clampedOffset = offset - new Vector2(Mathf.Floor(offset.x), Mathf.Floor(offset.y));

        material.SetVector("_UVScroll", clampedOffset * uvRange);
        material.SetVector("_SpriteCorners", spriteCorners);
    }
}
