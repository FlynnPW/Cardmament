using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CardImpact
{
    public bool attemptToPlay(Vector2Int atTile, Player playedBy);
}
