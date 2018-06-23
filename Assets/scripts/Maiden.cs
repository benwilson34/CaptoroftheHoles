using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maiden : MonoBehaviour {

    public Main.Team side;

    private void OnTriggerEnter2D(Collider2D collision) {
        Character character = collision.GetComponent<Character>();
        if (character != null && character.team != side && !character.CarryingMaiden) {
            character.SetMaiden(this);
        }
    }

}
