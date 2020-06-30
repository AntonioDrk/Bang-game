using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;
public class RoomListedItem : MonoBehaviour
{
    [SerializeField]
    private Text _roomName;

    public void OnClickRoomListedItem()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName) || string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
        {
            CanvasManager.Instance.ShowPopup(CanvasManager.Instance.ErrorTitle, CanvasManager.Instance.EmptyNickNameErrorMsg);
            return;
        }
        
        if(_roomName != null && !string.IsNullOrWhiteSpace(_roomName.text))
        {
            PhotonNetwork.JoinRoom(_roomName.text);
        }
        else
        {
            Debug.LogError("Room name not set?!");
        }
        
    }
}
