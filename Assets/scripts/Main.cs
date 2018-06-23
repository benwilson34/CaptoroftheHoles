using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    public enum Side { Left, Right };
    public Transform levelObjects;
    public Transform middleWall;

    public static int leftScore = 0, rightScore = 0;

    private Safehouse leftPlayerSafehouse, rightPlayerSafehouse;

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
                leftPlayerSafehouse = obj.GetComponent<Safehouse>();
                leftPlayerSafehouse.Init();
                rightPlayerSafehouse = mirrorObj.GetComponent<Safehouse>();
                rightPlayerSafehouse.side = Side.Right;
                rightPlayerSafehouse.Init();
            }
        }


    }

    // Update is called once per frame
    void Update() {

    }

    public static void ScorePoint(Side side) {
        if (side == Side.Left) {
            leftScore++;
        } else {
            rightScore++;
        }
    }
}
