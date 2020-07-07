using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Responsible for managing the network aspects of the game itself
/// Eg Player spawning, player ownership 
/// </summary>
public class NetworkPhotonPlayer : MonoBehaviourPunCallbacks
{
    // Contains the prefab associated with this player
    public GameObject myPrefab;
    private PhotonView _pv;

    public int spawnIndex = 0;
    
    private void Awake()
    {
        _pv = GetComponent<PhotonView>();

        // Spawn at the correct position
        // We count each connection through index
        // We also keep a reference to the object
        if (_pv.IsMine)
        {
            // Tells the server to register this players seat 
            GameSetup.instance.photonView.RPC("RPC_SetPlayerSeat", RpcTarget.AllBufferedViaServer, _pv.Owner.ActorNumber);
            
            // Instaintiates the player at the correct spawn position
            _pv.RPC("RPC_SetSpawnIndex", RpcTarget.AllBufferedViaServer);
        }
    }

    [PunRPC]
    void RPC_SetSpawnIndex()
    {
        _pv = GetComponent<PhotonView>();
        for (spawnIndex = 0; spawnIndex < GameSetup.instance.playerSeats.Length; spawnIndex++)
            if (GameSetup.instance.playerSeats[spawnIndex] == _pv.Owner.ActorNumber)
                break;

        if (spawnIndex >= GameSetup.instance.playerSeats.Length)
        {
            spawnIndex = 0;
            Debug.LogError("Couldn't find the player seat associated with this player, help!");
        }

        if (_pv.IsMine)
        {
            myPrefab = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerPrefab"), 
                GameSetup.instance.spawnPoints[spawnIndex].position, GameSetup.instance.spawnPoints[spawnIndex].rotation, 0);
            myPrefab.GetComponent<PhotonView>().RPC("RPC_DisableCamera", RpcTarget.AllBuffered);
        }
        
        if (GameSetup.instance.NumberOfOccupiedSeats() == PhotonNetwork.CurrentRoom.MaxPlayers) 
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<PhotonView>().RPC("RPC_DistributeRoles", RpcTarget.MasterClient);
    }
    
}
