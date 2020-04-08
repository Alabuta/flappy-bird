using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;


public abstract class GameState {
    protected GameController gameController;
    protected InputSystem inputSystem;

    public GameState(GameController gc)
    {
        gameController = gc;

        inputSystem = new InputSystem();
    }

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

        inputSystem.AddInputHandler("Fire1",
            () => {
                playerAnimator.SetTrigger("GameHasStarted");
                canvasAnimator.SetTrigger("GameHasStarted");

                //UpdateGameState();
            },
            () => { },
            () => { }
        );
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

        inputSystem.AddInputHandler("Fire1",
            OnFirePressed,
            () => { },
            OnFireUnpressed
        );

        playerAnimator.ResetTrigger("GameHasStarted");
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