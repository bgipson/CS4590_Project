using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Standard Message. No specific Fields
//We can create new ones if necessary
//ID would be used to identify the message originator
//TYPE would be used to identify the message purpose/contents
public class NetworkMess : MessageBase {

    public string messageContents;
    public int id;
    public short messageType;
    public Vector3 position;
    public Vector3 rotation;
    public string name;
}
