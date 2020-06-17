using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ListingRooms : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject _listItem;
    [SerializeField]
    private GameObject _scrollContent;
    private List<RoomInfo> _listRoomsLocal = new List<RoomInfo>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("There's a room update");
        foreach (RoomInfo roomInfo in roomList)
        {
            // If the room is removed
            if (roomInfo.RemovedFromList)
            {
                // We remove it from the gui as well
                int index = _listRoomsLocal.IndexOf(roomInfo);
                if(index != -1)
                {
                    _listRoomsLocal.Remove(roomInfo);
                    Destroy(_scrollContent.transform.GetChild(index).gameObject);
                }
            }
            else
            {
                // Otherwise check to see if it is already displayed or not
                if (!_listRoomsLocal.Contains(roomInfo))
                {
                    GameObject instantiatedListItem = Instantiate(_listItem, _scrollContent.transform);
                    instantiatedListItem.transform.GetChild(0).GetComponent<Text>().text = roomInfo.Name;
                    instantiatedListItem.transform.GetChild(1).GetComponent<Text>().text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
                    _listRoomsLocal.Add(roomInfo);
                }
                else
                {
                    int index = _listRoomsLocal.IndexOf(roomInfo);
                    _scrollContent.transform.GetChild(index).GetChild(1).gameObject.GetComponent<Text>().text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
                }
            }
        }

    }
}
