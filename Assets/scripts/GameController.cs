using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour {
    public GameObject player;
    public GameObject frame;
    public GameObject platform;

    public GameObject prefabPipes;
    public float pipesOffset = 1f;

    public Vector3 pipesStartPoint = new Vector3(0, 0, 4);

    public Canvas idleStateCanvas;

    public Animator playerAnimator { get; private set; }
    public Animator idleStateCanvasAnimator { get; private set; }

    public PlayerParams playerParams;// { get; private set; }

    private Queue<GameObject> pipes;

    //private int playerScore = 0;

    private GameState gameState = null;

    private InputSystem inputSystem;

    void Start()
    {
        inputSystem = new InputSystem();

        player.AddComponent<Collider2DEventsHandler>();

        playerAnimator = player.GetComponent<Animator>();
        idleStateCanvasAnimator = idleStateCanvas.GetComponent<Animator>();

        pipes = new Queue<GameObject>();

        for (var i = 0; i < 5; ++i)
            pipes.Enqueue(Instantiate(prefabPipes, pipesStartPoint + Vector3.right * pipesOffset * i, Quaternion.identity));

        UpdateGameState();
    }

    void Update()
    {
        // TODO: remove
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene("main", LoadSceneMode.Single);

        inputSystem.Update();

        gameState.Update();
    }

    void FixedUpdate()
    {
        gameState.FixedUpdate();
    }

    public void UpdateGameState()
    {
        Action onFinishGameStateFail = () =>
        {
            ;
        };

        Action onFinishGameStatePlay = () =>
        {
            gameState = new GameStateFail(this, onFinishGameStateFail);
        };

        Action onFinishGameStateIdle = () =>
        {
            gameState = new GameStatePlay(this, onFinishGameStatePlay);
        };

        if (gameState is null) {
            gameState = new GameStateIdle(this, onFinishGameStateIdle);
        }
    }
}
