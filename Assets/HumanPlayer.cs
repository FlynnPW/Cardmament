using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{
    public HumanPlayer(Card[] deck) : base(deck) { }

    public override void takeTurnActions()
    {
        
    }

    public override bool isHuman()
    {
        return true;
    }
}
