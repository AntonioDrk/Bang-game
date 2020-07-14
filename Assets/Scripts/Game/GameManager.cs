using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Cards;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private string _jsonLoadPath = "MetaData/Json";

    [SerializeField] private Transform discardPileContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform drawPileContainer;
    [SerializeField] private int numberOfDrawCards;
    [SerializeField] private PhotonView _photonView;
    [SerializeField] private int _generatedSeed;
    [SerializeField] private GameSetup _gameSetup;
    [SerializeField] private GameCanvasManager canvasManager; 

    private CharacterCard[] characterCards;
    private ActionCard[] actionCards;
    private WeaponCard[] weaponCards;

    private Role[] rolesForSeats;
    private bool rolesDistributed = false;

    private CharacterCard[] characterCardsPerSeat;
    private bool charactersDistributed = false;


    private List<PlayableCard> drawPileCards = new List<PlayableCard>();
    private List<GameObject> drawPileObjects = new List<GameObject>();
    private List<PlayableCard> discardPileCards = new List<PlayableCard>();
    private List<GameObject> discardPileObjects = new List<GameObject>();

    private int currentTurn = 0;


    /// <summary>
    /// Loads the character cards from the "characters" json file
    /// </summary>
    private void LoadCharacterCards()
    {
        TextAsset data = Resources.Load(_jsonLoadPath + "/characters") as TextAsset;
        if (data == null)
            Debug.LogError("There's no json file named \"characters\" in the Json folder");
        else
            characterCards = JsonHelper.getJsonArray<CharacterCard>(data.text);
    }

    private void LoadActionCards()
    {
        TextAsset data = Resources.Load(_jsonLoadPath + "/ActionCards") as TextAsset;
        if (data == null)
        {
            Debug.LogError("There's no json file named \"ActionCards\" in the Json folder");
            return;
        }

        actionCards = JsonHelper.getJsonArray<ActionCard>(data.text);
        for (int i = 0; i < actionCards.Length; i++)
        {
            Debug.Log(actionCards[i]);
        }
    }

    private void LoadWeaponCards()
    {
        //TODO: Load from a json the different types of weapon cards
    }

    /// <summary>
    /// Loads the distribution of cards (title + amount)
    /// </summary>
    /// <returns>The vector with distribution</returns>
    /// <exception cref="Exception">Exception raised when the json file isn't found or it can't be accessed, resulting in a null vector being passed around</exception>
    private CardDistribution[] LoadCardDistribution()
    {
        CardDistribution[] cardDistributions;
        TextAsset data = Resources.Load(_jsonLoadPath + "/CardsDistribution") as TextAsset;
        if (data == null)
        {
            throw new Exception("There's no json file named \"CardsDistribution\" in the Json folder");
        }

        cardDistributions = JsonHelper.getJsonArray<CardDistribution>(data.text);

        return cardDistributions;
    }

    /// <summary>
    /// Distributes the roles based on the seats
    /// </summary>
    public void DistributeRoles()
    {
        if(rolesDistributed)
            return;

        // Wait for the instantiation of the game setup
        int numberPlayers = _gameSetup.GetInstance().NumberOfOccupiedSeats();
        rolesForSeats = new Role[numberPlayers];

        // Basic start config for 4 players
        List<Role> config = new List<Role>();
        config.Add(Role.Sheriff);
        config.Add(Role.Outlaw);
        config.Add(Role.Outlaw);
        config.Add(Role.Renegade);

        // The different configurations based on the number of players
        switch (numberPlayers)
        {
            case 4:
                break;
            case 5:
                config.Add(Role.Deputy);
                break;
            case 6:
                config.Add(Role.Deputy);
                config.Add(Role.Outlaw);
                break;
            case 7:
                config.Add(Role.Deputy);
                config.Add(Role.Deputy);
                config.Add(Role.Outlaw);
                break;
            default:
                Debug.LogError("Number of players not in the configuration! Something went wrong");
                break;
        }

        // Setting the roles randomly per seat
        // (on index 0 meaning the first seat is a random role and so on...)
        Debug.Log("Assigning roles...");
        Debug.LogWarning("Number of players is " + numberPlayers);
        for (int i = 0; i < numberPlayers; i++)
        {
            int roleIndex = Random.Range(0, config.Count - 1);
            rolesForSeats[i] = config[roleIndex];
            config.RemoveAt(roleIndex);
            Debug.LogWarning("Player at seat " + i + " has role " + rolesForSeats[i]);
            // Sets the logic of the player
            GameObject playerObject = GameSetup.instance.GetPlayerObjectAtSeat(i);
            //TODO: There are still errors around here
            if(playerObject == null)
                Debug.LogError("Couldn't find the player object at seat " + i);
            else
            {
                playerObject.GetComponent<PlayerLogic>().PlayerRole = rolesForSeats[i];
                //playerObject.GetComponent<PhotonView>().RPC("RPC_SetRoleCard", RpcTarget.AllBufferedViaServer, rolesForSeats[i]);
                playerObject.GetComponent<PlayerManager>().RPC_SetRoleCard(rolesForSeats[i]);
            }
                
        }

        rolesDistributed = true;
    }

    /// <summary>
    /// Function that distributes the character cards to all player objects
    /// and also tells them to set up the display of them
    /// </summary>
    public void DistributeCharacters()
    {
        if(charactersDistributed)
            return;
        
        characterCardsPerSeat = new CharacterCard[GameSetup.instance.NumberOfPlayer];
        Debug.LogWarning("Distributing the character cards to " + GameSetup.instance.NumberOfPlayer + " players");
        for (int i = 0; i < GameSetup.instance.NumberOfPlayer; i++)
        {
            int nrLives = 4;
            if (Random.Range(0f, 1f) <= 0.15f)
                nrLives = 3;
            // TODO: Here you should pick a random already loaded character card
            CharacterCard chCard = new CharacterCard(CharacterActions.None, "Test Character", 
                "This is a test character card, it has a 15% chance of having 3 lives instead of 4.", (uint)nrLives);

            characterCardsPerSeat[i] = chCard;
            
            GameObject playerObject = GameSetup.instance.GetPlayerObjectAtSeat(i);
            playerObject.GetComponent<PlayerLogic>().CharCard = chCard;
            
            // Sheriff gets one permanent life in addition
            // the turn starts with him
            if (rolesForSeats[i] == Role.Sheriff)
            {
                nrLives += 1;
                currentTurn = i;
                playerObject.GetComponent<PlayerManager>().SetTurn(true);
                
                if(PhotonNetwork.LocalPlayer.NickName == GetPlayerNameTurn())
                    canvasManager.SetTurnText("YOURS");
                else
                    canvasManager.SetTurnText(GetPlayerNameTurn());
            }
                
            
            playerObject.GetComponent<PlayerLogic>().SetLives(nrLives);
            playerObject.GetComponent<PlayerManager>().SetCharacterCard();
            playerObject.GetComponent<PlayerManager>().DrawStartCards();
        }

        charactersDistributed = true;
    }

    [PunRPC]
    public void RPC_NextTurn()
    {
        ApplyCardsToPlayers();
        
        GameSetup.instance.GetPlayerObjectAtSeat(currentTurn).GetComponent<PlayerManager>().SetTurn(false);
        
        currentTurn++;
        currentTurn %= GameSetup.instance.playerSeats.Length;
        
        GameSetup.instance.GetPlayerObjectAtSeat(currentTurn).GetComponent<PlayerManager>().SetTurn(true);
        
        // Check if the we're the local player and it's our turn
        // If it is then show YOURS otherwise, from a different
        //  client show who's turn it is
        if(PhotonNetwork.LocalPlayer.NickName == GetPlayerNameTurn())
            canvasManager.SetTurnText("YOURS");
        else
            canvasManager.SetTurnText(GetPlayerNameTurn());
    }

    /// <summary>
    /// Function that generates the proper amount of cards of each type into the draw pile
    /// It also assigns the suite and the numbers to each card
    /// </summary>
    private void GeneratePlayableCards()
    {
        // Frequency vectors to keep track of what numbers with what suit are already taken;
        // [0,...] hearts
        // [1,...] diamonds
        // [2,...] spades
        // [3,...] clubs
        // bool [,] suitsNumbers = new bool[4,14];
        //
        // for (int i = 0; i < 4; i++)
        //     for(int j = 1; j <= 13; j++)
        //          suitsNumbers[i,j] = false;
        uint currNum = 1;
        CardSuit currSuit = 0;

        CardDistribution[] cardDistributions = LoadCardDistribution();
        
        // For each card distribution information
        for (int i = 0; i < cardDistributions.Length; i++)
        {
            CardDistribution cd = cardDistributions[i];
            PlayableCard playableCard = null;
            switch (cd.cardType)
            {
                case CardType.action:
                {
                    // Finds the index of the given title card
                    ActionCard ac = actionCards.First(c => c.Title == cd.Title);
                    
                    if (ac == null)
                    {
                        Debug.LogError("There's a typo in the title of one of the cards in the CardsDistribution json (" + cd.Title + ")");
                        continue;
                    }
                    
                    // Generate in the draw pile the copy card 
                    for (int j = 0; j < cd.Amount; j++)
                    {
                        ActionCard copyCard = new ActionCard(ac) {Number = currNum, Suit = currSuit};
                        
                        currNum++;
                        if (currNum > 13)
                        {
                            currSuit++;
                            currNum = 1;
                        }
                
                        GenerateDrawCard(copyCard);
                    }
                    break;
                }
                case CardType.weapon:
                {
                    // Finds the index of the given title card
                    WeaponCard wc = weaponCards.First(c => c.Title == cd.Title);
                    
                    if (wc == null)
                    {
                        Debug.LogError("There's a typo in the title of one of the cards in the CardsDistribution json (" + cd.Title + ")");
                        continue;
                    }

                    for (int j = 0; j < cd.Amount; j++)
                    {
                        WeaponCard copyCard = new WeaponCard(wc) {Number = currNum, Suit = currSuit};

                        currNum++;
                        if (currNum > 13)
                        {
                            currSuit++;
                            currNum = 1;
                        }
                
                        GenerateDrawCard(copyCard);
                    }
                    break;
                }
            }
        }
    }

    private void GenerateDrawCard(PlayableCard card)
    {
        GameObject cardObject = Instantiate(cardPrefab);

        drawPileCards.Add(card);
        drawPileObjects.Add(cardObject);
        
        MoveCardObjectToDrawPile(cardObject, drawPileCards.Count - 1);
    }
    
    // Early testing function, not really used anymore
    // TODO: Remove this
    private void GenerateRandomDrawCards(int numberOfCards)
    {
        for (int i = 0; i < numberOfCards; i++)
        {
            PlayableCard card = CardGeneration.GenerateRandomPlayableCard();
            GameObject cardObject = Instantiate(cardPrefab);
            
            drawPileCards.Add(card);
            drawPileObjects.Add(cardObject);

            MoveCardObjectToDrawPile(cardObject, i);
        }
    }

    /// <summary>
    /// Helper function to move the gameObjects physically to the draw pile
    /// </summary>
    /// <param name="cardObject">GameObject of the card to be moved</param>
    /// <param name="posInArray">It's position in the DrawPile vector</param>
    private void MoveCardObjectToDrawPile(GameObject cardObject, int posInArray)
    {
        cardObject.transform.SetParent(drawPileContainer, false);
        cardObject.transform.localPosition = Vector3.zero;
        cardObject.transform.localRotation = Quaternion.Euler(270,0,0);
        if (drawPileObjects.Count > 0)
        {
            cardObject.transform.localPosition = new Vector3(0, 0.0015f * posInArray, 0);
        }
        
        float cardHeight = cardObject.GetComponent<MeshRenderer>().bounds.size.y;
        ResizeDrawCollider(cardHeight);
    }
    
    /// <summary>
    /// Used when a card is played, it moves the card object to the selected player area.
    /// It is called from an RPC, so this runs on all clients.
    /// </summary>
    /// <param name="cardObject">The card object</param>
    /// <param name="cardLogic">The card logic</param>
    /// <param name="targetSeat">The seat of the player which is targeted</param>
    public void MoveCardObjectInFrontOfPlayer(GameObject cardObject, ActionCard cardLogic ,int targetSeat)
    {
        Debug.Log("Moving the card to the player spot");
        
        cardObject.GetComponent<PhotonView>().TransferOwnership(0);

        // The transform of the "CardTargeting" container
        Transform cardTargeting = GameSetup.instance.GetPlayerObjectAtSeat(targetSeat).transform.GetChild(4);
        
        TargetingStack targetingStack = cardTargeting.gameObject.GetComponent<TargetingStack>();
        if(targetingStack == null)
            Debug.LogError("TargetingStack script missing from targeting stack object");
        
        cardObject.transform.SetParent(cardTargeting);
        cardObject.transform.localEulerAngles = new Vector3(80, 180, -0.3f);
        cardObject.transform.localPosition = new Vector3(0, 0.0023f * targetingStack.GetNumberOfCards(), 0);

        // RESIZING COLLIDER
        // amnountResized represents the amount it increments each time
        float amountResized = 0.004f;
        BoxCollider cardTargetingColl = GameSetup.instance.GetPlayerObjectAtSeat(targetSeat).GetComponent<BoxCollider>();
        
        // Resizing the colider
        cardTargetingColl.size = new Vector3(cardTargetingColl.size.x, cardTargetingColl.size.y + amountResized, cardTargetingColl.size.z);
        Vector3 origCenter = cardTargetingColl.center;
        // Re-centering
        cardTargetingColl.center = new Vector3(origCenter.x, origCenter.y + amountResized/2f, origCenter.z);

        targetingStack.AddCardToStack(cardObject, cardLogic);
        
        // TODO: Give that player the possibility to defend themselves if it's a bang card
        switch (cardLogic.Effect)
        {
            case Effect.Bang:
                // LOGIC FOR PLAYING A BANG CARD
                break;
            case Effect.Missed:
                break;
            case Effect.DiscardOne:
                break;
            case Effect.DrawOne:
                break;
            default:
                Debug.LogError("Tried to play something that I don't know about...");
                break;
        }
    }

    /// <summary>
    /// Loops through all the players and applies each cards action
    /// </summary>
    public void ApplyCardsToPlayers()
    {
        for (int i = 0; i < GameSetup.instance.NumberOfPlayer; i++)
        {
            GameObject playerObject = GameSetup.instance.GetPlayerObjectAtSeat(i);
            TargetingStack targetingStack = playerObject.transform.GetChild(4).GetComponent<TargetingStack>();

            int bangCards = targetingStack.NumberOfCardsOfEffect(Effect.Bang);
            int missedCards = targetingStack.NumberOfCardsOfEffect(Effect.Missed);
            int healthLost = missedCards - bangCards;
            int currentLives = playerObject.GetComponent<PlayerLogic>().CurrentLives;

            if (healthLost < 0) 
                playerObject.GetComponent<PlayerLogic>().SetLives(currentLives + healthLost);
            
            Debug.LogWarning("Trying to remove the cards in front of " + playerObject.GetComponent<PhotonView>().Owner.NickName);
            // Go through each card and move them to the discard pile
            int noCards = targetingStack.GetNumberOfCards();
            for (int counter = 0; counter < noCards; counter++)
            {
                Tuple<GameObject, PlayableCard> tuple = targetingStack.GetTopCard();
                tuple.Item1.GetComponent<PhotonView>().TransferOwnership(playerObject.GetComponent<PhotonView>().Owner);
                
                MoveCardToDiscardPile(tuple.Item2, tuple.Item1);
            }
        }
    }
    
    /// <summary>
    /// Helper function to resize the collider from the draw pile, so it always is the height of all the cards
    /// </summary>
    /// <param name="height">The amount to resize the collider with on the Y axis</param>
    /// <param name="sizeUp">If the collider should be sized up or down, default value = true (meaning up)</param>
    private void ResizeDrawCollider(float height, bool sizeUp = true)
    {
        height = sizeUp ? height : -height;
        BoxCollider drawPileCollider = drawPileContainer.GetComponent<BoxCollider>();
        drawPileCollider.size += new Vector3(0, height, 0);
        drawPileCollider.center += new Vector3(0, height/2, 0);
    }

    /// <summary>
    /// Loads the action cards from the "playcards" json file
    /// </summary>
    private void LoadPlayCards()
    {
        // TODO: Load the playable cards and load them into the draw pile
    }
    
    /// <summary>
    /// Moves the card to the discard pile
    /// </summary>
    /// <param name="cardLogic">The card object</param>
    /// <param name="cardObject">The gameobject representation of the card</param>
    public void MoveCardToDiscardPile(PlayableCard cardLogic, GameObject cardObject)
    {
        // TODO:Check the card Logic, execute the action
        
        // If we play the card, we lose ownership of it, it become neutral
        cardObject.GetComponent<PhotonView>().TransferOwnership(0);
        
        // Move the card to the discard pile
        discardPileCards.Insert(0, cardLogic);
        discardPileObjects.Insert(0, cardObject);
        
        // Move the gameobject card to the discard pile
        cardObject.transform.SetParent(discardPileContainer);
        cardObject.transform.localPosition = new Vector3(0, 0.0015f * discardPileCards.Count, 0 );
        cardObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
    }
    
    /// <summary>
    /// Takes the top most playable card (removing it from the draw pile) and returns it
    /// </summary>
    /// <returns> The drawn card logic and object</returns>
    public Tuple<PlayableCard,GameObject> DrawFromDrawPile()
    {
        if (drawPileCards.Count == 0)
        {
            AddDiscPileToDrawPile();
            ShuffleDrawPile();
        }

        if (drawPileCards.Count <= 0)
        {
            Debug.LogError("There are no cards in the draw pile?!");
            return null;
        }
            
        PlayableCard pc = drawPileCards[drawPileCards.Count - 1];
        GameObject cardObject = drawPileObjects[drawPileObjects.Count - 1];
        
        drawPileCards.Remove(pc);
        drawPileObjects.Remove(cardObject);

        ResizeDrawCollider(cardObject.GetComponent<MeshRenderer>().bounds.size.y, false);

        return new Tuple<PlayableCard, GameObject>(pc, cardObject);
    }

    /// <summary>
    /// Adds all the cards from the discard pile to the draw pile, emptying the discard pile
    /// </summary>
    public void AddDiscPileToDrawPile()
    {
        for (var index = 0; index < discardPileCards.Count; index++)
        {
            PlayableCard pc = discardPileCards[index];
            GameObject co = discardPileObjects[index];
            
            drawPileCards.Add(pc);
            drawPileObjects.Add(co);
            
            MoveCardObjectToDrawPile(co, index);
        }

        discardPileCards.Clear();
    }

    /// <summary>
    /// Shuffles the draw pile
    /// </summary>
    public void ShuffleDrawPile()
    {
        for (int i = 0; i < drawPileCards.Count; i++)
        {
            PlayableCard temp = drawPileCards[i];
            GameObject tempObj = drawPileObjects[i];
            
            int randomInd = Random.Range(i, drawPileCards.Count);
            
            drawPileCards[i] = drawPileCards[randomInd];
            drawPileObjects[i] = drawPileObjects[randomInd];
            
            MoveCardObjectToDrawPile(drawPileObjects[i], i);
            
            drawPileCards[randomInd] = temp;
            drawPileObjects[randomInd] = tempObj;
            
            MoveCardObjectToDrawPile(drawPileObjects[randomInd], randomInd);
        }
    }
    
    /// <summary>
    /// Gets the OwnerId of the current turns player
    /// </summary>
    /// <returns>OwnerId</returns>
    public int GetPlayerTurnId()
    {
        return GameSetup.instance.playerSeats[currentTurn];
    }

    /// <summary>
    /// Gets the nickname of the player who's turn is
    /// </summary>
    /// <returns>The nickname</returns>
    public string GetPlayerNameTurn()
    {
        return GameSetup.instance.GetPlayerObjectAtSeat(currentTurn).GetComponent<PhotonView>().Owner.NickName;
    }
    
    
    /// <summary>
    /// Setup function for the GameManager, use this instead of classic "Start()" function
    /// Used for synchronising the start params of the script
    /// </summary>
    /// <param name="seed">The seed of the pseudo random generator </param>
    [PunRPC]
    private void RPC_RunSetup(int seed)
    {
        Random.InitState(seed);
        _generatedSeed = seed;
        
        Debug.LogWarning("My gamemanager seed is: " + _generatedSeed);
        
        LoadCharacterCards();
        LoadActionCards();
        GeneratePlayableCards();
        ShuffleDrawPile();
        //RPC_DistributeRoles();
    }

    private void Start()
    {
        // Sets on all the clients the same seed for the random function
        // This way, any random operations as shuffling will have the same output on all clients
        if (PhotonNetwork.IsMasterClient) // Gamemanager is a scene owned object, only the "master" client should make the rpc call to the others
        {
            _generatedSeed = Random.Range(int.MinValue, int.MaxValue);
            _photonView.RPC("RPC_RunSetup", RpcTarget.AllBufferedViaServer, _generatedSeed);
        }
    }
}
