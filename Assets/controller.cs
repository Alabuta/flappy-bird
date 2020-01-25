using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class controller : MonoBehaviour
{
    public float jumpForce = 1f;
    public float speed = 10.1f;
    Vector3 direction = new Vector3(0f, 0f, 0f);

    bool hasContact = false;

    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetButton("Fire1")) {
            var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            if (hasContact && Input.mousePosition.y > screenCenter.y) {
                var rigidbody = player.GetComponent<Rigidbody2D>();
                rigidbody.AddForce(new Vector3(0f, jumpForce, 0f));

                hasContact = false;
            }

            direction.x = Input.mousePosition.x > screenCenter.x ? 1f : -1f;

            var transform = player.GetComponent<Transform>();

            transform.position += direction * speed * Time.fixedDeltaTime;

            //transform.localRotation.y = direction.x >= 0f ? 0f : Mathf.PI;
            transform.localRotation = direction.x >= 0f ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 180, 0f);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        /*if (collision.gameObject.tag == "Enemy") {
            collision.gameObject.SendMessage("ApplyDamage", 10);
        }*/
        //Debug.Log(collision.contacts);
        Debug.Log(4444);
        hasContact = true;
    }
}
