using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

static public class NetworkClientProcessing
{

    #region Send and Receive Data Functions
    static public void ReceivedMessageFromServer(DataStreamReader streamReader)
    {

        int signifier = streamReader.ReadInt();

        if (signifier == ServerToClientSignifiers.NewBalloon)
        {
            float x = streamReader.ReadFloat();
            float y = streamReader.ReadFloat();
            Vector2 temp = new Vector2(x, y);

            gameLogic.SpawnNewBalloon(temp);
        }
        else if (signifier == ServerToClientSignifiers.PoppedBalloon)
        {
            float x = streamReader.ReadFloat();
            float y = streamReader.ReadFloat();
            Vector2 temp = new Vector2(x, y);

            ProcessPoppedBalloon(temp);
        }
        else if (signifier == ServerToClientSignifiers.AllBalloon)
        {

        }

        //gameLogic.DoSomething();

    }

    static public void SendMessageToServer(string msg)
    {
        networkClient.SendMessageToServer(msg);
    }

    static public void SendPoppedBalloon(Vector2 location)
    {
        networkClient.SendPoppedBalloon(location);
    }

    static public void ProcessPoppedBalloon(Vector2 location)
    {
        gameLogic.DestroyBalloon(location);
    }

    #endregion

    #region Connection Related Functions and Events
    static public void ConnectionEvent()
    {
        Debug.Log("Network Connection Event!");
    }
    static public void DisconnectionEvent()
    {
        Debug.Log("Network Disconnection Event!");
    }
    static public bool IsConnectedToServer()
    {
        return networkClient.IsConnected();
    }
    static public void ConnectToServer()
    {
        networkClient.Connect();
    }
    static public void DisconnectFromServer()
    {
        networkClient.Disconnect();
    }

    #endregion

    #region Setup
    static NetworkClient networkClient;
    static GameLogic gameLogic;

    static public void SetNetworkedClient(NetworkClient NetworkClient)
    {
        networkClient = NetworkClient;
    }
    static public NetworkClient GetNetworkedClient()
    {
        return networkClient;
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

