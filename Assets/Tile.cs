using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tile
{
    [SerializeField]
    private bool passable;
    [SerializeField]
    private Color tileColour;

    public bool isTilePassable()
    {
        return passable;
    }

    public Color getTileColour()
    {
        return tileColour;
    }
}
