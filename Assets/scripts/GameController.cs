using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    public GameObject player;

    private Animator playerAnimator;

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

        playerAnimator = player.GetComponent<Animator>();
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

            playerAnimator.SetTrigger("GameHasStarted");
        }
    }
    void updateOnPlayState()
    {
        //playerAnimator.ResetTrigger("GameHasStarted");

        if (Input.GetButton("Fire1")) {
            var rigidbody = player.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(new Vector3(0f, 10f, 0f));
        }
    }
}
