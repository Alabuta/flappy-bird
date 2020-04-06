﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;


public abstract class GameState {
    protected GameController gameController;

    public GameState(GameController gc) => gameController = gc;

    public abstract void OnFireButtonPressed();
    public abstract void OnFireButtonHeld();
    public abstract void OnFireButtonUnpressed();

    public abstract void Update();
    public abstract void FixedUpdate();
}

public class GameStateIdle : GameState {
    Animator canvasAnimator;
    Animator playerAnimator;

    public GameStateIdle(GameController gc) : base(gc)
    {
        canvasAnimator = gc.idleStateCanvasAnimator;
        playerAnimator = gc.playerAnimator;
    }

    public override void OnFireButtonPressed()
    {
        ;
    }

    public override void OnFireButtonHeld()
    {
        ;
    }

    public override void OnFireButtonUnpressed()
    {
        playerAnimator.SetTrigger("GameHasStarted");
        canvasAnimator.SetTrigger("GameHasStarted");
    }

    public override void Update()
    {
        ;
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
    Animator playerAnimator;

    float rollAngle = 0f;

    public GameStatePlay(GameController gc) : base(gc)
    {
        playerAnimator = gc.playerAnimator;
        player = gc.player;

        rigidbody = player.GetComponent<Rigidbody2D>();

        playerAnimator.ResetTrigger("GameHasStarted");
    }

    public override void OnFireButtonPressed()
    {
        ;
    }

    public override void OnFireButtonHeld()
    {
        ;
    }

    public override void OnFireButtonUnpressed()
    {
        ;
    }

    public override void Update()
    {
        var multiplier = 1f;

        if (rigidbody.velocity.y < 0)
            multiplier = gameController.playerParams.fallMultiplier;

        //else if (rigidbody.velocity.y > 0 && !Input.GetButton("Fire1"))
        //    multiplier = lowJumpMultiplier;

        rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1) * Time.deltaTime;

        if (Input.GetButtonDown("Fire1")) {
            //rigidbody.AddForce(Vector3.up * 100f);
            rigidbody.velocity = Vector3.up * gameController.playerParams.jumpVelocity;
            playerAnimator.SetTrigger("WingsHaveFlapped");
        }

        else {
            playerAnimator.ResetTrigger("WingsHaveFlapped");
        }

        var frameTransform = frame.GetComponent<Transform>();
        frameTransform.position += Vector3.right * gameController.playerParams.movementVelocity * Time.deltaTime;

        //foreach (var pipe in pipes) {
        //    var tr = pipe.GetComponent<Transform>();
        //    tr.position += Vector3.left * 8f * Time.deltaTime;
        //}
    }

    public override void FixedUpdate()
    {
        player.transform.Rotate(Vector3.forward * rollAngle);
    }
}