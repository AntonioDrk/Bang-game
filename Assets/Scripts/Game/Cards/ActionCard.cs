using System;
using System.Collections.Generic;
using UnityEngine;

public enum Target
{
    None,
    InWeaponRange,
    RangeOne,
    AnyPlayer,
    AllPlayers,
    Yourself
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
    private Effect effect;
    

    public Target Target
    {
        get => target;
        set => target = value;
    }

    public Effect Effect
    {
        get => effect;
        set => effect = value;
    }

    public ActionCard(Target target, Effect effect, CardSuit cardSuit, uint cardNumber, string cardTitle = "",
        string cardDescription = "") : base(cardSuit, cardNumber, cardTitle, cardDescription)
    {
        Target = target;
        Effect = effect;
    }

    public ActionCard(ActionCard ac) : base(ac)
    {
        Target = ac.Target;
        Effect = ac.Effect;
    }
}
