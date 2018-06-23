using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maiden : MonoBehaviour {

    public Main.Side side;

    private void OnTriggerEnter2D(Collider2D collision) {
        Character character = collision.GetComponent<Character>();
        if (character != null && character.screenSide != side && !character.CarryingMaiden) {
            character.SetMaiden(this);
        }
    }

}
