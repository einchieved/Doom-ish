using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    public float speed = 100f;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * speed, ForceMode.Impulse);
    }

    // Called once per frame
    private void Update()
    {
        if (Mathf.Abs(transform.position.x) > 200 || Mathf.Abs(transform.position.y) > 200 || Mathf.Abs(transform.position.z) > 200)
        {
            Destroy(gameObject);
        }
    }
}
