using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CanvasManager : MonoBehaviour
{
    [Tooltip("The top most parent of all the elements that compose the lobby UI elements.")]
    [SerializeField]
    private CanvasGroup _lobbyCanvasGroup;
    private CanvasGroup LobbyCanvasGroup { get { return _lobbyCanvasGroup; } }

    [Tooltip("The top most parent of all the elements that compose the room UI elements.")]
    [SerializeField]
    private CanvasGroup _roomCanvasGroup;
    private CanvasGroup RoomCanvasGroup => _roomCanvasGroup;

    [SerializeField]private CanvasGroup _popupCanvasGroup;

    public CanvasGroup PopupCanvasGroup
    { 
        get => _popupCanvasGroup;
        private set => _popupCanvasGroup = value;
    }

    public readonly string EmptyNickNameErrorMsg = "The nickname field is empty! You need to set a nickname in order to join.";
    public readonly string EmptyRoomNameErrorMsg = "The name field of the room is empty! You need to set a name for the room in order to create one.";
    public readonly string KickedInfoMsg = "You have been kicked from the room.";
    public readonly string ErrorTitle = "Error!";
    public readonly string InfoTitle = "Info!";
    
    public static CanvasManager Instance;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);
        Instance = this;
        
        _lobbyCanvasGroup = GameObject.FindGameObjectWithTag("LobbyCanvas").GetComponent<CanvasGroup>();
        _roomCanvasGroup = GameObject.FindGameObjectWithTag("RoomCanvas").GetComponent<CanvasGroup>();
        _popupCanvasGroup = GameObject.FindGameObjectWithTag("PopupCanvas").GetComponent<CanvasGroup>();

        if (RoomCanvasGroup == null)
            Debug.LogError("Canvas group for room not assigned to canvas manager");


        if (LobbyCanvasGroup == null)
            Debug.LogError("Canvas group for lobby not assigned to canvas manager");

        if(PopupCanvasGroup == null)
            Debug.LogError("Canvas group for popup panel not assigned to canvas manager");
            
    }
    
    public void HideLobby(bool state)
    {
        if(LobbyCanvasGroup != null)
        {
            if (state)
            {
                LobbyCanvasGroup.alpha = 0;
                LobbyCanvasGroup.blocksRaycasts = false;
                LobbyCanvasGroup.interactable = false;
            }
            else
            {
                LobbyCanvasGroup.alpha = 1;
                LobbyCanvasGroup.blocksRaycasts = true;
                LobbyCanvasGroup.interactable = true;
            }
        }
        else
        {
            Debug.LogError("The lobby canvas group isn't attributed");
        }
    }

    public void HideRoom(bool state)
    {
        if (RoomCanvasGroup != null)
        {
            if (state)
            {
                RoomCanvasGroup.alpha = 0;
                RoomCanvasGroup.blocksRaycasts = false;
                RoomCanvasGroup.interactable = false;
            }
            else
            {
                RoomCanvasGroup.alpha = 1;
                RoomCanvasGroup.blocksRaycasts = true;
                RoomCanvasGroup.interactable = true;
            }
        }
        else
        {
            Debug.LogError("The room canvas group isn't attributed!");
        }
    }

    public void HidePopup()
    {
        if (PopupCanvasGroup != null)
        {
            PopupCanvasGroup.alpha = 0;
            PopupCanvasGroup.interactable = false;
            PopupCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("The popup canvas group isn't set!");
        }
    }

    public void ShowPopup(string title, string message)
    {
        if (PopupCanvasGroup != null)
        {
            PopupCanvasGroup.alpha = 1;
            PopupCanvasGroup.interactable = true;
            PopupCanvasGroup.blocksRaycasts = true;
            // Explanation                              ErrorPanel           -> BG      -> Title / Content
            TextMeshProUGUI titleMesh = PopupCanvasGroup.gameObject.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI messageMesh = PopupCanvasGroup.gameObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            if (titleMesh == null || messageMesh == null)
            {
                Debug.LogError("Cannot find the children of the ErrorPanel! Check the pathing.");
                return;
            }

            titleMesh.text = title;
            messageMesh.text = message;

        }
        else
        {
            Debug.LogError("The popup canvas group isn't set!");
        }
    }

    public void SetStatus(string status, Color color)
    {
        if (_lobbyCanvasGroup == null)
        {
            Debug.LogError("The lobby canvas group isn't set!");
            return;
        }

        //                                                   ListingControlls -> CreateRoomControlls->StatusText
        TextMeshProUGUI statusMesh = _lobbyCanvasGroup.gameObject.transform.GetChild(0).GetChild(0).GetChild(3)
            .GetComponent<TextMeshProUGUI>();
        if (statusMesh == null)
        {
            Debug.LogError("Didn't find the status component, check the children count!");
            return;
        }

        statusMesh.text = "Status: " + status;
        statusMesh.color = color;
    }
}
