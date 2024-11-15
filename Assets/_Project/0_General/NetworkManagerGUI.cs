using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkManagerGUI : MonoBehaviour
    {
        static private NetworkManager m_NetworkManager;

        void Awake()
        {
            m_NetworkManager = GetComponent<NetworkManager>();
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 200));
            if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();

                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        static void StartButtons()
        {
            if (GUILayout.Button("Host")) m_NetworkManager.StartHost();
            if (GUILayout.Button("Client")) m_NetworkManager.StartClient();
            if (GUILayout.Button("Server")) m_NetworkManager.StartServer();
            if (GUILayout.Button("Quit")) Application.Quit();
        }
        
        static void StatusLabels()
        {
            var mode = m_NetworkManager.IsHost ?
                "Host" : m_NetworkManager.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        static void SubmitNewPosition()
        {
            if (GUILayout.Button(m_NetworkManager.IsServer ? "Host Dash" : "Client Request Dash"))
            {
                //This condition checks if the instance is a dedicated server (i.e., it’s only the server, without any client functions). Ensures this is a server-only instance and not a host
                if (m_NetworkManager.IsServer && !m_NetworkManager.IsClient )
                {
                    /*
                     - m_NetworkManager.ConnectedClientsIds: return a list of unique client IDs (ulong type) for all connected clients.
                     
                     - m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid): retrieves the NetworkObject for a specific player using their uid. The NetworkObject represents the networked player object associated with each client.
                     */
                    foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
                        m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<PlayerMovementController>().Move();
                }
                //If the instance is not a dedicated server (meaning it’s either a client or a host), the code in the else block executes.
                else
                {
                    var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<PlayerMovementController>();
                    player.Move();
                }
            }

            if (GUILayout.Button("Quit"))
            {
                Application.Quit();
            }
        }
    }