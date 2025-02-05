﻿using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.UI;

public class JoinedRoom : MonoBehaviourPunCallbacks
{
    [SerializeField]
    Text _roomName;
    private Text RoomName => _roomName;

    [SerializeField]
    private GameObject _playerListingPrefab;

    [SerializeField] private GameObject _startButton;
    private GameObject PlayerListingPrefab => _playerListingPrefab;

    private List<PlayerListedItem> _playerListedItems = new List<PlayerListedItem>();
    private List<PlayerListedItem> PlayerListedItems
    {
        get { return _playerListedItems; }
    }

    public void OnLeaveBtn()
    {
        PhotonNetwork.LeaveRoom(false);
    }

    public void OnClickStartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(1);
    }

    public override void OnJoinedRoom()
    {
        // Set up the room name to be visible
        if(_roomName != null)
        {
            if(PhotonNetwork.CurrentRoom != null)
            {
                _roomName.text = PhotonNetwork.CurrentRoom.Name;
            }
        }
        else
        {
            Debug.LogError("Room title in room panel not set up.");
        }

        // Cache the player list
        Player[] photonPlayers = PhotonNetwork.PlayerList;
        for(int i = 0; i < photonPlayers.Length; i++)
        {
            // Call the local function to add the players in GUI
            PlayerJoinedRoom(photonPlayers[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerJoinedRoom(newPlayer);
    }

    // Called when a remote player left the room or became inactive. Check otherPlayer.IsInactive.
    public override void OnPlayerLeftRoom(Player photonPlayer)
    {
        PlayerLeftRoom(photonPlayer);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Successfully left the room");
        CanvasManager.Instance.HideLobby(false);
        CanvasManager.Instance.HideRoom(true);

        foreach (PlayerListedItem item in PlayerListedItems)
        {
            Destroy(item.gameObject);
        }
        PlayerListedItems.RemoveRange(0, PlayerListedItems.Count);
    }

    private void PlayerJoinedRoom(Player photonPlayer)
    {
        if (photonPlayer == null)
            return;
        // Makes sure we don't have duplicates by accident
        PlayerLeftRoom(photonPlayer);

        // Instantiate the listing prefab and attach it to the content of the scroll view
        GameObject playerListingObject = Instantiate(PlayerListingPrefab, transform, false);

        // Get the script from the instantiated object
        PlayerListedItem playerListing = playerListingObject.GetComponent<PlayerListedItem>();
        // Cache the player and show his name
        playerListing.ApplyPlayer(photonPlayer);

        // Throw here everything you want to show only to the HOST of the room
        if (PhotonNetwork.IsMasterClient && photonPlayer != PhotonNetwork.LocalPlayer)
        {
            playerListing.KickButton.gameObject.SetActive(true);
            _startButton.SetActive(true);
            
        }
        else
        {
            // Implicit setting of the kick button to be deactivated to avoid future problems
            playerListing.KickButton.gameObject.SetActive(false);
            _startButton.SetActive(false);    
        }

        // Cache the local playerListing script
        PlayerListedItems.Add(playerListing);

    }

    private void PlayerLeftRoom(Player photonPlayer)
    {
        // Find the player index
        int index = PlayerListedItems.FindIndex(x => x.PhotonPlayer == photonPlayer);
        if(index != -1)
        {
            // If we found the index, destroy the gameobject
            Destroy(PlayerListedItems[index].gameObject);
            // Remove it from the list
            PlayerListedItems.RemoveAt(index);
        }
    }
}
