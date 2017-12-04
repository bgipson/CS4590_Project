using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destination : MonoBehaviour {

    public static Vector3 goal = new Vector3(-1200, 150, 500);

    static float defaultY = goal.y;
    float deviation = 25;
    Vector3 position;

	// Use this for initialization
	void Start () {
        position = goal;
    }
	
	// Update is called once per frame
	void Update () {
        position.y = defaultY + deviation*Mathf.Sin(Time.timeSinceLevelLoad);
        transform.position = position;
    }
}
