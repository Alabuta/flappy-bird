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

    public GameStateIdle(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        canvasAnimator = gc.idleStateCanvasAnimator;
        playerAnimator = gc.playerAnimator;

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
        ;
    }
}

public class GameStatePlay : GameState {
    GameObject player;
    GameObject frame;

    Rigidbody2D rigidbody;

    float rollAngle = 0f;


    public GameStatePlay(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        playerAnimator = gc.playerAnimator;
        player = gc.player;

        rigidbody = player.GetComponent<Rigidbody2D>();

        playerAnimator.ResetTrigger("GameHasStarted");

        inputSystem.AddInputHandler("Fire1",
            OnFirePressed,
            () => { },
            OnFireUnpressed
        );
    }

    public override void Update()
    {
        inputSystem.Update();

        var multiplier = 1f;

        if (rigidbody.velocity.y < 0)
            multiplier = gameController.playerParams.fallMultiplier;

        //else if (rigidbody.velocity.y > 0 && !Input.GetButton("Fire1"))
        //    multiplier = lowJumpMultiplier;

        rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1) * Time.deltaTime;

        /*var frameTransform = frame.GetComponent<Transform>();
        frameTransform.position += Vector3.right * gameController.playerParams.movementVelocity * Time.deltaTime;*/

        //foreach (var pipe in pipes) {
        //    var tr = pipe.GetComponent<Transform>();
        //    tr.position += Vector3.left * 8f * Time.deltaTime;
        //}
    }

    public override void FixedUpdate()
    {
        player.transform.Rotate(Vector3.forward * rollAngle);
    }

    void OnFirePressed()
    {
        //rigidbody.AddForce(Vector3.up * 100f);
        rigidbody.velocity = Vector3.up * gameController.playerParams.jumpVelocity;
        playerAnimator.SetTrigger("WingsHaveFlapped");
    }

    void OnFireUnpressed()
    {
        playerAnimator.ResetTrigger("WingsHaveFlapped");
    }
}