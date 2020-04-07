﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour {
    public GameObject player;
    public GameObject frame;

    public GameObject prefabPipes;
    public float pipesOffset = 1f;

    public Vector3 pipesStartPoint = new Vector3(0, 0, 4);

    public Canvas idleStateCanvas;

    public Animator playerAnimator { get; private set; }
    public Animator idleStateCanvasAnimator { get; private set; }

    public PlayerParams playerParams;// { get; private set; }

    private Queue<GameObject> pipes;

    private int playerScore = 0;

    private GameState gameState = null;

    private delegate void FireButtonHandler();
    private event FireButtonHandler FireJustPressed;
    private event FireButtonHandler FireIsHeld;
    private event FireButtonHandler FireJustUnPressed;

    private InputSystem inputSystem;

    void Start()
    {
        inputSystem = new InputSystem();

        playerAnimator = player.GetComponent<Animator>();
        idleStateCanvasAnimator = idleStateCanvas.GetComponent<Animator>();

        pipes = new Queue<GameObject>();

        for (var i = 0; i < 5; ++i)
            pipes.Enqueue(Instantiate(prefabPipes, pipesStartPoint + Vector3.right * pipesOffset * i, Quaternion.identity));

        UpdateGameState();
    }

    void Update()
    {
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
        if (gameState is null) {
            gameState = new GameStateIdle(this);
        }

        else if (gameState is GameStateIdle) {
            gameState = new GameStatePlay(this);
        }

        FireJustPressed += gameState.OnFireButtonPressed;
        FireIsHeld += gameState.OnFireButtonHeld;
        FireJustUnPressed += gameState.OnFireButtonUnpressed;
    }
}
