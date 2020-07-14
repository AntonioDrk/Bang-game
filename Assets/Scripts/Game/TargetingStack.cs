using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Function that handles the calculation of damage, actions,
/// and keeping track of the amount of cards that are in front of a player
/// </summary>
public class TargetingStack : MonoBehaviour
{
    private List<PlayableCard> cards;
    private List<GameObject> cardObjects;

    private void Awake()
    {
        cards = new List<PlayableCard>();
        cardObjects = new List<GameObject>();
    }
    
    public void AddCardToStack(GameObject cardObject, PlayableCard cardLogic)
    {
        cards.Add(cardLogic);
        cardObjects.Add(cardObject);
    }

    public int NumberOfCardsOfEffect(Effect givenEffect)
    {
        int sum = 0;
        foreach (PlayableCard playableCard in cards)
        {
            ActionCard card = (ActionCard) playableCard;
            if (card.Effect == givenEffect)
                sum++;
        }
        return sum;
    }
    
    /// <summary>
    /// Gets the top card and also removes it
    /// </summary>
    /// <returns>A tuple made of the gameObject and Logic of a card</returns>
    public Tuple<GameObject, PlayableCard> GetTopCard()
    {
        int index = cards.Count - 1;
        if (index >= 0)
        {
            Tuple<GameObject,PlayableCard> rez = new Tuple<GameObject, PlayableCard>(cardObjects[index],cards[index]);
            cards.RemoveAt(index);
            cardObjects.RemoveAt(index);
            return rez;
        }
        return null;
    }

    /// <summary>
    /// Checks if a card with the BANG effect is on top of the stack
    /// </summary>
    /// <returns>True if a card BANG is on top of the stack</returns>
    public bool IsBangFirstCard()
    {
        int index = cards.Count - 1;
        if (index >= 0 && ((ActionCard) cards[index]).Effect == Effect.Bang)
        {
            return true;
        }
        return false;
    }
    
    public int GetNumberOfCards()
    {
        return cardObjects.Count;
    }
}
