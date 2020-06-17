using System;
using UnityEngine;

namespace Game.Cards
{
    public enum CardType{
        action,
        weapon
    }
    
    [Serializable]
    public class CardDistribution
    {
        [SerializeField]
        private string title;
        [SerializeField]
        private uint amount;
        [SerializeField] 
        public CardType cardType;
        
        /// <summary>
        ///  The title of the card
        /// </summary>
        public string Title
        {
            get => title;
            set => title = value;
        }
    
        /// <summary>
        /// The number of cards with this title that should be placed in the starting deck
        /// </summary>
        public uint Amount
        {
            get => amount;
            set => amount = value;
        }
        
        
        public CardDistribution(string title, uint amount, CardType type)
        {
            Title = title;
            Amount = amount;
            cardType = type;
        }

        public override string ToString()
        {
            return Title + " : " + Amount;
        }
    }
}