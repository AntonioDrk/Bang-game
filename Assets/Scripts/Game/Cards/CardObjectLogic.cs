using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CardObjectLogic : MonoBehaviour
{
    /// <summary>
    /// Text object that represents the title of the card on the canvas
    /// </summary>
    [SerializeField] private TextMeshProUGUI cardTitleReference;
    /// <summary>
    /// Text object that represents the description of the card on the canvas
    /// </summary>
    [SerializeField] private TextMeshProUGUI cardDescriptionReference;
    /// <summary>
    /// Text object that represents the number of the card on the canvas
    /// </summary>
    [SerializeField] private TextMeshProUGUI cardNumber;
    /// <summary>
    /// Text object that represents the Image of the suit for the card on the canvas
    /// </summary>
    [SerializeField] private Image cardSuit;
    /// <summary>
    /// The gameobject that holds the action buttons
    /// </summary>
    [SerializeField] private GameObject actionBtns;

    // The images for the various suits
    [SerializeField] private Sprite clubsImg;
    [SerializeField] private Sprite spadesImg;
    [SerializeField] private Sprite heartsImg;
    [SerializeField] private Sprite diamondsImg;

    // The main camera to use for the canvases
    private Camera _mainCamera;

    void Start()
    {
        if (Camera.main is null)
            Debug.LogError("Main camera not found in scene! Make sure camera has MainCamera Tag");
        else
        {
            _mainCamera = Camera.main;
            transform.GetComponentInChildren<Canvas>().worldCamera = _mainCamera;
        }
    }

    /// <summary>
    /// Sets the info from the canvas to the card's object info.
    /// </summary>
    /// <param name="card">The card object that you want to display on this game-object's canvas</param>
    public void ApplyCardInfo(Card card)
    {
        if (cardDescriptionReference is null || cardTitleReference is null || cardNumber is null || cardSuit is null)
        {
            Debug.LogError("The reference to the title of the card or the description of the card are not set!");
            return;
        }
        
        cardTitleReference.text = card.Title;
        cardDescriptionReference.text = card.Description;

        // If card is actually a playable card, show the number and the sprite of the suit
        if (card is PlayableCard playableCard)
        {
            cardNumber.text = playableCard.Number.ToString();

            switch (playableCard.Suit)
            {
                case CardSuit.Clubs:
                    cardSuit.sprite = clubsImg;
                    break;
                case CardSuit.Spades:
                    cardSuit.sprite = spadesImg;
                    break;
                case CardSuit.Hearts:
                    cardSuit.sprite = heartsImg;
                    break;
                case CardSuit.Diamonds:
                    cardSuit.sprite = diamondsImg;
                    break;
                default:
                    Debug.LogError("There's an error while trying to render the suit of the card.");
                    break;
            }
            
        }
    }
    
    
    /// <summary>
    /// Get the exit action button of the card may return null
    /// </summary>
    /// <returns>Reference to the button component</returns>
    public Button GetExitButton()
    {
        return actionBtns.transform.GetChild(0) is null ? null : actionBtns.transform.GetChild(0).GetComponent<Button>();
    }
    
    /// <summary>
    /// Get the play action button of the card may return null
    /// </summary>
    /// <returns>Reference to the button component</returns>
    public Button GetPlayButton()
    {
        return actionBtns.transform.GetChild(1) is null ? null : actionBtns.transform.GetChild(1).GetComponent<Button>();
    }
    
    public bool GetActionBtnState()
    {
        return actionBtns.activeSelf;
    }
    
    public void ShowActionBtns(bool showBtns)
    {
        actionBtns.SetActive(showBtns);
    }
}
