using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {

    public Transform matchingPortal;

    void OnTriggerEnter2D(Collider2D collision) {
        Character character = collision.GetComponent<Character>();
        if (character != null && character.CanTeleport) {
            character.Teleported();
            character.transform.position = matchingPortal.position;
        }
    }
}
