using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCreateUnitImpact : CardImpact
{
    UnitWorld.Unit unit;

    public CardCreateUnitImpact(UnitWorld.Unit unit)
    {
        this.unit = unit;
    }

    bool CardImpact.attemptToPlay(Vector2Int atTile, Player playedBy)
    {
        UnitWorld unitAtTile = UnitManager.instance.getUnitAtTile(atTile);

        if (unitAtTile != null)
        {
            return false;
        }

        UnitManager.instance.createUnitAt(atTile, unit, playedBy);
        return true;
    }
}
