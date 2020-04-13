using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;


public abstract class GameState {
    protected GameController gameController;

    protected InputSystem inputSystem;

    protected Animator canvasAnimator;
    protected Animator playerAnimator;

    protected Action OnFinishAction;

    public GameState(GameController gc, Action onFinishAction)
    {
        gameController = gc;
        OnFinishAction = onFinishAction;

        inputSystem = new InputSystem();
    }

    public abstract void Update();
    public abstract void FixedUpdate();
}

public class GameStateIdle : GameState {
    Rigidbody2D rigidbody;
    GameObject player;

    public GameStateIdle(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        canvasAnimator = gc.idleStateCanvasAnimator;
        playerAnimator = gc.playerAnimator;

        player = gc.player;

        rigidbody = player.GetComponent<Rigidbody2D>();
        rigidbody.simulated = false;

        inputSystem.AddInputHandler("Fire1",
            () => {
                playerAnimator.SetTrigger("GameHasStarted");
                canvasAnimator.SetTrigger("GameHasStarted");

                OnFinishAction();
            },
            () => { },
            () => { }
        );
    }

    public override void Update()
    {
        inputSystem.Update();
    }

    public override void FixedUpdate()
    {
        var playerTransform = player.GetComponent<Transform>();
        playerTransform.position -= Vector3.up * Mathf.Sin(2 * Mathf.PI * Time.time / .58f) * .064f;
    }
}

public class GameStatePlay : GameState {
    GameObject player;
    GameObject frame;

    Rigidbody2D rigidbody;

    float rollAngle = 0f;

    float jumpStartTime = 0f;

    delegate void FixedUpdateDelegate();
    FixedUpdateDelegate FixedUpdateFunc;


    public GameStatePlay(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        playerAnimator = gc.playerAnimator;
        player = gc.player;

        rigidbody = gc.player.GetComponent<Rigidbody2D>();
        rigidbody.simulated = true;

        playerAnimator.ResetTrigger("GameHasStarted");

        inputSystem.AddInputHandler("Fire1",
            OnFirePressed,
            OnFireHeld,
            OnFireUnpressed
        );

        FixedUpdateFunc = () => { };
    }

    public override void Update()
    {
        inputSystem.Update();

        //var multiplier = 1f;

        //if (rigidbody.velocity.y < 0)
        //    multiplier = gameController.playerParams.fallMultiplier;

        //else if (rigidbody.velocity.y > 0 && !Input.GetButton("Fire1"))
        //    multiplier = lowJumpMultiplier;

        //rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1) * Time.deltaTime;

        /*var frameTransform = frame.GetComponent<Transform>();
        frameTransform.position += Vector3.right * gameController.playerParams.movementVelocity * Time.deltaTime;*/

        //foreach (var pipe in pipes) {
        //    var tr = pipe.GetComponent<Transform>();
        //    tr.position += Vector3.left * 8f * Time.deltaTime;
        //}
    }

    public override void FixedUpdate()
    {
        FixedUpdateFunc();

        float angle = 0f;

        if (rollAngle < Mathf.PI / 1) {
            angle = Mathf.PI / 4;
            rollAngle += Mathf.PI / 4;//16f * rigidbody.velocity.magnitude;
        }

        player.transform.Rotate(Vector3.forward, angle);
    }

    void OnFirePressed()
    {
        //rigidbody.AddForce(Vector3.up * 100f);
        //rigidbody.velocity = Vector3.up * gameController.playerParams.jumpVelocity;
        playerAnimator.SetTrigger("WingsHaveFlapped");

        rigidbody.velocity = Vector2.zero;
        player.transform.Rotate(Vector2.zero);

        jumpStartTime = Time.time;

        rigidbody.velocity = Vector2.zero;
        rigidbody.angularVelocity = 0;

        player.transform.Rotate(Vector3.forward, -rollAngle);
        rollAngle = 0f;

        FixedUpdateFunc = () => { };
    }

    void OnFireHeld()
    {
        FixedUpdateFunc = () =>
        {
            if (Time.time - jumpStartTime < .061f) {
                rigidbody.AddForce(-Vector2.up * Physics2D.gravity * rigidbody.gravityScale * 5f);
            }
        };
    }

    void OnFireUnpressed()
    {
        playerAnimator.ResetTrigger("WingsHaveFlapped");

        FixedUpdateFunc = () => { };
    }
}