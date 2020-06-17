using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviourPunCallbacks
{
    public string gameVersion = "0.1";
    public int GameScene = 1;
    private PhotonView _pv; 

    private void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
        DontDestroyOnLoad(this.gameObject);
        _pv = GetComponent<PhotonView>();
    }
    
    void Start()
    {
        // Connects to the PUN Servers with the default settings
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    // Once the game scene is loaded, create the player connection object
    private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        int currentScene = scene.buildIndex;
        if (currentScene == GameScene)
        {
            CreatePlayer();
        }
    }

    private void CreatePlayer()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), transform.position,
            Quaternion.identity, 0);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Launcher: We connected to master server!");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Launcher: Joined default lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Launcher: We got disconnected from master server, reason " + cause);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined the room");
        CanvasManager.Instance.HideLobby(true);
        CanvasManager.Instance.HideRoom(false);
    }
}
