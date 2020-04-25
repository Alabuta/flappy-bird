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

    [HideInInspector]
    public float movementVelocity;

    [HideInInspector]
    public float gameStartTime;

    [HideInInspector]
    public float playScore;

    [HideInInspector]
    public float bestScore;

    void Start()
    {
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

        InitGameStates();
    }

    void Update()
    {
        gameState.Update();

        platform.GetComponent<UVScroller>().velocity = new Vector2(movementVelocity, 0f);

        if (gameStartTime > 0f && (Time.time - gameStartTime) > .64f)
            idleStateCanvas.transform.position += Vector3.left * movementVelocity * Time.deltaTime;
    }

    void FixedUpdate()
    {
        gameState.FixedUpdate();
    }

    void InitGameStates()
    {
        Action onFinishGameStateFail = () =>
        {
            movementVelocity = 0f;

            bestScore = Math.Max(playScore, PlayerPrefs.GetFloat("BestScore", 0f));
            PlayerPrefs.SetFloat("BestScore", bestScore);

            gameState = new GameStateResult(this, () => { });
        };

        Action onFinishGameStatePlay = () =>
        {
            gameState = new GameStateFail(this, onFinishGameStateFail);
        };

        Action onFinishGameStateIdle = () =>
        {
            gameStartTime = Time.time;

            gameState = new GameStatePlay(this, onFinishGameStatePlay);
        };

        if (gameState is null) {
            gameState = new GameStateIdle(this, onFinishGameStateIdle);
        }
    }
    public void UpdatePipes()
    {
        foreach (var pipe in pipes) {
            var tr = pipe.GetComponent<Transform>();
            tr.position += Vector3.left * movementVelocity * Time.deltaTime;
        }

        var leftPipeGroup = pipes.Peek();
        var leftPipeBounds = leftPipeGroup.GetComponentInChildren<Collider2D>().bounds;

        var planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        bool visible = GeometryUtility.TestPlanesAABB(planes, leftPipeBounds);

        if (!visible && leftPipeGroup.transform.position.x < 0f) {
            leftPipeGroup = pipes.Dequeue();

            var gap = Mathf.Max(pipesParams.pipesVerticalGapMin, UnityEngine.Random.value * pipesParams.pipesVerticalGapMax);

            foreach (var transform in leftPipeGroup.GetComponentsInChildren<Transform>())
                transform.localPosition = new Vector3(0f, gap * Mathf.Sign(transform.localPosition.y), transform.localPosition.z);

            var tr = leftPipeGroup.GetComponent<Transform>();
            tr.position += Vector3.right * pipesParams.offset * (pipesParams.number - 1f);
            tr.position += Vector3.Scale(pipesParams.randomOffset, UnityEngine.Random.insideUnitSphere);

            pipes.Enqueue(leftPipeGroup);
        }
    }

    public void UpdatePlayer()
    {
        var playerRigidbody = player.GetComponent<Rigidbody2D>();

        float angularVelocity = Mathf.Clamp(
            playerRigidbody.velocity.y * playerParams.angularVelocityScaler,
            playerParams.minAngularVelocity, playerParams.maxAngularVelocity
        );

        if (angularVelocity < 0f) {
            angularVelocity = -Mathf.Pow(Mathf.Abs(angularVelocity), playerParams.negativeAngularVelocityScaler);
        }

        player.transform.rotation.ToAngleAxis(out float rollAngle, out Vector3 axis);

        rollAngle *= Mathf.Sign(axis.z);
        rollAngle += angularVelocity;
        rollAngle = Mathf.Clamp(rollAngle, playerParams.minRollAngle, playerParams.maxRollAngle);

        player.transform.rotation = Quaternion.AngleAxis(rollAngle, Vector3.forward);
    }
}
