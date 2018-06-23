using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    public enum Team { Red, Blue };
    public Transform levelObjects;
    public Transform middleWall;

    public static int redScore = 0, blueScore = 0;

    private static Safehouse redPlayerSafehouse, bluePlayerSafehouse;

    // Use this for initialization
    void Start() {
        MirrorLevelObjects();

    }

    void MirrorLevelObjects() {
        float midpoint = middleWall.position.x;
        Transform mirrors = new GameObject().transform;
        mirrors.gameObject.name = "mirrorPlatforms";

        foreach (Transform obj in levelObjects) {
            float newX = midpoint - obj.position.x;
            GameObject mirrorObj = Instantiate(obj.gameObject, mirrors);
            mirrorObj.transform.position = new Vector3(newX, obj.position.y);

            if (obj.GetComponent<Portal>() != null) {
                obj.GetComponent<Portal>().matchingPortal = mirrorObj.transform;
                mirrorObj.GetComponent<Portal>().matchingPortal = obj.transform;
            }

            if (obj.GetComponent<Safehouse>() != null) {
                redPlayerSafehouse = obj.GetComponent<Safehouse>();
                redPlayerSafehouse.Init();
                bluePlayerSafehouse = mirrorObj.GetComponent<Safehouse>();
                bluePlayerSafehouse.side = Team.Blue;
                bluePlayerSafehouse.Init();
            }
        }


    }

    // Update is called once per frame
    void Update() {

    }

    public static void ScorePoint(Team team) {
        if (team == Team.Red) {
            redScore++;
        } else {
            blueScore++;
        }
    }

    public static Safehouse GetSafehouse(Team team) {
        return team == Team.Red ? redPlayerSafehouse : bluePlayerSafehouse;
    }
}
