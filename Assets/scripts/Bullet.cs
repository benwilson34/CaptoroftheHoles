using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    private const float BULLET_VEL = 10f;

    private Main.Team _team;
    //public Vector3 moveDirection;
    //public float range = 2;

    //private Rigidbody2D rb;

	// Use this for initialization
	void Start () {
        //rb = GetComponent<Rigidbody2D>();
	}

    public void Init(Main.Team team, float angle) {
        _team = team;
        GetComponent<SpriteRenderer>().color =
            team == Main.Team.Red ? Color.red : Color.blue;

        Debug.Log(angle);
        float rad = Mathf.Deg2Rad * angle;
        Vector2 units = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        GetComponent<Rigidbody2D>().velocity = units * BULLET_VEL;
        StartCoroutine(DestroyAfterTime());
    }

    // Update is called once per frame
    void Update() {
        //Vector3 position = transform.position;
        //position += (moveDirection/Vector3.Magnitude(moveDirection)) * (BULLET_SPEED * Time.deltaTime);
        //transform.position = position;

        //if (Mathf.Abs(Vector3.Distance(origPosition, position)) > range)
        //    Destroy(gameObject);

    }

    IEnumerator DestroyAfterTime() {
        const float aliveTime = 2; // 2 second lifetime
        yield return new WaitForSeconds(aliveTime);
        Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        var character = collision.GetComponent<Character>();
        if (character != null) {
            if (character.team != _team) {
                character.Die();
                Destroy(gameObject);
            }
        } else {
            Destroy(gameObject);
        }
    }
}
