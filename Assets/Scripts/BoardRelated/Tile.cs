using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tile
{
    [SerializeField]
    private bool passable;

    public bool isTilePassable()
    {
        return passable;
    }
}
