using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListedItem : MonoBehaviour
{
    public Player PhotonPlayer { get; private set; }

    [SerializeField]
    private Button _kickButton;
    public Button KickButton { get { return _kickButton; } }

    [SerializeField]
    private Text _playerName;
    private Text PlayerName { get { return _playerName; } }

    public void ApplyPlayer(Player player)
    {
        PlayerName.text = player.NickName;
        PhotonPlayer = player;
    }


    // Don't get confused, this piece gets run on the hosts computer, that's why it will work
    public void OnKickButton_Clicked()
    {
        PhotonNetwork.CloseConnection(PhotonPlayer);
    }
}
