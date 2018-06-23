using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    private const float BULLET_SPEED = 70f;

    public Main.Side side;

    public Vector3 moveDirection;
    public float range = 2;

    private Vector3 origPosition;

	// Use this for initialization
	void Start () {
        origPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 position = transform.position;
        position += (moveDirection/Vector3.Magnitude(moveDirection)) * (BULLET_SPEED * Time.deltaTime);
        transform.position = position;

        if (Mathf.Abs(Vector3.Distance(origPosition, position)) > range)
            Destroy(gameObject);
	}
}
