using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;
using System.Collections.Generic;

public class NetworkServer : MonoBehaviour
{
    public NetworkDriver networkDriver;
    private NativeList<NetworkConnection> networkConnections;
    NetworkPipeline reliableAndInOrderPipeline;
    NetworkPipeline nonReliableNotInOrderedPipeline;
    const ushort NetworkPort = 9001;
    const int MaxNumberOfClientConnections = 1000;
    Dictionary<int, NetworkConnection> idToConnectionLookup;
    Dictionary<NetworkConnection, int> connectionToIDLookup;

    void Start()
    {
        if (NetworkServerProcessing.GetNetworkServer() == null)
        {
            NetworkServerProcessing.SetNetworkServer(this);
            DontDestroyOnLoad(this.gameObject);

            #region Connect

            idToConnectionLookup = new Dictionary<int, NetworkConnection>();
            connectionToIDLookup = new Dictionary<NetworkConnection, int>();

            networkDriver = NetworkDriver.Create();
            reliableAndInOrderPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
            nonReliableNotInOrderedPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage));
            NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
            endpoint.Port = NetworkPort;

            int error = networkDriver.Bind(endpoint);
            if (error != 0)
                Debug.Log("Failed to bind to port " + NetworkPort);
            else
                networkDriver.Listen();

            networkConnections = new NativeList<NetworkConnection>(MaxNumberOfClientConnections, Allocator.Persistent);

            #endregion
        }
        else
        {
            Debug.Log("Singleton-ish architecture violation detected, investigate where NetworkedServer.cs Start() is being called.  Are you creating a second instance of the NetworkedServer game object or has the NetworkedServer.cs been attached to more than one game object?");
            Destroy(this.gameObject);
        }
    }

    void OnDestroy()
    {
        networkDriver.Dispose();
        networkConnections.Dispose();
    }

    void Update()
    {
        networkDriver.ScheduleUpdate().Complete();

        #region Remove Unused Connections

        for (int i = 0; i < networkConnections.Length; i++)
        {
            if (!networkConnections[i].IsCreated)
            {
                networkConnections.RemoveAtSwapBack(i);
                i--;
            }
        }

        #endregion

        #region Accept New Connections

        while (AcceptIncomingConnection());

        #endregion

        #region Manage Network Events

        DataStreamReader streamReader;
        NetworkPipeline pipelineUsedToSendEvent;
        NetworkEvent.Type networkEventType;

        for (int i = 0; i < networkConnections.Length; i++)
        {
            if (!networkConnections[i].IsCreated)
                continue;

            while (PopNetworkEventAndCheckForData(networkConnections[i], out networkEventType, out streamReader, out pipelineUsedToSendEvent))
            {
                TransportPipeline pipelineUsed = TransportPipeline.NotIdentified;
                if (pipelineUsedToSendEvent == reliableAndInOrderPipeline)
                    pipelineUsed = TransportPipeline.ReliableAndInOrder;
                else if (pipelineUsedToSendEvent == nonReliableNotInOrderedPipeline)
                    pipelineUsed = TransportPipeline.FireAndForget;

                switch (networkEventType)
                {
                    case NetworkEvent.Type.Data:
                    
                        NetworkServerProcessing.ReceivedMessageFromClient(streamReader, connectionToIDLookup[networkConnections[i]]);

                        break;
                    case NetworkEvent.Type.Disconnect:

                        NetworkConnection nc = networkConnections[i];
                        int id = connectionToIDLookup[nc];
                        NetworkServerProcessing.DisconnectionEvent(id);
                        idToConnectionLookup.Remove(id);
                        connectionToIDLookup.Remove(nc);
                        networkConnections[i] = default(NetworkConnection);
                        break;
                }
            }
        }

        #endregion
    }

    private bool AcceptIncomingConnection()
    {

        NetworkConnection connection = networkDriver.Accept();
        if (connection == default(NetworkConnection))
            return false;

        NetworkServerProcessing.gameLogic.isClientIn = true;

        networkConnections.Add(connection);

        int id = 0;
        while (idToConnectionLookup.ContainsKey(id))
        {
            id++;
        }
        idToConnectionLookup.Add(id, connection);
        connectionToIDLookup.Add(connection, id);

        NetworkServerProcessing.ConnectionEvent(id);

        SendAllBalloons(connection);

        return true;
    }

    private bool PopNetworkEventAndCheckForData(NetworkConnection networkConnection, out NetworkEvent.Type networkEventType, out DataStreamReader streamReader, out NetworkPipeline pipelineUsedToSendEvent)
    {
        networkEventType = networkConnection.PopEvent(networkDriver, out streamReader, out pipelineUsedToSendEvent);

        if (networkEventType == NetworkEvent.Type.Empty)
            return false;
        return true;
    }

    public void SendMessageToClient(string msg, int connectionID)
    {
        NetworkPipeline networkPipeline = reliableAndInOrderPipeline;

        byte[] msgAsByteArray = Encoding.Unicode.GetBytes(msg);
        NativeArray<byte> buffer = new NativeArray<byte>(msgAsByteArray, Allocator.Persistent);
        DataStreamWriter streamWriter;

        networkDriver.BeginSend(networkPipeline, idToConnectionLookup[connectionID], out streamWriter);
        streamWriter.WriteInt(buffer.Length);
        streamWriter.WriteBytes(buffer);
        networkDriver.EndSend(streamWriter);

        buffer.Dispose();
    }

    public void SendNewBalloonToClient(Vector2 location)
    {
        NetworkPipeline networkPipeline = reliableAndInOrderPipeline;

        foreach (NetworkConnection c in networkConnections)
        {
            DataStreamWriter streamWriter;
            networkDriver.BeginSend(networkPipeline, c, out streamWriter);
            streamWriter.WriteInt(ServerToClientSignifiers.NewBalloon);
            streamWriter.WriteFloat(location.x);
            streamWriter.WriteFloat(location.y);
            networkDriver.EndSend(streamWriter);
        }
    }

    public void SendPoppedBalloonToClient(Vector2 location)
    {

        NetworkPipeline networkPipeline = reliableAndInOrderPipeline;

        foreach (NetworkConnection c in networkConnections)
        {
            DataStreamWriter streamWriter;
            networkDriver.BeginSend(networkPipeline, c, out streamWriter);
            streamWriter.WriteInt(ServerToClientSignifiers.PoppedBalloon);
            streamWriter.WriteFloat(location.x);
            streamWriter.WriteFloat(location.y);
            networkDriver.EndSend(streamWriter);
        }
    }

    public void SendAllBalloons(NetworkConnection connection)
    {
        NetworkPipeline networkPipeline = reliableAndInOrderPipeline;

        DataStreamWriter streamWriter;
        networkDriver.BeginSend(networkPipeline, connection, out streamWriter);
        streamWriter.WriteInt(ServerToClientSignifiers.AllBalloon);

        foreach (Vector2 v2 in NetworkServerProcessing.gameLogic.allBallons)
        {
            streamWriter.WriteInt(1);
            streamWriter.WriteFloat(v2.x);
            streamWriter.WriteFloat(v2.y);
        }

        streamWriter.WriteInt(0);

        networkDriver.EndSend(streamWriter);

    }
}

public enum TransportPipeline
{
    NotIdentified,
    ReliableAndInOrder,
    FireAndForget
}
