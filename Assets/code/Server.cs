using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Server : MonoBehaviour {

    short SHRT_MAX = 32767;
    short MSG_NEW_HUMAN = 1000;
    short MSG_POSITION = 100;

    public Text serverText;
    public Text humansTotal;
    public InputField yField;
    public InputField xField;
    public int port = 1234;

    List<HumanInfo> humans;

    public Server()
    {
        // avoid:
        // called 2x in scene start
        // called 0x times when started from menu
    }

    // Use this for initialization
    void Start() {
        humans = new List<HumanInfo>();
        NetworkServer.Listen(port);
        NetworkServer.RegisterHandler(MsgType.Connect, onConnection);
        NetworkServer.RegisterHandler(MsgType.Disconnect, onDisnnection);

        serverText.text = "SERVER STARTED";
        humansTotal.text = "Humans Total: " + humans.Count.ToString();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            serverText.text = "STATUS: RESPONDING";
            sendPosition();
        }

        humansTotal.text = "Humans Total: " + humans.Count.ToString();
    }

    //Handler for any incoming connection requests
    void onConnection(NetworkMessage message) {
        short newPlayerID = GenerageUniquePlayerId();
        serverText.text = "STATUS: CONNECTION";
        NetworkServer.RegisterHandler(newPlayerID, OnPlayerUpdate);
        humans.Add(new HumanInfo(newPlayerID));

        // send to player their ID
        NetworkMess msg = new NetworkMess();
        msg.messageContents = newPlayerID.ToString();
        NetworkServer.SendToAll(MSG_NEW_HUMAN, msg);
        print(newPlayerID);
    }

    void onDisnnection(NetworkMessage message)
    {
        //HumanInfo temp = message.ReadMessage<HumanInfo>();
    }

    void OnPlayerUpdate(NetworkMessage message)
    {
        HumanInfo temp = message.ReadMessage<HumanInfo>();
        if (temp.getMessage().Equals("New Player"))
        {
            for (int i = 0; i < humans.Count; i++)
            {
                if (humans[i].getID() == temp.getID())
                {
                    humans[i].setX(temp.getX());
                    humans[i].setY(temp.getY());
                    //print(temp.getX() + " " + temp.getY());
                }
            }
        }
        else if (temp.getMessage().Equals("Disconnecting"))
        {
            int index = FindHumanById(temp.getID());
            if (index != -1) {
                humans.RemoveAt(index);
            }
        }
    }

    short GenerageUniquePlayerId()
    {
        short newID = (short)Random.Range(1, SHRT_MAX);
        for (int i=0; i<humans.Count; i++)
        {
            if (humans[i].getID() == newID)
            {
                i = 0;
                newID = (short)Random.Range(1, SHRT_MAX);
            }
        }
        return newID;
    }

    public void sendPosition() {
        NetworkMess msg = new NetworkMess();
        if (yField.text == "" || xField.text == "") {
            msg.messageContents = "N/A";
        } else {
            msg.messageContents = yField.text + " " + xField.text;
        }
        NetworkServer.SendToAll(MSG_POSITION, msg);
        serverText.text = "STATUS: CONNECTED";
    }

    public void returnToMenu() {
        SceneManager.LoadScene(0);
    }

    int FindHumanById(short id)
    {
        for (int i=0; i<humans.Count; i++)
        {
            if (humans[i].getID() == id)
            {
                return i;
            }
        }
        return -1;
    }
}
