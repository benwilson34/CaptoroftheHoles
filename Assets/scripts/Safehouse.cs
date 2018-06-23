using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Safehouse : MonoBehaviour {

    public GameObject playerPF, maidenPF;
    public Main.Side side = Main.Side.Left;

    private Transform spawnPoint;

    public void Init() {
        spawnPoint = transform.Find("spawnPoint");
        Transform maidenSpawns = transform.Find("maidenSpawns");
        foreach (Transform spawn in maidenSpawns) { // spawn maidens
            var maiden = Instantiate(maidenPF).GetComponent<Maiden>();
            maiden.side = side;
            maiden.transform.position = spawn.position;
        }

        SpawnPlayer();
    }

    public void SpawnPlayer() {
        var player = Instantiate(playerPF);
        player.GetComponent<Character>().screenSide = side;
        player.transform.position = spawnPoint.position;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Character character = collision.GetComponent<Character>();
        if (character != null && character.CarryingMaiden) {
            Main.ScorePoint(side);
            character.ClearMaiden();
        }
    }
}
