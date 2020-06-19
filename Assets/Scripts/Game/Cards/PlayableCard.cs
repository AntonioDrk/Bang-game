using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public enum CardSuit
{
    Clubs,
    Hearts,
    Spades,
    Diamonds
}

/// <summary>
/// Class that represents the cards player can hold in hand and so on
/// </summary>
[Serializable]
public class PlayableCard : Card
{
    [SerializeField]
    private CardSuit suit;
    [SerializeField]
    private uint number;

    public CardSuit Suit
    {
        get => suit;
        set => suit = value;
    }

    public uint Number
    {
        get => number;
        set => number = value > 13 ? 0 : value;
    }

    public PlayableCard(CardSuit cardSuit, uint cardNumber,string cardTitle = "",
                        string cardDescription = ""):base(cardTitle, cardDescription)
    {
        Suit = cardSuit;
        Number = cardNumber;
    }

    protected PlayableCard(PlayableCard ac) : base(ac)
    {
        Suit = ac.Suit;
        Number = ac.Number;
    }
}
