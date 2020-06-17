using UnityEngine;
using UnityEngine.UI;

public sealed class CanvasManager : ScriptableObject
{
    [Tooltip("The top most parent of all the elements that compose the lobby UI elements.")]
    [SerializeField]
    private CanvasGroup _lobbyCanvasGroup;
    private CanvasGroup LobbyCanvasGroup { get { return _lobbyCanvasGroup; } }

    [Tooltip("The top most parent of all the elements that compose the room UI elements.")]
    [SerializeField]
    private CanvasGroup _roomCanvasGroup;
    private CanvasGroup RoomCanvasGroup { get { return _roomCanvasGroup; } }

    private static CanvasManager instance = null;
    // Thread safety for the singleton
    private static readonly object padlock = new object();

    private CanvasManager() { }

    public static CanvasManager Instance
    {
        get
        {
            // Thread safe
            lock (padlock)
            {
                if(instance == null)
                {
                    instance = CreateInstance<CanvasManager>();
                }
                return instance;
            }
        }
    }

    private void Awake()
    {
        _lobbyCanvasGroup = GameObject.FindGameObjectWithTag("LobbyCanvas").GetComponent<CanvasGroup>();
        _roomCanvasGroup = GameObject.FindGameObjectWithTag("RoomCanvas").GetComponent<CanvasGroup>();

        if (RoomCanvasGroup == null)
        {
            Debug.LogError("Canvas group for room not assigned to canvas manager");
        }

        if (LobbyCanvasGroup == null)
        {
            Debug.LogError("Canvas group for lobby not assigned to canvas manager");
        }
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
            Debug.LogError("The room canvas group isn't attributed");
        }
    }
}
