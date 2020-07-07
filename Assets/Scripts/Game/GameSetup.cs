using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class GameSetup : MonoBehaviour
{
    public static GameSetup instance;

    public List<Transform> spawnPoints;
    public int[] playerSeats;

    private int numberOfPlayer = 0;
    
    public PhotonView photonView;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        if (photonView == null)
            photonView = GetComponent<PhotonView>();

        // Initializes the player seats with the ID 0
        playerSeats = new int[spawnPoints.Count];
        for (int i = 0; i < playerSeats.Length; i++)
            playerSeats[i] = 0;
    }

    public GameSetup GetInstance()
    {
        if (instance == null)
            instance = this;
        return instance;
    }
    
    /// <summary>
    /// Gets the player object that sits at index
    /// </summary>
    /// <param name="index">The index of the seat (0-3)</param>
    /// <returns>The player GameObject if it's found, if not returns NULL</returns>
    public GameObject GetPlayerObjectAtSeat(int index)
    {
        // Check to see if the index isn't out of bounds
        if (index < 0 || index >= playerSeats.Length) return null;

        Transform playerContainer = GameObject.FindGameObjectWithTag("PlayerObjects").transform;
        for (int i = 0; i < playerContainer.childCount; i++)
        {
            GameObject playerObj = playerContainer.GetChild(i).gameObject;
            if (playerObj.GetComponent<PhotonView>().Owner.ActorNumber == playerSeats[index])
                return playerObj;
        }

        return null;
    }
    
    /// <summary>
    /// Gets the seat position of the player with a given actornumber
    /// </summary>
    /// <param name="actorNumber">The actor number of the player foun in the photon view</param>
    /// <returns>The seat index if it's found, if not return -1</returns>
    public int GetPlayerSeatWithActorNumber(int actorNumber)
    {
        for (int i = 0; i < playerSeats.Length; i++)
        {
            if (playerSeats[i] == actorNumber)
                return i;
        }

        return -1;
    }

    public int NumberOfOccupiedSeats()
    {
        return numberOfPlayer;
    }
    
    [PunRPC]
    private void RPC_SetPlayerSeat(int viewId)
    {
        for (int index = 0; index < playerSeats.Length; index++)
        {
            if (playerSeats[index] == 0)
            {
                playerSeats[index] = viewId;
                numberOfPlayer++;
                break;
            }
        }
        
        // if(PhotonNetwork.IsMasterClient && numberOfPlayer == PhotonNetwork.CurrentRoom.MaxPlayers)
        //     GameObject.FindGameObjectWithTag("GameManager").GetComponent<PhotonView>().RPC("RPC_DistributeRoles", RpcTarget.AllBufferedViaServer);
        
        
    }
}
