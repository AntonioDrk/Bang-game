using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private InputField _roomNameInput;

    public void OnCreateRoomBtn()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {

            if (!string.IsNullOrWhiteSpace(_roomNameInput.text))
            {
                RoomOptions ropt = new RoomOptions
                {
                    MaxPlayers = 4,
                    EmptyRoomTtl = 0
                };
                PhotonNetwork.CreateRoom(_roomNameInput.text, ropt);
            }
            else
            {
                //TODO:Display an error message to the user
            }

        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // TODO: Display error message to the user
    }
}
