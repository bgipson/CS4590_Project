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
    public float maxTimeBeforeKick = 5.0f;

    List<PlayerInfo> players;

    public Server()
    {
        // avoid:
        // called 2x in scene start
        // called 0x times when started from menu
    }

    // Use this for initialization
    void Start() {
        players = new List<PlayerInfo>();
        NetworkServer.Listen(port);
        NetworkServer.RegisterHandler(MsgType.Connect, onConnection);
        NetworkServer.RegisterHandler(MsgType.Disconnect, onDisnnection);

        serverText.text = "SERVER STARTED";
        humansTotal.text = "Humans Total: " + players.Count.ToString();
    }

    // Update is called once per frame
    void Update() {
        //sendPosition();
        if (players.Count == 0)
        {
            serverText.text = "SERVER STARTED";
        }
        // check if players have timed out
        for (int i=players.Count-1; i>=0; i--) // must go backward when deleting
        {
            if ((players[i].getTimeStamp() + maxTimeBeforeKick < Time.time) &&
                players[i].getTimeStamp() > 0)
            {
                print("Player timed out: " + players[i].getID());
                NetworkServer.UnregisterHandler(players[i].getID());
                players.RemoveAt(i);
            }
        }

        humansTotal.text = "Humans Total: " + players.Count.ToString();
    }

    //Handler for any incoming connection requests
    void onConnection(NetworkMessage message) {
        short newPlayerID = GenerageUniquePlayerId();
        serverText.text = "STATUS: CONNECTION";
        NetworkServer.RegisterHandler(newPlayerID, OnPlayerUpdate);
        PlayerInfo newPlayer = new PlayerInfo(newPlayerID);
        newPlayer.Init();
        players.Add(newPlayer);

        // send to player their ID
        NetworkMess msg = new NetworkMess();
        msg.messageContents = newPlayerID.ToString();
        NetworkServer.SendToAll(MSG_NEW_HUMAN, msg);
        print("new player: " + newPlayerID);
    }

    void onDisnnection(NetworkMessage message)
    {
        //PlayerInfo temp = message.ReadMessage<PlayerInfo>();
    }

    void OnPlayerUpdate(NetworkMessage message)
    {
        PlayerInfo temp = message.ReadMessage<PlayerInfo>();
        int index = -1;
        if (temp.getMessage() != null) {
            if (temp.getMessage().Equals("Disconnecting"))
            {
                index = FindPlayerById(temp.getID());
                if (index != -1)
                {
                    print("player disconnecting: " + players[index].getID());
                    NetworkServer.UnregisterHandler(temp.getID());
                    players.RemoveAt(index);
                }
            }
        }
        index = FindPlayerById(temp.getID());
        if (index != -1)
        {
            players[index].setTimeStamp(Time.time);
            players[index].setPosition(temp.getPosition());
            //print("x: " + temp.getPosition().x + " z: " + temp.getPosition().z);
        }
    }

    short GenerageUniquePlayerId()
    {
        short newID = (short)Random.Range(1, SHRT_MAX);
        for (int i=0; i<players.Count; i++)
        {
            if (players[i].getID() == newID)
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

    int FindPlayerById(short id)
    {
        for (int i=0; i<players.Count; i++)
        {
            if (players[i].getID() == id)
            {
                return i;
            }
        }
        return -1;
    }
}
