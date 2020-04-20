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
    public PipesParams pipesParams;

    public Queue<GameObject> pipes;

    private GameState gameState = null;

    private InputSystem inputSystem;

    void Start()
    {
        inputSystem = new InputSystem();

        player.AddComponent<Collider2DEventsHandler>();

        playerAnimator = player.GetComponent<Animator>();
        idleStateCanvasAnimator = idleStateCanvas.GetComponent<Animator>();

        pipes = new Queue<GameObject>();

        for (var i = 0; i < 5; ++i) {
            var position = pipesParams.startPoint + Vector3.right * pipesParams.offset * i;
            var offset = Vector3.Scale(pipesParams.randomOffset, UnityEngine.Random.insideUnitSphere);

            var gap = Mathf.Max(pipesParams.pipesVerticalGapMin, UnityEngine.Random.value * pipesParams.pipesVerticalGapMax);

            foreach (var transform in prefabPipes.GetComponentsInChildren<Transform>())
                transform.localPosition = new Vector3(0f, gap * Mathf.Sign(transform.localPosition.y), transform.localPosition.z);

            var pipe = Instantiate(prefabPipes, position + offset, Quaternion.identity);

            pipes.Enqueue(pipe);
        }

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

    void UpdateGameState()
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
