using System;

using UnityEngine;
using UnityEngine.UI;

public abstract class GameState {
    protected GameController gameController;

    protected InputSystem inputSystem;

    protected GameObject player;

    protected readonly Action OnFinishAction;

    protected readonly PlayerParams playerParams;

    protected readonly float stateStartTime;

    public float gameStartTime {
        get {
            return gameController.gameStartTime;
        }
    }

    public float movementVelocity {
        get {
            return gameController.movementVelocity;
        }

        protected set {
            gameController.movementVelocity = value;
        }
    }

    public float playScore {
        get {
            return gameController.playScore;
        }

        protected set {
            gameController.playScore = value;
        }
    }

    public float bestScore {
        get {
            return gameController.bestScore;
        }
    }

    public GameState(GameController gc, Action onFinishAction)
    {
        gameController = gc;

        inputSystem = new InputSystem();

        OnFinishAction = onFinishAction;

        player = gc.player;
        playerParams = gc.playerParams;

        movementVelocity = playerParams.movementVelocity;

        stateStartTime = Time.time;
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
    readonly Rigidbody2D playerRigidbody;

    float jumpStartTime = 0f;

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

        playerRigidbody.AddForce(-Physics2D.gravity * playerRigidbody.gravityScale * playerParams.jumpForceScale);

        player.GetComponent<Animator>().ResetTrigger("GameHasStarted");

        FixedUpdateFunc = () => { };
    }

    public override void Update()
    {
        inputSystem.Update();

        gameController.UpdatePipes();
    }

    public override void FixedUpdate()
    {
        FixedUpdateFunc();

        gameController.UpdatePlayer();
    }

    void OnFirePressed()
    {
        player.GetComponent<Animator>().SetTrigger("WingsHaveFlapped");

        playerRigidbody.velocity = Vector2.zero;
        //player.transform.rotation = Quaternion.identity;

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

    public GameStateFail(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        player.GetComponent<Animator>().enabled = false;
        player.GetComponent<Collider2D>().isTrigger = true;

        playerRigidbody = player.GetComponent<Rigidbody2D>();
        playerRigidbody.velocity = Vector2.zero;

        player.transform.rotation = Quaternion.identity;

        gc.playStateCanvas.GetComponent<Animator>().SetTrigger("GameFailed");

    }

    public override void Update()
    {
        movementVelocity = Mathf.Lerp(playerParams.movementVelocity, 0, Time.time - stateStartTime);

        gameController.UpdatePipes();

        if (movementVelocity < 1e-3f) {
            gameController.playStateCanvas.SetActive(false);
            OnFinishAction();
        }
    }

    public override void FixedUpdate()
    {
        gameController.UpdatePlayer();
    }
}

public class GameStateResult : GameState {
    public GameStateResult(GameController gc, Action onFinishAction) : base(gc, onFinishAction)
    {
        movementVelocity = 0f;

        gc.failStateCanvas.SetActive(true);

        var score = gc.failStateCanvas.transform.Find("score");

        {
            var scoreText = score.transform.Find("score-text");
            scoreText.GetComponent<Text>().text = playScore.ToString();
        }

        {
            var scoreText = score.transform.Find("best-score-text");
            scoreText.GetComponent<Text>().text = PlayerPrefs.GetFloat("BestScore", playScore).ToString();
        }
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
