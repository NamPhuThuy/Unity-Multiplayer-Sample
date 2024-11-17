using System.Collections;
using System.Collections.Generic;
using GameFramwork.Manager;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //--MAKE BOTH PLAYERS SPAWN IN THE SAME SCENE
        // Disable the connection approval process. Any client that attempts to connect will be immediately accepted, This is generally suitable for simpler games or situations where you don't need to control which clients can join
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        
        //If you are the Host
        if (RelayManager.Instance.IsHost)
        {   
            /*
             * .ConnectionApprovalCallback: A property of the Network Manager, It's a delegate (a type that represents a reference to a method) that holds a function to be called whenever a client attempts to connect to the server
             */
            NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApproval;
            (byte[] allocationId, byte[] key, byte[] connectionData, string ip, int port) =
                RelayManager.Instance.GetHostConnectionInfo();
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(ip, (ushort)port, allocationId, key, connectionData, true);
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            (byte[] allocationId, byte[] key, byte[] connectionData, byte[] hostConnectionData, string ip, int port) =
                RelayManager.Instance.GetClientConnectionInfo();
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(ip, (ushort)port, allocationId, key, connectionData, hostConnectionData, true);
            NetworkManager.Singleton.StartClient();
        }
        
    }

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true; //Indicates that server accepts the connection request.
        response.CreatePlayerObject = true; //Tells the Network Manager to automatically create a player object (a prefab from your Network Prefab List) for this client on the server. This is typically what you want for a multiplayer game.
        response.Pending = false; //Indicates that the connection approval process is complete, and the client's connection status is no longer pending.
    }
}
