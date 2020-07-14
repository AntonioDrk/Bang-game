using System;
using System.Collections.Generic;
using System.Diagnostics;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
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
[RequireComponent(typeof(PlayerLogic))]
public class PlayerManager : MonoBehaviour, IPunInstantiateMagicCallback
{
    // CONSTANTS
    private const float HandXOffset = -0.07f;
    private const float CardXOffset = -0.15f;
    private const float CardYOffset = -0.007f;

    private const float CardYMovement = 0.1f;
    private const float CardZMovement = -0.6f;

    private const float BoxColCentY = -.375f;
    private const float BoxColSizeY = 1.75f;
    
    private const float CardSizeIncrease = 0.2f; // Percentage size increase of the card when it's highlighted

    private const float CardMiddleMovementIncrease = 0.4f;
    // ----------------------------------

    private Vector3 handTransformStartPos;
    
    [SerializeField]
    private Camera _mainCamera;
    
    private List<PlayableCard> playableCards = new List<PlayableCard>();
    private List<GameObject> playableCardsObjects = new List<GameObject>();
 
    private bool _playerCardActions = false;
    private bool _isMyTurn = false;
    private bool _drawnTurnCards = false;
    private bool _targeting = false;
    private int _cardToBePlayedIndex;
    private int _numberOfCardsToDraw = 2;
    private int _numberOfCardsInHand = 0;
    
    
    private GameManager gameManager;
    private GameCanvasManager canvasManager;
    private PlayerLogic playerLogic;
    [SerializeField] private TargetingStack targetingStack;

    [SerializeField] private Transform handTransform;

    [SerializeField] private GameObject playerBoardCanvas;

    private GameObject _lastObjectHitPersistent;
    private GameObject _objectHit;
    
    private PhotonView _photonView;
    
    private void Awake()
    {
        GameSetup.instance.NumberOfPlayer++;
        StartChecks();
        Debug.LogWarning("Number of player objects is " + GameSetup.instance.NumberOfPlayer);
    }

    private void Start()
    {
        
        handTransformStartPos = handTransform.localPosition;
        _photonView.RPC("RPC_SetNamePlayerBoard", RpcTarget.AllBuffered, _photonView.Owner.NickName);
        //Debug.LogWarning("Setting up the role card with role " + GetComponent<PlayerLogic>().PlayerRole);
        //_photonView.RPC("RPC_SetRoleCard", RpcTarget.AllBufferedViaServer, GetComponent<PlayerLogic>().PlayerRole);
        
        // Why is this getting called multiple times?
        if (GameSetup.instance.NumberOfPlayer == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            gameManager.DistributeRoles();
            gameManager.DistributeCharacters();
        }
        
        if(_photonView.IsMine)
            canvasManager.AddListenerToEndTurnButton(()=>_photonView.RPC("RPC_On_EndTurn", RpcTarget.AllViaServer));
    }
    
    /// <summary>
    /// Function to tell if it's the players turn or not.
    /// Could be used for checks and initializations at the start of the turn.
    /// </summary>
    public void SetTurn(bool isMyTurn)
    {
        if (isMyTurn)
        {
            _isMyTurn = true;
            _numberOfCardsToDraw = 2;
            _drawnTurnCards = false;
            // Not setting the end turn button here because player needs to draw before he can end his turn
        }
        else
        {
            
            _isMyTurn = false;
            _targeting = false;
            _cardToBePlayedIndex = -1;
            canvasManager.HideInfoMessage();
            canvasManager.ToggleEndTurnBtn(true);
        }
        
    }

    /// <summary>
    /// Called when the player presses the button end turn
    /// </summary>
    [PunRPC]
    private void RPC_On_EndTurn()
    {
        // Check to see how many cards the player has
        if (_numberOfCardsInHand > playerLogic.CurrentLives && _photonView.IsMine)    // If more than the number of current lives
        {
            // Make the player discard until the number it's equal with the cards in hand
            canvasManager.InfoMessageDiscardCard(_numberOfCardsInHand - playerLogic.CurrentLives);
        }
        else
        {
            Debug.Log("Going next turn, this player has " + _numberOfCardsInHand + " cards in hand and " + playerLogic.CurrentLives + " lives");
            //gameManager.GetComponent<PhotonView>().RPC("RPC_NextTurn", RpcTarget.AllViaServer);
            gameManager.RPC_NextTurn();
        }
        
    }
    
