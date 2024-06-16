using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileOnBoard
{
    private Tile tile;
    private TileCapturePoint capturePoint;

    public TileOnBoard(Tile tile)
    {
        this.tile = tile;
    }

    public void unitSteppedOn(UnitWorld unit)
    {
        if (capturePoint == null)
        {
            return;
        }

        capturePoint.unitAtPoint(unit);
    }

    public void setCapturePoint(TileCapturePoint to)
    {
        capturePoint = to;
    }

    public Tile getTile()
    {
        return tile;
    }
}
