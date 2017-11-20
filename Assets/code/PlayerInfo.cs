using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerInfo : MessageBase {

    public enum Type
    {
        NONE,
        HUMAN,
        ZOMBIE
    };

    // Warning: any default values will override actual values during a message
    // any variables available to the server must be public, won't serialize otherwise
    public short ID;
    public Vector3 position;
    private bool initialized = false; // not needed for server, can have default value
    public string message;
    public float timeStamp; // only really used for server
    public Type playerType;

    public PlayerInfo()
    {
        // empty constructor must exist to send client -> server
    }

    public PlayerInfo(short idIn)
    {
        ID = idIn;
    }

    public void Init()
    {
        position = new Vector3();
        playerType = Type.NONE;
        timeStamp = 0;
        message = "";
    }

    public short getID() { return ID; }

    public void setID(short IDIn)
    {
        ID = IDIn;
    }

    public Vector3 getPosition() { return position; }

    public void setPosition( Vector3 pIn)
    {
        position.x = pIn.x;
        position.y = pIn.y;
        position.z = pIn.z;
    }

    public bool isInitialized()
    {
        return initialized;
    }

    public void setInitialized(bool initializedIn)
    {
        initialized = initializedIn;
    }

    public string getMessage() { return message; }

    public void setMessage(string messageIn)
    {
        message = messageIn;
    }

    public float getTimeStamp() { return timeStamp; }

    public void setTimeStamp(float timeStampIn)
    {
        timeStamp = timeStampIn;
    }

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