    /// <summary>
    /// Basic function that wraps all the checks for variables and also sets them (should be called in awake function)
    /// </summary>
    private void StartChecks()
    {
        _photonView = gameObject.GetComponent<PhotonView>();
        if (_photonView == null)
         Debug.LogError("Photon view not found!");

        // This should never be able to be null because of the [RequireComponent]
        playerLogic = GetComponent<PlayerLogic>();
        
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if(gameManager == null)
            Debug.LogError("Game manager couldn't be found!");

        canvasManager = GameObject.FindGameObjectWithTag("CanvasManagerGame").GetComponent<GameCanvasManager>();
        
        if(targetingStack == null)
            Debug.LogError("Targeting stack probably isn't linked up in the inspector!");
        
        if(canvasManager == null)
            Debug.LogError("CanvasManagerGame probably isn't in scene or doesn't have the tag \"CanvasManagerGame\"");
        
        if(playerBoardCanvas == null)
            Debug.LogError("Player board canvas isn't set, make sure you set it in the inspector!");
    }

    private void Update()
    {
        //TODO: REFACTOR Split the functionality for the love of GOD
        
        if ( _playerCardActions) return;
        if(_photonView.IsMine == false) return;

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
            
            // Check if we have to choose a player as target
            if (_targeting)
            {
                // Did we select a player ?
                if (Input.GetMouseButtonDown(0) && hit.transform.CompareTag("Player"))
                {
                    GameObject playerObject = hit.transform.gameObject;
                    
                    Debug.LogWarning("Selected the player " + playerObject.GetComponent<PhotonView>().Owner.NickName);
                    // Get the seat of the player
                    int seatNumberOfTarget =
                        GameSetup.instance.GetPlayerSeatWithActorNumber(playerObject.GetComponent<PhotonView>().Owner
                            .ActorNumber);
                    
                    _photonView.RPC("RPC_PlayCard", RpcTarget.AllViaServer, _cardToBePlayedIndex, seatNumberOfTarget);
                    canvasManager.HideInfoMessage();
                }
            }
            

            // Is it my turn ?!
            if (_isMyTurn)
            {
                // IF WE DIDN'T DRAW OUR FIRST PHASE CARDS
                if (_drawnTurnCards == false)
                {
                    // Tell the player how many cards he still has to draw
                    canvasManager.InfoMessageDrawCard(_numberOfCardsToDraw);

                    // If we click on the object with the tag DrawPile we then add a random card to hand
                    if (hit.transform.gameObject.CompareTag("DrawPile") && Input.GetMouseButtonDown(0))
                    {
                        //TODO: Check if he can hold any more cards
                        _photonView.RPC("RPC_AddCardToPlayerHand", RpcTarget.All);
                        // Number of required cards to draw goes down
                        _numberOfCardsToDraw -= 1;
                        // When we drew the amount necessary stop letting the player draw
                        if (_numberOfCardsToDraw <= 0)
                        {
                            _drawnTurnCards = true;
                            // Hide the info messaging
                            canvasManager.HideInfoMessage();
                            
                            // Display the end turn button
                            canvasManager.ToggleEndTurnBtn(false);
                        }
                    }
                }
            }
            
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
            if (index >= 0 && index < playableCardsObjects.Count)
            {
                // Then move the card down
                MoveCardDownInHand(_lastObjectHitPersistent, index);
                _lastObjectHitPersistent.GetComponent<CardObjectLogic>().ActivateOutline(false);
            }
        }
    }
    
    /// <summary>
    /// Function that executes the movement of the card, the action of the card
    /// </summary>
    /// <param name="index">Index of the card that's getting played</param>
    /// <param name="playerSeat">The seat of the targeted player</param>
    [PunRPC]
    private void RPC_PlayCard(int index, int playerSeat)
    {
        //TODO: Checks on targeting should be done in the update
        ActionCard cardToBePlayed = (ActionCard)playableCards[index];
        if (cardToBePlayed.Target == Target.AnyPlayer
            || cardToBePlayed.Target == Target.RangeOne
            || cardToBePlayed.Target == Target.InWeaponRange)
        {
            Debug.LogWarning("Testing if it's our player board");
            if (playerSeat != GameSetup.instance.GetPlayerSeatWithActorNumber(_photonView.Owner.ActorNumber))
            {
                if(playableCards[index] == null)
                    Debug.LogError("The logic of the card at index " + index + " cannot be found in the array");
                
                gameManager.MoveCardObjectInFrontOfPlayer(playableCardsObjects[index], (ActionCard)playableCards[index], playerSeat);
                
                RPC_RemoveCardFromPlayerHand(index);
            }
        }
        // If the card that has been played needs to be targeted on yourself
        // Check if you were the target
        if (cardToBePlayed.Target == Target.Yourself &&
            playerSeat == GameSetup.instance.GetPlayerSeatWithActorNumber(_photonView.Owner.ActorNumber))
        {
            // TODO: Change the logic?
            switch (cardToBePlayed.Effect)
            {
                // For the missed effect we need to have an extra check
                // Because it can't be played when a bang card isn't played
                case Effect.Missed:
                    if (targetingStack.IsBangFirstCard())
                    {
                        gameManager.MoveCardObjectInFrontOfPlayer(playableCardsObjects[index], (ActionCard)playableCards[index], playerSeat);
                
                        RPC_RemoveCardFromPlayerHand(index);
                    }
                    break;
                default:
                    gameManager.MoveCardObjectInFrontOfPlayer(playableCardsObjects[index], (ActionCard)playableCards[index], playerSeat);
                    RPC_RemoveCardFromPlayerHand(index);
                    break;
            }
        }
        
    }
    
    /// <summary>
    /// Exits the targeting state
    /// </summary>
    private void DisableTargeting()
    {
        _targeting = false;
        _cardToBePlayedIndex = -1;
        // TODO: Make the end turn button visible back
    }

    /// <summary>
    /// Move the card to the middle of the screen and present the player two buttons, one to play the card,
    /// one to put the card back in hand
    /// </summary>
    /// <param name="cardToMove">Card to be moved</param>
    private void MoveCardToMiddleScreen(GameObject cardToMove)
    {
        // Check to see if the card we're moving to the middle is ours
        if(cardToMove.GetComponent<PhotonView>().IsMine == false)
            return;
        
        Debug.LogWarning("Moving card to the middle of the screen for client " + _photonView.Owner.ActorNumber);

        // TODO: cut the function to fewer lines
        BoxCollider cardColl = cardToMove.GetComponent<BoxCollider>();
        if (cardColl is null)
        {
            Debug.LogError("This should not be null, where the fuck is the collider");
            return;
        }
        
        cardToMove.transform.localPosition = new Vector3(CardXOffset * (playableCardsObjects.Count / 2f) ,
            CardYMovement + CardYMovement * CardMiddleMovementIncrease, CardZMovement + CardZMovement * CardMiddleMovementIncrease);
        
        
        CardObjectLogic cardObjectLogic = cardToMove.GetComponent<CardObjectLogic>();
        
        int index = playableCardsObjects.FindIndex(o => o.gameObject == cardToMove);
        
        cardObjectLogic.ShowActionBtns(true);
        
        cardObjectLogic.GetDiscardButton().onClick.RemoveAllListeners();
        cardObjectLogic.GetExitButton().onClick.RemoveAllListeners();
        cardObjectLogic.GetPlayButton().onClick.RemoveAllListeners();
        
        Button exitCardBtn = cardObjectLogic.GetExitButton();
        if(exitCardBtn is null == false)
            exitCardBtn.onClick.AddListener(() =>
            {
                //_photonView.RPC("RPC_OnCancelBtnPressed", RpcTarget.All, index);
                RPC_OnCancelBtnPressed(index);
            });

        // If it's our turn, only then let the player play a card
        // Or if we're getting attacked with a bang card
        Button playCardBtn = cardObjectLogic.GetPlayButton();
        switch (((ActionCard) playableCards[index]).Effect)
        {
            // If the card we're looking at now, is a Missed card we can play it
            case Effect.Missed:
                if (targetingStack.IsBangFirstCard())
                {
                    if (playCardBtn is null == false)
                        playCardBtn.onClick.AddListener(() =>
                        {
                            RPC_OnPlayBtnPressed(index);
                            //_photonView.RPC("RPC_OnPlayBtnPressed", RpcTarget.All, index);
                        });
                }
                break;
            default:
                if (_isMyTurn)
                {
                    if (playCardBtn is null == false)
                        playCardBtn.onClick.AddListener(() =>
                        {
                            RPC_OnPlayBtnPressed(index);
                        });
                }
                break;
        }

        if (_isMyTurn)
        {
            Button discardCardBtn = cardObjectLogic.GetDiscardButton();
            if (discardCardBtn is null == false)
                discardCardBtn.onClick.AddListener(() =>
                {
                    _photonView.RPC("RPC_OnDiscardBtnPressed", RpcTarget.All, index);
                });
        }
        
        
        _playerCardActions = true;
    }

    /// <summary>
    /// Function that gets called as an RPC when one of the players plays their card
    /// </summary>
    /// <param name="index">Index of the card about to be played in the playableCards vector</param>
    private void RPC_OnPlayBtnPressed(int index)
    {
        if (index >= 0 && index < playableCardsObjects.Count)
        {
            // Cache the card that's about to be played
            _cardToBePlayedIndex = index;

            _playerCardActions = false;
            
            // Move the card down back in hand 
            MoveCardDownInHand(playableCardsObjects[index], index);
            
            // TODO: Change the End Turn button to a "Cancel" button to cancel the action, it would reset the _cardToBePlayed and _targeting to false
            
            // Info message that the player needs to select a target or cancel the action
            canvasManager.InfoActionCard();
            
            // Make the player select target according to the played card
            _targeting = true;
            
            // After he selects the card, rpc the effect of the card to all players
        }
        else
            Debug.LogError("Card to play wasn't found in the playableCards vector");
        
    }

    /// <summary>
    /// Function that gets called once the cancel button on a card is pressed.
    /// It calls back the MoveCardDown function.
    /// </summary>
    /// <param name="index">The index of the card</param>
    [PunRPC]
    private void RPC_OnCancelBtnPressed(int index)
    {
        _playerCardActions = false;
                
        if(index >= 0 && index < playableCardsObjects.Count)
            MoveCardDownInHand(playableCardsObjects[index], index);
        else
            Debug.LogError("Card to play wasn't found in the playableCards vector");
    }

    /// <summary>
    /// Function that gets called as an RPC when one of the players discards their card
    /// </summary>
    /// <param name="index">Index of the card about to be discarded in the playableCards vector</param>
    [PunRPC]
    private void RPC_OnDiscardBtnPressed(int index)
    {
        _playerCardActions = false;
        if (index >= 0 && index < playableCardsObjects.Count)
        {
            if(playableCardsObjects[index].GetComponent<CardObjectLogic>().isUpInHand)
                DecreaseCardSize(playableCardsObjects[index]);
            
            playableCardsObjects[index].GetComponent<CardObjectLogic>().ShowActionBtns(false);
            gameManager.MoveCardToDiscardPile(playableCards[index], playableCardsObjects[index]);
            //RPC_RemoveCardFromPlayerHand(playableCards[index], index);
            // Remove the card from this player on all clients
            //_photonView.RPC("RPC_RemoveCardFromPlayerHand", RpcTarget.All, index);
            RPC_RemoveCardFromPlayerHand(index);
        }
    }
    
    private void MoveCardUp(GameObject cardToMove)
    {
        // Check to see if the card we're moving up is ours
        if(cardToMove.GetComponent<PhotonView>().IsMine == false || 
           cardToMove.GetComponent<CardObjectLogic>().isUpInHand)
            return;
        // Pull the card up on hovering
        cardToMove.transform.localPosition = new Vector3(cardToMove.transform.localPosition.x, CardYMovement, CardZMovement);
        IncreaseCardSize(cardToMove);
        
        cardToMove.GetComponent<CardObjectLogic>().ActivateOutline(true);
        cardToMove.GetComponent<CardObjectLogic>().isUpInHand = true;
        
        // Resizing the collider so that flickering doesn't happen anymore
        BoxCollider objectCollider = cardToMove.GetComponent<BoxCollider>();
        objectCollider.center = new Vector3(0, BoxColCentY, 0);
        objectCollider.size = new Vector3(1, BoxColSizeY, 1);
    }

    /// <summary>
    /// Increases the scale of the card on X,Y axis by 'CardSizeIncrease' percent
    /// </summary>
    /// <param name="cardToIncrease">The card gameobject to resize</param>
    private void IncreaseCardSize(GameObject cardToIncrease)
    {
        Vector3 originalScale = cardToIncrease.transform.localScale;
        cardToIncrease.transform.localScale = new Vector3(originalScale.x * CardSizeIncrease + originalScale.x,
            originalScale.y * CardSizeIncrease + originalScale.y, originalScale.z);
    }

    /// <summary>
    /// Decreases the scale of the card on X,Y axis by the same amount it was increased by 'CardSizeIncrease'
    /// </summary>
    /// <param name="cardToDecrease">The card gameobject to resize</param>
    private void DecreaseCardSize(GameObject cardToDecrease)
    {
        Vector3 originalScale = cardToDecrease.transform.localScale;
        // Recalculate the percent for decreasing the size to the original size (because 0.2 increase of 1 is a total of 1.2)
        float CardSizeDecrease = CardSizeIncrease / (1 + CardSizeIncrease);
        cardToDecrease.transform.localScale = new Vector3(originalScale.x - originalScale.x * CardSizeDecrease,
            originalScale.y - originalScale.y * CardSizeDecrease, originalScale.z);
    }


    /// <summary>
    /// Moves the card object back in hand to the position it was
    /// </summary>
    /// <param name="cardToMove">The card object to move</param>
    /// <param name="positionInArray">The index of the position in the array of the object</param>
    private void MoveCardDownInHand(GameObject cardToMove, int positionInArray)
    {
        // Check to see if the card we're moving down is ours
        if(cardToMove.GetComponent<PhotonView>().IsMine == false || cardToMove.GetComponent<CardObjectLogic>().isUpInHand == false)
            return;
        
        // Makes sure it hides the action buttons when it gets back in hand
        cardToMove.GetComponent<CardObjectLogic>().ShowActionBtns(false);
        cardToMove.GetComponent<CardObjectLogic>().isUpInHand = false;
        
        DecreaseCardSize(cardToMove);
        
        // If the object wasn't hit by the raycast, reset the position and the scaling of the collider
        cardToMove.transform.localPosition = new Vector3(CardXOffset * positionInArray, 
            positionInArray * CardYOffset , 0);
        
        // Reducing the amount of calls for "GetComponent", expensive operation
        BoxCollider objectCollider = cardToMove.GetComponent<BoxCollider>();
        objectCollider.center = new Vector3(0, 0, 0);
        objectCollider.size = new Vector3(1, 1, 1);
        
    }

    [PunRPC]
    public void RPC_SetNamePlayerBoard(string value)
    {
        // Gets the textmeshpro of the title of the player board
        playerBoardCanvas.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = value;
    }
    
    [PunRPC]
    public void RPC_SetRoleCard(Role role)
    {
        if (_photonView.IsMine || role == Role.Sheriff)
        {
            Debug.LogWarning("Setting the role text of player " + _photonView.Owner.NickName);
            // If it's our client then display the role (or if it's the sheriff display it)
            GameObject roleCardBack = playerBoardCanvas.transform.GetChild(4).gameObject;
            if(roleCardBack == null)
                Debug.LogError("COULDN'T FIND THE ROLECARDBACK, CHECK THE CHILD COUNT");
            else
                roleCardBack.SetActive(false);

            string temp = "";
            switch (role)
            {
                case Role.Deputy:
                    temp = "Deputy";
                    break;
                case Role.Outlaw:
                    temp = "Outlaw";
                    break;
                case Role.Renegade:
                    temp = "Renegade";
                    break;
                case Role.Sheriff:
                    temp = "Sheriff";
                    break;
                default:
                    temp = "None";
                    break;
            }
            Debug.LogWarning("Setting the text to " + temp);
            playerBoardCanvas.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = temp;
        }
    }

    public void SetCharacterCard()
    {
        Debug.LogWarning("Setting up character card");
        CharacterCard chCard = GetComponent<PlayerLogic>().CharCard;
        if (chCard == null)
        {
            Debug.LogError("Character card wasn't set and we're trying to access it!");
            return;
        }
        
        // Title of the card
        playerBoardCanvas.transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = chCard.Title;
        
        // Description of the card
        playerBoardCanvas.transform.GetChild(5).GetChild(1).GetComponent<TextMeshProUGUI>().text = chCard.Description;

        UpdateLivesText();

    }

    /// <summary>
    /// Updates the number of lives the player has
    /// </summary>
    public void UpdateLivesText()
    {
        // Check to see if the player is actually LOCAL
        // then change the visible Canvas
        if (_photonView.IsMine)
        {
            canvasManager.SetLives(playerLogic.CurrentLives);
        }
    }

    public void DrawStartCards()
    {
        // This should usually be called inside a IsMine check but let's be sure about that
        if(!_photonView.IsMine)
            return;
        
        Debug.LogWarning("Giving player " + _photonView.Owner.NickName + " " + GetComponent<PlayerLogic>().CurrentLives + " cards");
        
        for (int i = 0; i < GetComponent<PlayerLogic>().CurrentLives; i++)
        {
            _photonView.RPC("RPC_AddCardToPlayerHand", RpcTarget.AllViaServer);
        }
    }
    
    
    /// <summary>
    /// Adds a card to the player hand
    /// </summary>
    [PunRPC]
    public void RPC_AddCardToPlayerHand()
    {
        // TODO: This should not be called here!
        Tuple<PlayableCard, GameObject> tuple = gameManager.DrawFromDrawPile();
        
        PlayableCard card = tuple.Item1;
        GameObject instantiatedCard = tuple.Item2;
        
        // Counter for number of cards
        _numberOfCardsInHand++;
        
        // Transfering the ownership of the card to the player who drew the card
        instantiatedCard.GetComponent<PhotonView>().TransferOwnership(_photonView.Owner);
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
            handTransform.localPosition += new Vector3(HandXOffset, 0, 0);
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

    /// <summary>
    /// Removes the cached card from the player
    /// </summary>
    /// <param name="index">It's index in the vector</param>
    [PunRPC]
    private void RPC_RemoveCardFromPlayerHand(int index)
    {
        if (index < 0 || index >= playableCards.Count)
        {
            Debug.LogError("Index out of bounds");
            return;
        }

        PlayableCard cardToRemove = playableCards[index];

        if (cardToRemove == null)
        {
            Debug.LogError("No card to remove was found");
            return;
        }

        handTransform.position -= new Vector3(HandXOffset, 0, 0);
        
        // Counter for the number of cards
        _numberOfCardsInHand--;
        if (_numberOfCardsInHand <= 0)
            _numberOfCardsInHand = 0;

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
    
    /// <summary>
    /// Called when the gameobject successfully instantiates
    /// </summary>
    /// <param name="info"></param>
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Transform temp = GameObject.FindGameObjectWithTag("PlayerObjects").transform;
        if(temp == null)
            Debug.LogError("There isn't any object tagged with \'PlayerObjects\', " +
                           "this is made so all the players are grouped under one object on each client");
        gameObject.transform.SetParent(temp);
        
    }
}
