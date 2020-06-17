using System;
using UnityEngine;

[Serializable]
public class WeaponCard : PlayableCard
{
    [SerializeField]
    private uint rangeIncrease;
    
    public uint RangeIncrease
    {
        get => rangeIncrease;
        set => rangeIncrease = value;
    }

    public WeaponCard(uint rangeIncrease, CardSuit cardSuit, uint cardNumber, string cardTitle = "",
        string cardDescription = "") : base(cardSuit, cardNumber, cardTitle, cardDescription)
    {
        RangeIncrease = rangeIncrease;
    }

    public WeaponCard(WeaponCard wc) : base(wc)
    {
        RangeIncrease = wc.RangeIncrease;
    }
}
