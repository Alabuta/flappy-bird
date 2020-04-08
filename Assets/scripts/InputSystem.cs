using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


struct InputEventHandler {
    public InputEventHandler(Action onPressed, Action onHeld, Action onUnpressed)
    {
        OnPressed = onPressed;
        OnHeld = onHeld;
        OnUnpressed = onUnpressed;
    }

    public Action OnPressed { get; }
    public Action OnHeld { get; }
    public Action OnUnpressed { get; }
}

public class InputSystem {
    Dictionary<string, List<InputEventHandler>> handlers;

    public InputSystem()
    {
        handlers = new Dictionary<string, List<InputEventHandler>>();
    }

    public void AddInputHandler(string buttonName, Action onPressed, Action onHeld, Action onUnpressed)
    {
        if (!handlers.ContainsKey(buttonName))
            handlers.Add(buttonName, new List<InputEventHandler>());

        handlers[buttonName].Add(new InputEventHandler(
            onPressed, onHeld, onUnpressed
        ));
    }

    public void Update()
    {
        foreach (var item in handlers) {
            if (Input.GetButtonDown(item.Key))
                foreach (var handler in item.Value)
                    handler.OnPressed();

            else if (Input.GetButton(item.Key))
                foreach (var handler in item.Value)
                    handler.OnHeld();

            else if (Input.GetButtonUp(item.Key))
                foreach (var handler in item.Value)
                    handler.OnUnpressed();
        }
    }
}
