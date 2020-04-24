using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour {
    public GameObject player;
    public GameObject frame;
    public GameObject platform;

    public GameObject prefabPipes;

    public GameObject idleStateCanvas;
    public GameObject playStateCanvas;
    public GameObject failStateCanvas;

    public PlayerParams playerParams;
    public PipesParams pipesParams;

    public Queue<GameObject> pipes;

    private GameState gameState;

    private InputSystem inputSystem;

    void Start()
    {
        inputSystem = new InputSystem();

        player.AddComponent<Collider2DEventsHandler>();

        failStateCanvas.SetActive(false);

        pipes = new Queue<GameObject>();

        for (var i = 0; i < 5; ++i) {
            var position = pipesParams.startPoint + Vector3.right * pipesParams.offset * i;
            var offset = Vector3.Scale(pipesParams.randomOffset, UnityEngine.Random.insideUnitSphere);

            var gap = Mathf.Max(pipesParams.pipesVerticalGapMin, UnityEngine.Random.value * pipesParams.pipesVerticalGapMax);

            var pipe = Instantiate(prefabPipes, Vector3.zero, Quaternion.identity);

            position += offset;

            foreach (var transform in pipe.GetComponentsInChildren<Transform>())
                transform.position = position + Vector3.up * gap * Mathf.Sign(transform.localPosition.y);

            var colliderGameObject = new GameObject("collider");
            colliderGameObject.tag = "PipesGap";

            var collider = colliderGameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, gap * 2f);
            collider.isTrigger = true;

            colliderGameObject.transform.position = position;
            colliderGameObject.transform.parent = pipe.transform;

            pipes.Enqueue(pipe);
        }

        var restartButton = failStateCanvas.transform.Find("restart-button");
        restartButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.LoadScene("main", LoadSceneMode.Single);
        });

        UpdateGameState();
    }

    void Update()
    {
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
            gameState = new GameStateResult(this, () => { });
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
