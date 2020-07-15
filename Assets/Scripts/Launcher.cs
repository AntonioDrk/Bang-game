using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class Launcher : MonoBehaviourPunCallbacks
{
    public string gameVersion = "0.1";
    public int GameScene = 1;
    public int MultiplayerScene = 0;
    public int lastScene;
    private PhotonView _pv;
    [SerializeField] private JoinedRoom _joinedRoom;
    [SerializeField] private int _id = -1;

    public static GameObject instance;

    public static Launcher Get(int id)
    {
        List<Launcher> instances = new List<Launcher>(FindObjectsOfType<Launcher>());
        return instances.FirstOrDefault(i => i._id == id);
    }
    
    private void Awake()
    {
        
        if(instance != null)
            Destroy(instance);
        instance = gameObject;

        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
        
        DontDestroyOnLoad(gameObject);
        _pv = GetComponent<PhotonView>();

        // Temporary fix avoiding the "You got kicked" message
        _joinedRoom._leftByChoice = true;
    }
    
    void Start()
    {
        // Connects to the PUN Servers with the default settings
        Debug.LogWarning("Trying to connect to the master server!");
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            //PhotonNetwork.Disconnect();
        }
        else
        {
            
        }
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
            PhotonNetwork.AutomaticallySyncScene = false;
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
        CanvasManager.Instance.SetStatus("Connected", new Color(105/256f,217/256f,87/256f));
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Launcher: Joined default lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Launcher: We got disconnected from master server, reason " + cause);
        CanvasManager.Instance.SetStatus("Connecting", new Color(254/256f,255/256f,102/256f));
        PhotonNetwork.Reconnect();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined the room");
        CanvasManager.Instance.HideLobby(true);
        CanvasManager.Instance.HideRoom(false);
    }
}
