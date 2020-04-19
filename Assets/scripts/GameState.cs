﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;


public abstract class GameState {
    protected GameController gameController;

    protected PlayerParams playerParams;

    protected InputSystem inputSystem;

    protected Animator canvasAnimator;
    protected Animator playerAnimator;

    protected Action OnFinishAction;

    public float movementVelocity { get; protected set; }

    public GameState(GameController gc, Action onFinishAction)
    {
        gameController = gc;
        OnFinishAction = onFinishAction;

        inputSystem = new InputSystem();

        playerParams = gc.playerParams;
        movementVelocity = playerParams.movementVelocity;
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

        gc.platform.GetComponent<UVScroller>().velocity = new Vector2(movementVelocity, 0f);

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

        player.GetComponent<Collider2DEventsHandler>().onTriggerEnter2D += OnPlayerTriggerCollision;

        playerAnimator.ResetTrigger("GameHasStarted");

        inputSystem.AddInputHandler("Fire1",
            OnFirePressed,
            OnFireHeld,
            OnFireUnpressed
        );

        gc.platform.GetComponent<UVScroller>().velocity = new Vector2(movementVelocity, 0f);

        FixedUpdateFunc = () => { };
    }

    public override void Update()
    {
        inputSystem.Update();

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

        float angularVelocity = Mathf.Clamp(
            rigidbody.velocity.y * playerParams.angularVelocityScaler,
            playerParams.minAngularVelocity, playerParams.maxAngularVelocity
        );

        if (angularVelocity < 0f) {
            angularVelocity = -Mathf.Pow(Mathf.Abs(angularVelocity), playerParams.negativeAngularVelocityScaler);
        }

        rollAngle += angularVelocity;
        rollAngle = Mathf.Clamp(rollAngle, playerParams.minRollAngle, playerParams.maxRollAngle);

        player.transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * rollAngle, Vector3.forward);
    }

    void OnFirePressed()
    {
        playerAnimator.SetTrigger("WingsHaveFlapped");

        rigidbody.velocity = Vector2.zero;
        player.transform.Rotate(Vector2.zero);

        jumpStartTime = Time.time;

        rigidbody.velocity = Vector2.zero;
        rigidbody.AddForce(-Physics2D.gravity * rigidbody.gravityScale);

        FixedUpdateFunc = () => { };
    }

    void OnFireHeld()
    {
        FixedUpdateFunc = () =>
        {
            if (Time.time - jumpStartTime < .0964f)
                rigidbody.AddForce(-Physics2D.gravity * rigidbody.gravityScale * playerParams.jumpForceScale);
        };
    }

    void OnFireUnpressed()
    {
        playerAnimator.ResetTrigger("WingsHaveFlapped");

        rigidbody.AddForce(Physics2D.gravity * rigidbody.gravityScale);

        FixedUpdateFunc = () => { };
    }

    void OnPlayerTriggerCollision(Collider2D collider)
    {
        player.GetComponent<Collider2DEventsHandler>().onTriggerEnter2D -= OnPlayerTriggerCollision;

        rigidbody.velocity = Vector2.zero;
        rigidbody.AddForce(-Physics2D.gravity * rigidbody.gravityScale * playerParams.deadJumpForceScale);

        OnFinishAction();
    }
}

public class GameStateFail : GameState {
    GameObject player;
    Rigidbody2D rigidbody;
    UVScroller uvScroller;

    float rollAngle;
    float startTime;
    float timeScaler;

    public GameStateFail(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        //canvasAnimator = gc.idleStateCanvasAnimator;

        gc.playerAnimator.enabled = false;

        player = gc.player;

        rollAngle = 0f;

        rigidbody = player.GetComponent<Rigidbody2D>();
        rigidbody.velocity = Vector2.zero;

        player.transform.Rotate(Vector2.zero);

        startTime = Time.time;

        uvScroller = gc.platform.GetComponent<UVScroller>();
    }

    public override void Update()
    {
        ;
        uvScroller.velocity = new Vector2(movementVelocity, 0f);
    }

    public override void FixedUpdate()
    {
        float angularVelocity = Mathf.Clamp(
            rigidbody.velocity.y * playerParams.angularVelocityScaler,
            playerParams.minAngularVelocity * 2f, playerParams.maxAngularVelocity
        );

        if (angularVelocity < 0f) {
            angularVelocity = -Mathf.Pow(Mathf.Abs(angularVelocity), playerParams.negativeAngularVelocityScaler);
        }

        rollAngle += angularVelocity;
        rollAngle = Mathf.Clamp(rollAngle, playerParams.minRollAngle, playerParams.maxRollAngle);

        player.transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * rollAngle, Vector3.forward);

        movementVelocity = Mathf.Lerp(playerParams.movementVelocity, 0, (Time.time - startTime) / 1f);

        //uvScroller.velocity = Mathf.Lerp(timeScaler, 0, (Time.time - startTime) / 1f);
    }
}
