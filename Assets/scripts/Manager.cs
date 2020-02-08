using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {
    public GameObject player;

    private int playerScore = 0;

    private enum GameState {
        IDLE,
        PLAY,
        FAIL,
        SCORE
    }

    private GameState state;

    private delegate void DelegateUpdateOnState();
    DelegateUpdateOnState updateOnState;

    void Start()
    {
        state = GameState.IDLE;

        updateOnState = updateOnIdleState;
    }

    void Update()
    {
        updateOnState();
    }

    void updateOnIdleState()
    {
        if (Input.GetButton("Fire1")) {
            state = GameState.PLAY;
            updateOnState = updateOnPlayState;
        }
    }
    void updateOnPlayState()
    {
        if (Input.GetButton("Fire1")) {
            Debug.Log(4444);
        }
    }
}
