using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCreateUnitImpact : CardImpact
{
    private UnitWorld.Unit unit;
    private bool restrictedToHomeZone;

    public CardCreateUnitImpact(UnitWorld.Unit unit)
    {
        this.unit = unit;
    }

    bool CardImpact.attemptToPlay(Vector2Int atTile, Player playedBy)
    {
        UnitWorld unitAtTile = UnitManager.instance.getUnitAtTile(atTile);

        if (unitAtTile != null || TileManager.instance.isAvaliableTileToPlace(atTile, playedBy, restrictedToHomeZone == false))
        {
            return false;
        }

        UnitManager.instance.createUnitAt(atTile, unit, playedBy);
        return true;
    }
}
