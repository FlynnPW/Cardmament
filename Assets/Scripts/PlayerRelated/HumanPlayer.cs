using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{
    public HumanPlayer(Card[] deck, Color colour) : base(deck, colour) { }

    public override void takeTurnActions()
    {
        
    }

    public override bool isHuman()
    {
        return true;
    }
}
