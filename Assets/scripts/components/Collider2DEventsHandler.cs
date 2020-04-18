using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class Collider2DEventsHandler : MonoBehaviour {
    public event Action<Collision2D> onCollisionEnter2D = delegate{};
    public event Action<Collision2D> onCollisionStay2D = delegate{};
    public event Action<Collision2D> onCollisionExit2D = delegate{};

    public event Action<Collider2D> onTriggerEnter2D = delegate{};
    public event Action<Collider2D> onTriggerStay2D = delegate{};
    public event Action<Collider2D> onTriggerExit2D = delegate{};

    void OnCollisionEnter2D(Collision2D collision)
    {
        onCollisionEnter2D(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        onCollisionStay2D(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        onCollisionExit2D(collision);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        onTriggerEnter2D(collider);
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        onTriggerStay2D(collider);
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        onTriggerExit2D(collider);
    }
}
