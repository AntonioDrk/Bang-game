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
    
    private void Start()
    {
        _pv = GetComponent<PhotonView>();

        // Spawn at the correct position
        // We count each connection through index
        // We also keep a reference to the object
        if (_pv.IsMine)
        {
            // Tells the server to register this players seat 
            GameSetup.setup.photonView.RPC("RPC_SetPlayerSeat", RpcTarget.AllBufferedViaServer, _pv.Owner.ActorNumber);
            
            // Instaintiates the player at the correct spawn position
            _pv.RPC("RPC_SetSpawnIndex", RpcTarget.AllBufferedViaServer);
        }
    }

    [PunRPC]
    void RPC_SetSpawnIndex()
    {
        _pv = GetComponent<PhotonView>();
        for (spawnIndex = 0; spawnIndex < GameSetup.setup.playerSeats.Length; spawnIndex++)
            if (GameSetup.setup.playerSeats[spawnIndex] == _pv.Owner.ActorNumber)
                break;

        if (spawnIndex >= GameSetup.setup.playerSeats.Length)
        {
            spawnIndex = 0;
            Debug.LogError("Couldn't find the player seat associated with this player, help!");
        }

        if (_pv.IsMine)
        {
            myPrefab = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerPrefab"), 
                GameSetup.setup.spawnPoints[spawnIndex].position, GameSetup.setup.spawnPoints[spawnIndex].rotation, 0);
            //myPrefab.transform.parent = ;
            myPrefab.GetComponent<PhotonView>().RPC("RPC_DisableCamera", RpcTarget.AllBuffered);
        }
    }
    
}
