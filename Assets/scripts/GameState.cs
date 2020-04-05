using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public abstract class GameState {
    protected GameController gameController;

    public GameState(GameController gc) => gameController = gc;

    public abstract void OnFireButtonPressed();
    public abstract void OnFireButtonHeld();
    public abstract void OnFireButtonUnpressed();

    public abstract void Update();
}

public class GameStateIdle : GameState {
    Animator canvasAnimator;

    GameStateIdle(GameController gc) : base(gc) {
        ;
    }

    public override void OnFireButtonPressed()
    {
        ;
    }

    public override void OnFireButtonHeld()
    {
        ;
    }

    public override void OnFireButtonUnpressed()
    {
        ;
    }

    public override void Update()
    {
        gameController.playerAnimator.SetTrigger("GameHasStarted");
        gameController.idleStateCanvasAnimator.SetTrigger("GameHasStarted");
    }
}

public class GameStatePlay : GameState {
    GameStatePlay(GameController gc) : base(gc) { }

    public override void OnFireButtonPressed()
    {
        ;
    }

    public override void OnFireButtonHeld()
    {
        ;
    }

    public override void OnFireButtonUnpressed()
    {
        ;
    }

    public override void Update()
    {
        ;
    }
}