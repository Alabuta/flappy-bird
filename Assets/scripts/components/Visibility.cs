using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class VisibilityComponent : MonoBehaviour {
    public event Action onBecameInvisible = delegate{};
    public event Action onBecameVisible = delegate{};

    void OnBecameInvisible()
    {
        onBecameInvisible();
    }

    void OnBecameVisible()
    {
        onBecameVisible();
    }
}
