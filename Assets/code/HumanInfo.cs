using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HumanInfo : MessageBase {

    // Warning: any default values will override actual values during a message
    public short ID;
    public float xPos;
    public float yPos;
    public bool initialized = false;
    public string message;

    public HumanInfo()
    {
        // empty constructor must exist to send client -> server
    }

    public HumanInfo(short idIn)
    {
        ID = idIn;
    }

    public short getID() { return ID; }

    public void setID(short IDIn)
    {
        ID = IDIn;
    }

    public float getX() { return xPos; }

    public void setX(float xIn)
    {
        xPos = xIn;
    }

    public float getY() { return yPos; }

    public void setY(float yIn)
    {
        yPos = yIn;
    }  

    public string getMessage() { return message; }

    public void setMessage(string messageIn)
    {
        message = messageIn;
    }

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
