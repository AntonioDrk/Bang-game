using System;
using System.Collections.Generic;
using UnityEngine;

public enum Target
{
    None,
    InWeaponRange,
    RangeOne,
    AnyPlayer,
    AllPlayers
}

public enum Effect
{
    DiscardOne,
    Bang,
    Missed,
    DrawOne
}

[Serializable]
public class ActionCard : PlayableCard
{
    [SerializeField]
    private Target target;
    [SerializeField]
    private Effect[] effects;
    

    public Target Target
    {
        get => target;
        set => target = value;
    }

    public Effect[] Effects
    {
        get => effects;
        set => value.Clone();
    }

    public ActionCard(Target target, Effect[] effects, CardSuit cardSuit, uint cardNumber, string cardTitle = "",
        string cardDescription = "") : base(cardSuit, cardNumber, cardTitle, cardDescription)
    {
        Target = target;
        Effects = effects;
    }

    public ActionCard(ActionCard ac) : base(ac)
    {
        Target = ac.Target;
        Effects = ac.Effects;
    }
}
