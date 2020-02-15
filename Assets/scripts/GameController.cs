using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    public GameObject player;
    public GameObject frame;

    public Canvas idleStateCanvas;

    private Animator playerAnimator;
    private Animator idleStateCanvasAnimator;

    public float speed = 8;

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
        idleStateCanvasAnimator = idleStateCanvas.GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        updateOnState();
    }

    void updateOnIdleState()
    {
        if (Input.GetButton("Fire1")) {
            state = GameState.PLAY;
            updateOnState = updateOnPlayState;

            //playerAnimator.SetTrigger("GameHasStarted");
            idleStateCanvasAnimator.SetTrigger("GameHasStarted");
        }
    }
    void updateOnPlayState()
    {
        //playerAnimator.ResetTrigger("GameHasStarted");

        if (Input.GetButton("Fire1")) {
            var rigidbody = player.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(new Vector3(0f, 100f, 0f));
        }

        var frameTransform = frame.GetComponent<Transform>();
        frameTransform.position += new Vector3(speed, 0, 0) * Time.deltaTime;
    }
}
