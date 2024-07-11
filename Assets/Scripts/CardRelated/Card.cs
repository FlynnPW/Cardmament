using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    private string name;
    private string description;
    private Sprite image;
    private CardImpact cardImpact;
    private int manaCost;

    public Card(string name, string description, Sprite image, CardImpact cardImpact, int manaCost)
    {
        this.name = name;
        this.description = description;
        this.image = image;
        this.cardImpact = cardImpact;
        this.manaCost = manaCost;
    }

    public bool attemptToPlayCard(Vector2Int at, Player playedBy)
    {
        return cardImpact.attemptToPlay(at, playedBy);
    }

    public string getName()
    {
        return name;
    }

    public string getDescription()
    {
        return description;
    }

    public Sprite getImage()
    {
        return image;
    }

    public int getManaCost()
    {
        return manaCost;
    }
}
