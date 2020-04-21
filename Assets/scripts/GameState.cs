using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

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

    System.Random randomGenerator;

    float traveledDistance = 0f;

    float playScore = 0f;


    public GameStatePlay(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        playerAnimator = gc.playerAnimator;
        player = gc.player;

        rigidbody = gc.player.GetComponent<Rigidbody2D>();
        rigidbody.simulated = true;

        player.GetComponent<Collider2DEventsHandler>().onCollisionEnter2D += OnPlayerCollisionEnter;
        player.GetComponent<Collider2DEventsHandler>().onTriggerExit2D += OnPlayerCollisionExit;

        playerAnimator.ResetTrigger("GameHasStarted");

        inputSystem.AddInputHandler("Fire1",
            OnFirePressed,
            OnFireHeld,
            OnFireUnpressed
        );

        gc.platform.GetComponent<UVScroller>().velocity = new Vector2(movementVelocity, 0f);

        rigidbody.AddForce(-Physics2D.gravity * rigidbody.gravityScale * playerParams.jumpForceScale);

        FixedUpdateFunc = () => { };
    }

    public override void Update()
    {
        inputSystem.Update();

        foreach (var pipe in gameController.pipes) {
            var tr = pipe.GetComponent<Transform>();
            tr.position += Vector3.left * movementVelocity * Time.deltaTime;
        }

        var leftPipeGroup = gameController.pipes.Peek();
        var leftPipeBounds = leftPipeGroup.GetComponentInChildren<Collider2D>().bounds;

        var cam = Camera.main;
        var planes = GeometryUtility.CalculateFrustumPlanes(cam);

        bool visible = GeometryUtility.TestPlanesAABB(planes, leftPipeBounds);

        if (!visible && leftPipeGroup.transform.position.x < 0f) {
            leftPipeGroup = gameController.pipes.Dequeue();

            var pipesParams = gameController.pipesParams;

            var gap = Mathf.Max(pipesParams.pipesVerticalGapMin, UnityEngine.Random.value * pipesParams.pipesVerticalGapMax);

            foreach (var transform in leftPipeGroup.GetComponentsInChildren<Transform>())
                transform.localPosition = new Vector3(0f, gap * Mathf.Sign(transform.localPosition.y), transform.localPosition.z);

            var tr = leftPipeGroup.GetComponent<Transform>();
            tr.position += Vector3.right * pipesParams.offset * (gameController.pipesParams.number - 1f);
            tr.position += Vector3.Scale(pipesParams.randomOffset, UnityEngine.Random.insideUnitSphere);

            gameController.pipes.Enqueue(leftPipeGroup);
        }

        traveledDistance += (Vector3.left * movementVelocity * Time.deltaTime).magnitude;

        if (traveledDistance > 4f)
            gameController.idleStateCanvas.transform.position += Vector3.left * movementVelocity * Time.deltaTime;
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

    void OnPlayerCollisionEnter(Collision2D collision)
    {
        player.GetComponent<Collider2DEventsHandler>().onCollisionEnter2D -= OnPlayerCollisionEnter;
        player.GetComponent<Collider2DEventsHandler>().onTriggerExit2D -= OnPlayerCollisionExit;

        rigidbody.velocity = Vector2.zero;
        rigidbody.AddForce(-Physics2D.gravity * rigidbody.gravityScale * playerParams.deadJumpForceScale);

        OnFinishAction();
    }

    void OnPlayerCollisionExit(Collider2D collision)
    {
        if (collision.gameObject.tag == "PipesGap") {
            var scoreText = gameController.playStateCanvas.transform.Find("score-text");
            scoreText.GetComponent<Text>().text = (++playScore).ToString();
        }
    }
}

public class GameStateFail : GameState {
    GameObject player;
    Rigidbody2D rigidbody;
    UVScroller uvScroller;

    float rollAngle;
    float startTime;
    float originalMovementVelocity;

    public GameStateFail(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        //canvasAnimator = gc.idleStateCanvasAnimator;

        gc.playerAnimator.enabled = false;

        player = gc.player;

        player.GetComponent<Collider2D>().isTrigger = true;

        rollAngle = 0f;

        rigidbody = player.GetComponent<Rigidbody2D>();
        rigidbody.velocity = Vector2.zero;

        player.transform.Rotate(Vector2.zero);

        startTime = Time.time;

        uvScroller = gc.platform.GetComponent<UVScroller>();

        originalMovementVelocity = movementVelocity;
    }

    public override void Update()
    {
        movementVelocity = Mathf.Lerp(originalMovementVelocity, 0, Time.time - startTime);
        uvScroller.velocity = new Vector2(movementVelocity, 0f);

        foreach (var pipe in gameController.pipes) {
            var tr = pipe.GetComponent<Transform>();
            tr.position += Vector3.left * movementVelocity * Time.deltaTime;
        }

        gameController.idleStateCanvas.transform.position += Vector3.left * movementVelocity * Time.deltaTime;
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
    }
}
