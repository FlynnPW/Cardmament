using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileOnBoard
{
    private Tile tile;
    private TileFeature feature;
    private TileCapturePoint capturePoint;
    private Player homeZoneOf;
    private Player territoryOf;
    private List<BoardPiece> inInfluenceOf;
    private TileManager.tilePassableStatus passableStatus;

    public TileOnBoard(Tile tile)
    {
        this.tile = tile;
        passableStatus = TileManager.tilePassableStatus.passable;
        inInfluenceOf = new List<BoardPiece>();
    }

    //when a unit is passing a tile
    public void unitSteppedOn(UnitWorld unit)
    {
        if (capturePoint == null)
        {
            return;
        }

        capturePoint.unitAtPoint(unit);
        setTerritoryOf(unit.getAllegiance());
    }

    public void tileInInfluenceOf(BoardPiece influencedBy)
    {
        if (inInfluenceOf.Count == 0)
        {
            setTerritoryOf(influencedBy.getPlayerOwner());
        }

        inInfluenceOf.Add(influencedBy);
    }

    public void tileNoLongerInInfluenceOf(BoardPiece noLongerInfluencedBy)
    {
        inInfluenceOf.Remove(noLongerInfluencedBy);

        //no influences so territory does not change hands
        if (inInfluenceOf.Count == 0)
        {
            return;
        }

        List<Player> influencerAllegiances = new List<Player>();

        foreach (BoardPiece checkInfluencer in inInfluenceOf)
        {
            if (influencerAllegiances.Contains(checkInfluencer.getPlayerOwner()) == false)
            {
                influencerAllegiances.Add(checkInfluencer.getPlayerOwner());
            }
        }

        //no longer influenced by this player's pieces!
        if (influencerAllegiances.Contains(noLongerInfluencedBy.getPlayerOwner()) == false)
        {
            //we are no only influenced by a single player's pieces so we switch to their territory
            if (influencerAllegiances.Count == 1)
            {
                setTerritoryOf(influencerAllegiances[0]);
            }
            else //multiple new allegiances it could switch to, so we become null! (Note: this will only happen if player > 2 rn that's not really going to happen so this is future proofing?)
            {
                setTerritoryOf(null);
            }
        }
    }

    //when a unit is specifically ON the tile (not just passing by)
    public void unitNowOn()
    {
        passableStatus = TileManager.tilePassableStatus.unitBlocked;
    }

    public void unitNowOff()
    {
        passableStatus = TileManager.tilePassableStatus.passable;
    }

    public void setCapturePoint(TileCapturePoint to)
    {
        capturePoint = to;
    }

    public Tile getTile()
    {
        return tile;
    }
    
    public TileFeature getFeature()
    {
        return feature;
    }

    public void addFeature(TileFeature toAdd)
    {
        if (feature != null)
        {
            Debug.LogError("Trying to add feature to tile that already has feature!");
        }

        feature = toAdd;
        passableStatus = toAdd.getFeaturePassable() ? TileManager.tilePassableStatus.passable : TileManager.tilePassableStatus.tileBlocked;
    }

    public Player getHomeZoneOf()
    {
        return homeZoneOf;
    }

    public void setHomeZoneOf(Player allegiance)
    {
        homeZoneOf = allegiance;
        setTerritoryOf(allegiance);
    }

    public Player getTerritoryOf()
    {
        return territoryOf;
    }

    public void setTerritoryOf(Player allegiance)
    {
        if (allegiance == territoryOf)
        {
            return;
        }

        territoryOf = allegiance;
    }

    public bool isTilePassable()
    {
        return passableStatus == TileManager.tilePassableStatus.passable;
    }
}
