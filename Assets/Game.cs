using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField] Text textComponent;
    int counter = 0;

    void Start()
    {
        textComponent.text = (counter.ToString());
    }

    void Update()
    {
        textComponent.text = (counter++.ToString());
    }
}
