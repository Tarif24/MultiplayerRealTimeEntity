using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

static public class NetworkServerProcessing
{

    #region Send and Receive Data Functions
    static public void ReceivedMessageFromClient(DataStreamReader streamReader, int clientConnectionID)
    {

        int signifier = streamReader.ReadInt();


        if (signifier == ClientToServerSignifiers.PoppedBalloon)
        {

            float x = streamReader.ReadFloat();
            float y = streamReader.ReadFloat();

            Vector2 temp = new Vector2(x, y);

            ProcessPoppedBalloon(temp);
        }

        //gameLogic.DoSomething();
    }
    static public void SendMessageToClient(string msg, int clientConnectionID)
    {
        networkServer.SendMessageToClient(msg, clientConnectionID);
    }

    static public void SendNewBalloonToClient(Vector2 location)
    {
        networkServer.SendNewBalloonToClient(location);
    }

    static public void SendPoppedBalloonToClient(Vector2 location)
    {
        networkServer.SendPoppedBalloonToClient(location);
    }

    static public void ProcessPoppedBalloon(Vector2 location)
    {
        SendPoppedBalloonToClient(location);
        gameLogic.allBallons.Remove(location);
    }

    #endregion

    #region Connection Events

    static public void ConnectionEvent(int clientConnectionID)
    {
        Debug.Log("Client connection, ID == " + clientConnectionID);
    }
    static public void DisconnectionEvent(int clientConnectionID)
    {
        Debug.Log("Client disconnection, ID == " + clientConnectionID);
    }

    #endregion

    #region Setup
    static NetworkServer networkServer;
    static public GameLogic gameLogic;

    static public void SetNetworkServer(NetworkServer NetworkServer)
    {
        networkServer = NetworkServer;
    }
    static public NetworkServer GetNetworkServer()
    {
        return networkServer;
    }
    static public void SetGameLogic(GameLogic GameLogic)
    {
        gameLogic = GameLogic;
    }

    #endregion
}

#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const int PoppedBalloon = 0;
}

static public class ServerToClientSignifiers
{
    public const int NewBalloon = 0;
    public const int PoppedBalloon = 1;
    public const int AllBalloon = 2;
}

#endregion

