using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Cards;
using Photon.Pun;
using UnityEngine;
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

    private CharacterCard[] characterCards;
    private ActionCard[] actionCards;
    private WeaponCard[] weaponCards;
    
    
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
        if(data == null)
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

    public int GetPlayerTurnId()
    {
        return GameSetup.setup.playerSeats[currentTurn];
    }

    [PunRPC]
    public void NextTurn()
    {
        currentTurn++;
        currentTurn %= GameSetup.setup.playerSeats.Length;
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
    
    private void GenerateRandomDrawCards(int numberOfCards)
    {
        // TODO: This function should generate all the cards that can be drawn
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
    /// Function that is responsible for executing the card action and logic.
    /// The one calling this function is responsible to delete the local cache of it.
    /// </summary>
    /// <param name="cardLogic">The card object</param>
    /// <param name="cardObject">The gameobject representation of the card</param>
    public void PlayCard(PlayableCard cardLogic, GameObject cardObject)
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
    }

    private void Awake()
    {
        // Sets on all the clients the same seed for the random function
        // This way, any random operations as shuffling will have the same output on all clients
        if (PhotonNetwork.IsMasterClient) // Gamemanager is a scene owned object, only the "master" client should make the rpc call to the others
        {
            _generatedSeed = Random.Range(int.MinValue, int.MaxValue);
            _photonView.RPC("RPC_RunSetup", RpcTarget.AllBuffered, _generatedSeed);
        }
    }

    private void Start()
    {
        //GenerateRandomDrawCards(NumberOfDrawCards);
    }
}
