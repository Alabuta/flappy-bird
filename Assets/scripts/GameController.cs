using System.Collections;
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

    private Animator playerAnimator;
    private Animator idleStateCanvasAnimator;

    private Queue<GameObject> pipes;

    public float jumpVelocity = 1f;
    public float movementVelocity = 8f;

    public float fallMultiplier = 2.4f;
    public float lowJumpMultiplier = 2f;

    private int playerScore = 0;

    private float rollAngle = 0f;

    private enum GameState {
        IDLE,
        PLAY,
        FAIL,
        SCORE
    }

    private GameState state;

    private delegate void DelegateUpdateOnState();
    DelegateUpdateOnState updateOnState;

    private delegate void FireButtonHandler();
    private event FireButtonHandler FireJustPressed;
    private event FireButtonHandler FireIsHeld;
    private event FireButtonHandler FireJustUnPressed;

    void Start()
    {
        state = GameState.IDLE;

        updateOnState = updateOnIdleState;

        playerAnimator = player.GetComponent<Animator>();
        idleStateCanvasAnimator = idleStateCanvas.GetComponent<Animator>();

        pipes = new Queue<GameObject>();

        for (var i = 0; i < 5; ++i)
            pipes.Enqueue(Instantiate(prefabPipes, pipesStartPoint + Vector3.right * pipesOffset * i, Quaternion.identity));

        FireJustPressed += () => {
            Debug.Log(8888);
        };

        FireIsHeld += () => {
            Debug.Log(4444);
        };

        FireJustUnPressed += () => {
            Debug.Log(2222);
        };
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) {
            FireJustPressed();
        }

        else if (Input.GetButton("Fire1")) {
            FireIsHeld();
        }

        else if (Input.GetButtonUp("Fire1")) {
            FireJustUnPressed();
        }

        updateOnState();
    }

    void updateOnIdleState()
    {
        if (Input.GetButtonDown("Fire1")) {
            state = GameState.PLAY;
            updateOnState = updateOnPlayState;

            playerAnimator.SetTrigger("GameHasStarted");
            idleStateCanvasAnimator.SetTrigger("GameHasStarted");
        }
    }
    void updateOnPlayState()
    {
        playerAnimator.ResetTrigger("GameHasStarted");

        var rigidbody = player.GetComponent<Rigidbody2D>();

        var multiplier = 1f;

        if (rigidbody.velocity.y < 0)
            multiplier = fallMultiplier;

        //else if (rigidbody.velocity.y > 0 && !Input.GetButton("Fire1"))
        //    multiplier = lowJumpMultiplier;

        rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1) * Time.deltaTime;

        if (Input.GetButtonDown("Fire1")) {
            //rigidbody.AddForce(Vector3.up * 100f);
            rigidbody.velocity = Vector3.up * jumpVelocity;
            playerAnimator.SetTrigger("WingsHaveFlapped");
        }

        else {
            playerAnimator.ResetTrigger("WingsHaveFlapped");
        }

        var frameTransform = frame.GetComponent<Transform>();
        frameTransform.position += Vector3.right * movementVelocity * Time.deltaTime;

        //foreach (var pipe in pipes) {
        //    var tr = pipe.GetComponent<Transform>();
        //    tr.position += Vector3.left * 8f * Time.deltaTime;
        //}

        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene("main", LoadSceneMode.Single);
    }

    void FixedUpdate()
    {
        player.transform.Rotate(Vector3.forward * rollAngle);
    }
}
