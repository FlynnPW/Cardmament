using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileFeature
{
    [SerializeField]
    private bool featurePassable;

    public bool getFeaturePassable()
    {
        return featurePassable;
    }
}
