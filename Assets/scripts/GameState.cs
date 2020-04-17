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

        player.GetComponent<Collider2DEventsHandler>().onCollisionEnter2D += OnPlayerCollide;

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

        float angularVelocity = Mathf.Clamp(rigidbody.velocity.y * .01f, -Mathf.PI / 64f, Mathf.PI / 4f);

        if (angularVelocity < 0f) {
            angularVelocity = -Mathf.Pow(Mathf.Abs(angularVelocity), 1.1f);
        }

        rollAngle += angularVelocity;
        rollAngle = Mathf.Clamp(rollAngle, -Mathf.PI / 2f, Mathf.PI / 8f);

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
            if (Time.time - jumpStartTime < .064f)
                rigidbody.AddForce(-Physics2D.gravity * rigidbody.gravityScale * 10f);
        };
    }

    void OnFireUnpressed()
    {
        playerAnimator.ResetTrigger("WingsHaveFlapped");

        rigidbody.AddForce(Physics2D.gravity * rigidbody.gravityScale);

        FixedUpdateFunc = () => { };
    }

    void OnPlayerCollide(Collision2D collision)
    {
        player.GetComponent<Collider2DEventsHandler>().onCollisionEnter2D -= OnPlayerCollide;

        rigidbody.velocity = Vector2.zero;
        rigidbody.AddForce(-Physics2D.gravity * rigidbody.gravityScale * 32f);

        OnFinishAction();
    }
}

public class GameStateFail : GameState {
    Rigidbody2D rigidbody;
    GameObject player;

    float rollAngle = 0f;

    public GameStateFail(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        //canvasAnimator = gc.idleStateCanvasAnimator;

        gc.playerAnimator.enabled = false;

        player = gc.player;

        rigidbody = player.GetComponent<Rigidbody2D>();

        rigidbody.velocity = Vector2.zero;
        player.transform.Rotate(Vector2.zero);
    }

    public override void Update()
    {
        ;
    }

    public override void FixedUpdate()
    {
        float angularVelocity = Mathf.Clamp(rigidbody.velocity.y * .01f, -Mathf.PI / 32f, Mathf.PI / 4f);

        if (angularVelocity < 0f) {
            angularVelocity = -Mathf.Pow(Mathf.Abs(angularVelocity), 1.1f);
        }

        rollAngle += angularVelocity;
        rollAngle = Mathf.Clamp(rollAngle, -Mathf.PI / 2f, Mathf.PI / 8f);

        player.transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * rollAngle, Vector3.forward);
    }
}
