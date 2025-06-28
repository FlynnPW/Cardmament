using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tile
{
    [SerializeField]
    private Material material;

    public Material getMaterial()
    {
        return material;
    }
}
