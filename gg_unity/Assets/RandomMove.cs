using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMove : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    private Camera _camera;

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 viewportPosition = _camera.WorldToViewportPoint(transform.position);
        Vector3 direction = _rigidbody2D.velocity;

        // If object hits viewport boundaries, reverse direction
        if (viewportPosition.x <= 0f || viewportPosition.x >= 1f)
        {
            direction.x = -direction.x;
        }

        if (viewportPosition.y <= 0f || viewportPosition.y >= 1f)
        {
            direction.y = -direction.y;
        }

        // If object has no velocity, give it an initial random direction
        if (direction.magnitude < 0.1f)
        {
            direction = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ).normalized * 1f;
        }

        _rigidbody2D.velocity = direction;
    }
}
