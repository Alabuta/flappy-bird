using System;

using UnityEngine;
using UnityEngine.UI;

public abstract class GameState {
    protected GameController gameController;

    protected InputSystem inputSystem;

    protected Action OnFinishAction;

    protected GameObject player;

    protected readonly PlayerParams playerParams;

    public float movementVelocity { get; protected set; }

    public GameState(GameController gc, Action onFinishAction)
    {
        OnFinishAction = onFinishAction;

        inputSystem = new InputSystem();

        gameController = gc;
        player = gc.player;
        playerParams = gc.playerParams;
        movementVelocity = playerParams.movementVelocity;
    }

    public abstract void Update();
    public abstract void FixedUpdate();
}

public class GameStateIdle : GameState {
    public GameStateIdle(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        gc.playStateCanvas.SetActive(true);
        gc.failStateCanvas.SetActive(false);

        var rigidbody = player.GetComponent<Rigidbody2D>();
        rigidbody.simulated = false;

        gc.platform.GetComponent<UVScroller>().velocity = new Vector2(movementVelocity, 0f);

        inputSystem.AddInputHandler("Fire1",
            () => {
                player.GetComponent<Animator>().SetTrigger("GameHasStarted");
                gc.idleStateCanvas.GetComponent<Animator>().SetTrigger("GameHasStarted");

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
        playerTransform.position -= Vector3.up * Mathf.Sin(2f * Mathf.PI * Time.time / .58f) * .064f;
    }
}

public class GameStatePlay : GameState {
    Rigidbody2D playerRigidbody;

    float rollAngle = 0f;
    float jumpStartTime = 0f;
    float traveledDistance = 0f;
    float playScore = 0f;

    delegate void FixedUpdateDelegate();
    FixedUpdateDelegate FixedUpdateFunc;


    public GameStatePlay(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        playerRigidbody = player.GetComponent<Rigidbody2D>();
        playerRigidbody.simulated = true;

        player.GetComponent<Collider2DEventsHandler>().onCollisionEnter2D += OnPlayerCollisionEnter;
        player.GetComponent<Collider2DEventsHandler>().onTriggerExit2D += OnPlayerCollisionExit;

        inputSystem.AddInputHandler("Fire1",
            OnFirePressed,
            OnFireHeld,
            OnFireUnpressed
        );

        gc.platform.GetComponent<UVScroller>().velocity = new Vector2(movementVelocity, 0f);

        playerRigidbody.AddForce(-Physics2D.gravity * playerRigidbody.gravityScale * playerParams.jumpForceScale);

        player.GetComponent<Animator>().ResetTrigger("GameHasStarted");

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

        var planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

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
            playerRigidbody.velocity.y * playerParams.angularVelocityScaler,
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
        player.GetComponent<Animator>().SetTrigger("WingsHaveFlapped");

        playerRigidbody.velocity = Vector2.zero;
        player.transform.Rotate(Vector2.zero);

        jumpStartTime = Time.time;

        playerRigidbody.velocity = Vector2.zero;

        FixedUpdateFunc = () => { };
    }

    void OnFireHeld()
    {
        FixedUpdateFunc = () =>
        {
            if (Time.time - jumpStartTime < .072f)
                playerRigidbody.AddForce(-Physics2D.gravity * playerRigidbody.gravityScale * playerParams.jumpForceScale);
        };
    }

    void OnFireUnpressed()
    {
        player.GetComponent<Animator>().ResetTrigger("WingsHaveFlapped");

        playerRigidbody.AddForce(Physics2D.gravity * playerRigidbody.gravityScale);

        FixedUpdateFunc = () => { };
    }

    void OnPlayerCollisionEnter(Collision2D collision)
    {
        player.GetComponent<Collider2DEventsHandler>().onCollisionEnter2D -= OnPlayerCollisionEnter;
        player.GetComponent<Collider2DEventsHandler>().onTriggerExit2D -= OnPlayerCollisionExit;

        playerRigidbody.velocity = Vector2.zero;
        playerRigidbody.AddForce(-Physics2D.gravity * playerRigidbody.gravityScale * playerParams.deadJumpForceScale);

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
    Rigidbody2D playerRigidbody;
    UVScroller uvScroller;

    float rollAngle;
    float startTime;
    float originalMovementVelocity;

    public GameStateFail(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        player.GetComponent<Animator>().enabled = false;
        player.GetComponent<Collider2D>().isTrigger = true;

        rollAngle = 0f;

        playerRigidbody = player.GetComponent<Rigidbody2D>();
        playerRigidbody.velocity = Vector2.zero;

        player.transform.Rotate(Vector2.zero);

        startTime = Time.time;

        uvScroller = gc.platform.GetComponent<UVScroller>();

        originalMovementVelocity = movementVelocity;

        gc.playStateCanvas.GetComponent<Animator>().SetTrigger("GameFailed");

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

        if (movementVelocity < 1e-3f) {
            gameController.playStateCanvas.SetActive(false);
            OnFinishAction();
        }
    }

    public override void FixedUpdate()
    {
        float angularVelocity = Mathf.Clamp(
            playerRigidbody.velocity.y * playerParams.angularVelocityScaler,
            playerParams.minAngularVelocity * 2.4f, playerParams.maxAngularVelocity
        );

        if (angularVelocity < 0f) {
            angularVelocity = -Mathf.Pow(Mathf.Abs(angularVelocity), playerParams.negativeAngularVelocityScaler);
        }

        rollAngle += angularVelocity;
        rollAngle = Mathf.Clamp(rollAngle, playerParams.minRollAngle, playerParams.maxRollAngle);

        player.transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * rollAngle, Vector3.forward);
    }
}

public class GameStateResult : GameState {
    public GameStateResult(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        gc.failStateCanvas.SetActive(true);

        var scoreText = gc.failStateCanvas.transform.Find("score").transform.Find("score-text");
        scoreText.GetComponent<Text>().text = (0123456789).ToString();
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
