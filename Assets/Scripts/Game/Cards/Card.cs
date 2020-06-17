using System;
using UnityEngine;

[Serializable]
public class Card
{
    [SerializeField]
    private string cTitle;
    [SerializeField]
    private string cDescription;

    public string Title
    {
        get => cTitle;
        set => cTitle = value == null ? "" : value;
    }

    public string Description
    {
        get => cDescription;
        set => cDescription = value == null ? "" : value;
    }

    public Card(string cardTitle = "", string cardDescription = "")
    {
        Title = cardTitle;
        Description = cardDescription;
    }

    protected Card(Card c)
    {
        Title = c.Title;
        Description = c.Description;
    }

    public override string ToString()
    {
        return Title + ": " + Description;
    }
}
