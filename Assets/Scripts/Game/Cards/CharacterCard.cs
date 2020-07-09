using System;
using UnityEngine;

// FC = Firs card drawn (in the first phase of your turn)
// OLL = On lost life

/// <summary>
/// Enum for implementing all the different actions for each card.
/// </summary>
[Serializable]
public enum CharacterActions
{
    None,
    DrawOnLostLife,
    ShowSecondCard,
    InterBangMissed,
    DrawOLLFromPlayer,
    DrawFCFromPlayerOrDeck,
    Barrel,
    ChooseTwoFromThreeDrawn,
    OnDrawRequestChooseOneFromTwo,
    Distance,
    DrawFCFromDiscardOrDeck,
    Scope,
    DiscardTwoGainLife,
    RequiresTwoMissed,
    OnNoCardDraws,
    OnOtherPlayerDeathGainTheCards,
    AnyNumberOfBangs
}

/// <summary>
/// Class that represents the character cards.
/// </summary>
[Serializable]
public class CharacterCard : Card
{
    [SerializeField]
    private uint lives;

    public uint Lives
    {
        get => lives;
        set => lives = value;
    }

    private CharacterActions characterAction;


    public CharacterCard(CharacterActions action, string title ="", string description="", 
                        uint nrLives = 0):base(title, description)
    {
        Lives = nrLives;
        characterAction = action;
    }
}
