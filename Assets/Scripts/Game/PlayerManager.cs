using System;
using System.Collections.Generic;
using System.Diagnostics;
using Photon.Pun;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// ReSharper disable ReplaceWithSingleAssignment.True

/// <summary>
/// Component that is responsible of showing, 
/// interacting and deciding things for the players and cards
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PlayerManager : MonoBehaviour
{
    // CONSTANTS
    private const float HandXOffset = -0.07f;
    private const float CardXOffset = -0.15f;
    private const float CardYOffset = -0.007f;

    private const float CardYMovement = 0.1f;
    private const float CardZMovement = -0.6f;

    private const float BoxColCentY = -.375f;
    private const float BoxColSizeY = 1.75f;
    // ---------


    private Vector3 handTransformStartPos;
    
    [SerializeField]
    private Camera _mainCamera;
    
    private List<PlayableCard> playableCards = new List<PlayableCard>();
    private List<GameObject> playableCardsObjects = new List<GameObject>();

    private bool _playerCardActions = false;

    [SerializeField] private GameManager gameManager;

    [SerializeField] private Transform handTransform;
    
    private GameObject _lastObjectHitPersistent;
    private GameObject _objectHit;

    private PhotonView _photonView;

    private void Start()
    {
        handTransformStartPos = handTransform.localPosition;
        _photonView = gameObject.GetComponent<PhotonView>();
        if (_photonView == null)
        {
            Debug.LogError("Photon view not found!");
        }

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if(gameManager == null)
            Debug.LogError("Game manager couldn't be found!");
    }

    private void Update()
    {
        if ( _playerCardActions && !_photonView.IsMine) return;
        
        bool mouseInScreen = true;
        
        // Trying to make so the game doesn't register mouse inputs from outside the game window.
        #if UNITY_EDITOR
                if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 ||
                    Input.mousePosition.x >= Handles.GetMainGameViewSize().x - 1 ||
                    Input.mousePosition.y >= Handles.GetMainGameViewSize().y - 1)
                {
                    mouseInScreen = false;
                }
        #else
                if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || 
                    Input.mousePosition.x >= Screen.width - 1 || 
                    Input.mousePosition.y >= Screen.height - 1) 
                {
                    mouseInScreen = false;
                }
        #endif
        
        // Cast a ray from the mouse position from the camera to the world
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction, Color.green);
        
        // Switch to know if the object hit has changed
        bool objectHitChanged = false;
        // If we hit something
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity) && mouseInScreen)
        {
            
            // If the casted ray hit something different than the last time
            if (_objectHit != hit.transform.gameObject)
            {
                // Save the last object touched by the ray only if it isn't null
                if(!(_objectHit is null))
                    _lastObjectHitPersistent = _objectHit;
                
                _objectHit = hit.transform.gameObject;
                // Signal that there was a change
                objectHitChanged = true;
            }
            
            // Check to see if we put our mouse over a card, if we did we move the card up
            if (!(hit.transform.parent is null) && hit.transform.parent.CompareTag("PlyHand"))
            {
                MoveCardUp(_objectHit);
                if (Input.GetMouseButtonUp(0))
                {
                    MoveCardToMiddleScreen(_objectHit);
                }
            }
                
            
            // If we click on the object with the tag DrawPile we then add a random card to hand
            if (hit.transform.gameObject.CompareTag("DrawPile") && Input.GetMouseButtonDown(0))
                // TODO: The card that we draw is from a predefined List that is shuffled
                AddCardToPlayerHand();
            
            //_objectHit = hit.transform.gameObject;
        }
        else
        {
            // If nothing was hit with the ray then our current "object hit" will be set to null
            _lastObjectHitPersistent = _objectHit;
            _objectHit = null;
            objectHitChanged = true;
        }
        
        // If we hit something else from the last time
        // and what we hovered over last time is a card
        if (objectHitChanged &&
            !(_lastObjectHitPersistent is null) &&
            !(_lastObjectHitPersistent.transform.parent is null) &&
            _lastObjectHitPersistent.transform.parent.CompareTag("PlyHand"))
        {
            // Get the index of that object
            int index = playableCardsObjects.FindIndex(o => o.gameObject == _lastObjectHitPersistent);
            if(index >= 0)
                // Then move the card down
                MoveCardDownInHand(_lastObjectHitPersistent, index);
        }
        
    }
    
    /// <summary>
    /// Move the card to the middle of the screen
    /// </summary>
    /// <param name="cardToMove">Card to be moved</param>
    private void MoveCardToMiddleScreen(GameObject cardToMove)
    {
        // TODO: cut the function to fewer lines
        BoxCollider cardColl = cardToMove.GetComponent<BoxCollider>();
        if (cardColl is null)
        {
            Debug.LogError("This should not be null, where the fuck is the collider");
            return;
        }
        
        cardToMove.transform.localPosition = new Vector3(CardXOffset * (playableCardsObjects.Count / 2) ,CardYMovement, CardZMovement);
        CardObjectLogic cardObjectLogic = cardToMove.GetComponent<CardObjectLogic>();
        
        cardObjectLogic.ShowActionBtns(true);
        Button exitCardBtn = cardObjectLogic.GetExitButton();
        if(!(exitCardBtn is null))
            exitCardBtn.onClick.AddListener(() =>
            {
                _playerCardActions = false;
                int index = playableCardsObjects.FindIndex(o => o.gameObject == cardToMove);
                if(index >= 0)
                    MoveCardDownInHand(cardToMove, index);
            });

        Button playCardBtn = cardObjectLogic.GetPlayButton();
        if (!(playCardBtn is null))
            playCardBtn.onClick.AddListener(() =>
            {
                _playerCardActions = false;
                cardObjectLogic.ShowActionBtns(false);
                
                int index = playableCardsObjects.FindIndex(o => o.gameObject == cardToMove);
                if (index >= 0)
                {
                    gameManager.PlayCard(playableCards[index], cardToMove);
                    RemoveCardFromPlayerHand(playableCards[index], index);
                }
            });
        _playerCardActions = true;
    }

    private void MoveCardUp(GameObject cardToMove)
    {
        // Pull the card up on hovering
        cardToMove.transform.localPosition = new Vector3(cardToMove.transform.localPosition.x, CardYMovement, CardZMovement);

        // Resizing the collider so that flickering doesn't happen anymore
        BoxCollider objectCollider = cardToMove.GetComponent<BoxCollider>();
        objectCollider.center = new Vector3(0, BoxColCentY, 0);
        objectCollider.size = new Vector3(1, BoxColSizeY, 1);
    }

    /// <summary>
    /// Moves the card object back in hand to the position it was
    /// </summary>
    /// <param name="cardToMove">The card object to move</param>
    /// <param name="positionInArray">The index of the position in the array of the object</param>
    private void MoveCardDownInHand(GameObject cardToMove, int positionInArray)
    {
        // Makes sure it hides the action buttons when it gets back in hand
        cardToMove.GetComponent<CardObjectLogic>().ShowActionBtns(false);
        
        // If the object wasn't hit by the raycast, reset the position and the scaling of the collider
        cardToMove.transform.localPosition = new Vector3(CardXOffset * positionInArray, positionInArray * CardYOffset , 0);
        
        // Reducing the amount of calls for "GetComponent", expensive operation
        BoxCollider objectCollider = cardToMove.GetComponent<BoxCollider>();
        objectCollider.center = new Vector3(0, 0, 0);
        objectCollider.size = new Vector3(1, 1, 1);
        
    }

    /// <summary>
    /// Adds a card to the player hand
    /// </summary>
    public void AddCardToPlayerHand()
    {
        // TODO: This should not be called here!
        Tuple<PlayableCard, GameObject> tuple = gameManager.DrawFromDrawPile();
        
        PlayableCard card = tuple.Item1;
        GameObject instantiatedCard = tuple.Item2;
        
        instantiatedCard.transform.SetParent(handTransform, false);
        
        // Add the info into the list of cards
        playableCards.Add(card);
        
        // Set the position to 0 for the moment
        instantiatedCard.transform.localPosition = new Vector3(0, 0, 0); 
        instantiatedCard.transform.localRotation = Quaternion.Euler(80,180,-0.3f);
            
        // If it's not the first card position it accordingly
        if(playableCardsObjects.Count > 0)
        {
            Vector3 lastCardPos = playableCardsObjects[playableCardsObjects.Count - 1].transform.localPosition;
            instantiatedCard.transform.localPosition = lastCardPos + new Vector3(CardXOffset, CardYOffset, 0);
            handTransform.position += new Vector3(HandXOffset, 0, 0);
        }
        
        // Add it to the list of card gameobjects
        playableCardsObjects.Add(instantiatedCard);
        
        // Apply the card info to the canvas of the card
        instantiatedCard.GetComponent<CardObjectLogic>().ApplyCardInfo(card);
    }

    private void RefreshObjectPositions()
    {
        if (playableCardsObjects.Count <= 0)
            return;
        
        playableCardsObjects[0].transform.localPosition = Vector3.zero;
        
        for (int index = 1; index < playableCardsObjects.Count; index++)
        {
            GameObject cardObject = playableCardsObjects[index];
            Vector3 lastPos = playableCardsObjects[index - 1].transform.localPosition;
            
            cardObject.transform.localPosition = lastPos + new Vector3(CardXOffset, CardYOffset, 0);
        }
        
        handTransform.localPosition =
            handTransformStartPos + new Vector3(HandXOffset * (playableCardsObjects.Count - 1), 0, 0);
    }

    private void RemoveCardFromPlayerHand(PlayableCard cardToRemove, int index = -1)
    {
        if (cardToRemove is null) return;

        if(index == -1)
            index = playableCards.FindIndex(c=> c == cardToRemove);

        if (index < 0)
        {
            Debug.LogError("The card to be removed has not been found");
            return;
        }
        
        handTransform.position -= new Vector3(HandXOffset, 0, 0);
        
        // Remove the card obj from the hand list
        playableCardsObjects.Remove(playableCardsObjects[index]);
        // Remove the card logic from the hand list
        playableCards.Remove(cardToRemove);

        RefreshObjectPositions();
    }

    /// <summary>
    /// RPC function that disables the camera if the player object is not ours
    /// </summary>
    [PunRPC]
    public void RPC_DisableCamera()
    {
        // For some reason the _photonView is not cached on Start (maybe putting it OnEnable/Awake will change something)
        // But getting the view again fixes it
        _photonView = gameObject.GetComponent<PhotonView>();
        
        if(_photonView.IsMine)
            return;
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
