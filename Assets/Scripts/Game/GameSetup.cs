using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class GameSetup : MonoBehaviour
{
    public static GameSetup setup;

    public List<Transform> spawnPoints;
    public int[] playerSeats;
    public PhotonView photonView;

    private void Awake()
    {
        if (setup == null)
            setup = this;

        if (photonView == null)
            photonView = GetComponent<PhotonView>();

        // Initializes the player seats with the ID 0
        playerSeats = new int[spawnPoints.Count];
        for (int i = 0; i < playerSeats.Length; i++)
            playerSeats[i] = 0;
    }
    
    [PunRPC]
    private void RPC_SetPlayerSeat(int viewId)
    {
        for (int index = 0; index < playerSeats.Length; index++)
            if (playerSeats[index] == 0)
            {
                playerSeats[index] = viewId;
                break;
            }
    }
}
