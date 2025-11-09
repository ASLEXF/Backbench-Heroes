using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkGUI : MonoBehaviour
{
    private NetworkManager m_NetworkManager;
    public GUISkin customSkin;

    void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
    }

    void OnGUI()
    {
        //Debug.Log("OnGUI called: " + Event.current.type);
        if (customSkin != null)
            GUI.skin = customSkin;
        GUILayout.BeginArea(new Rect(10, 10, 800, 600));
        if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();

            //SubmitNewPosition();
        }

        GUILayout.EndArea();
    }

    // Make these methods instance methods so they can access m_NetworkManager
    void StartButtons()
    {
        if (GUILayout.Button("Host", GUILayout.Height(100))) m_NetworkManager.StartHost();
        if (GUILayout.Button("Client", GUILayout.Height(100))) m_NetworkManager.StartClient();
        if (GUILayout.Button("Server", GUILayout.Height(100))) m_NetworkManager.StartServer();
    }

    void StatusLabels()
    {
        var mode = m_NetworkManager.IsHost ?
            "Host" : m_NetworkManager.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    //void SubmitNewPosition()
    //{
    //    if (GUILayout.Button(m_NetworkManager.IsServer ? "Move" : "Request Position Change"))
    //    {
    //        if (m_NetworkManager.IsServer && !m_NetworkManager.IsClient)
    //        {
    //            foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
    //                m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().Move();
    //        }
    //        else
    //        {
    //            var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
    //            var player = playerObject.GetComponent<HelloWorldPlayer>();
    //            player.Move();
    //        }
    //    }
    //}
}
