using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public interface IInputHandler {
    void OnPressed();
    void OnHeld();
    void OnUnpressed();
}

public class InputSystem {
    Dictionary<string, List<IInputHandler>> handlers;

    public void AddInputHandler(IInputHandler handler, string buttonName)
    {
        if (!handlers.ContainsKey(buttonName))
            handlers.Add(buttonName, new List<IInputHandler>());

        handlers[buttonName].Add(handler);
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
