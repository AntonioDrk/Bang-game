using UnityEngine;

/// <summary>
/// Helper class for ease of use generation of different aspects of the cards
/// </summary>
public static class CardGeneration
{
    /// <summary>
    /// Generates a random suit
    /// </summary>
    /// <returns>A random suit</returns>
    public static CardSuit GenerateRandomSuit()
    {
        // Change the seed each time
        Random.InitState(Random.Range(0, 864346724));
        // Generate a random suit for the card
        switch (Random.Range(0,4))
        {
            case 0:
                return CardSuit.Clubs;
            case 1:
                return CardSuit.Diamonds;
            case 2:
                return CardSuit.Hearts;
            case 3:
                return CardSuit.Spades;
            default: 
                return CardSuit.Spades;
        }
    }

    /// <summary>
    /// Function designed specifically for testing purposes. Generates a BANG! card with a random suit and number.
    /// </summary>
    /// <returns>The playable card</returns>
    public static PlayableCard GenerateRandomPlayableCard()
    {
        return new PlayableCard(GenerateRandomSuit(), (uint)Random.Range(1,14), "BANG!", 
            "Target someone in range and deal 1 point of damage! (Can be blocked with a \"Missed!\" card)");
    }
    
    //TODO: The different types of playable cards and store them locally, make function to return such cards
}
