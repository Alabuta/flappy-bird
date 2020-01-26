using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class controller : MonoBehaviour
{
    public float jumpForce = 1f;
    public float jumpThreshold = 10f; // in pixels
    public float speed = 10f;

    Vector3 prevDirection = new Vector3(0f, 0f, 0f);

    bool hasContact = false;
    bool directionHasChanged = false;
    bool firstContact = false;

    public GameObject player;
    public Camera mainCamera;

    Vector3 offset = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        ;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetButton("Fire1")) {
            var transform = player.GetComponent<Transform>();

            var screenSpacePosition = mainCamera.WorldToScreenPoint(transform.position);
            var cameraTransform = mainCamera.GetComponent<Transform>();

            var newDirection = Input.mousePosition - screenSpacePosition;

            if (hasContact && newDirection.y > jumpThreshold) {
                var rigidbody = player.GetComponent<Rigidbody2D>();
                rigidbody.AddForce(new Vector3(0f, jumpForce, 0f));

                hasContact = false;
            }

            newDirection.y = 0f;
            newDirection = Vector3.Normalize(newDirection);

            directionHasChanged = Vector3.Distance(prevDirection, newDirection) > 0f;

            prevDirection = newDirection;

            transform.position += newDirection * speed * Time.fixedDeltaTime;

            //transform.localRotation = direction.x >= 0f ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 180, 0f);

            if (directionHasChanged) {
                if (newDirection.x < 0f) {
                    transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                }

                else {
                    transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                }
            }
        }

        if (firstContact)
        {
            var transform = player.GetComponent<Transform>();
            var cameraTransform = mainCamera.GetComponent<Transform>();

            var newPostion = transform.position - offset;
            newPostion.Set(newPostion.x, Mathf.Clamp(cameraTransform.position.y, 0f, .4f), newPostion.z);

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, newPostion, .04f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("r")) {
            SceneManager.LoadScene("demo", LoadSceneMode.Single);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!firstContact)
        {
            var transform = player.GetComponent<Transform>();
            var cameraTransform = mainCamera.GetComponent<Transform>();

            offset = transform.position - cameraTransform.position;
            firstContact = true;
        }
        /*if (collision.gameObject.tag == "Enemy") {
            collision.gameObject.SendMessage("ApplyDamage", 10);
        }*/
        //Debug.Log(collision.contacts);
        hasContact = true;
    }
}
