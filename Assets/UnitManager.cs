using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitManager;

public class UnitManager : MonoBehaviour
{
    public static UnitManager instance { get; private set; }
    public enum direction { down, left, right, up }
    private UnitWorld[,] unitsAtPositions;
    private TileManager tileManager;
    [SerializeField]
    private GameObject unitPrefab;

    void Start()
    {
        instance = GetComponent<UnitManager>();
        tileManager = GetComponent<TileManager>();
    }

    public void mapSet(Vector2Int size)
    {
        unitsAtPositions = new UnitWorld[size.x, size.y];
        createUnitAt(new Vector2Int(0, 0));
    }

    public List<unitMove> potentialMovePositions(Vector2Int position, int maxBy)
    {
        direction[] directionsToMove = new direction[] { direction.down, direction.left, direction.right, direction.up };
        List<unitMove> movePositions = new List<unitMove>();

        foreach (direction moveInDirection in directionsToMove)
        {
            for (int distance = 1; distance <= maxBy; distance++)
            {
                Vector2Int potentialPosition = position + tileManager.getDirectionVector(moveInDirection) * distance;

                if (tileManager.isTileCurrentlyPassable(potentialPosition))
                {
                    movePositions.Add(new unitMove(moveInDirection, distance, potentialPosition));
                }
                else
                {
                    break;
                }
            }
        }

        return movePositions;
    }

    public struct unitMove
    {
        public direction direction;
        public int distance;
        public Vector2Int gridPosition;

        public unitMove(direction unitDirection, int unitDistance, Vector2Int position)
        {
            direction = unitDirection;
            distance = unitDistance;
            gridPosition = position;
        }
    }

    public bool moveUnitBy(UnitWorld unitToMove, direction inDirection, int by)
    {
        Vector2Int unitPosition = unitToMove.getGridPosition();
        Vector2Int unitMoveTo = unitPosition + tileManager.getDirectionVector(inDirection) * by;
        TileOnBoard[] tilesOnPath = tileManager.getPathTo(unitPosition, inDirection, by);

        if (tilesOnPath == null)
        {
            return false;
        }

        //we stepped on each point between this and the last tile
        for (int i = 0; i < tilesOnPath.Length - 1; i++)
        {
            TileOnBoard tile = tilesOnPath[i];
            tile.unitSteppedOn(unitToMove);
        }

        setUnitToPosition(unitToMove, unitMoveTo);
        return true;
    }

    private void setUnitToPosition(UnitWorld unit, Vector2Int nowAt)
    {
        Vector2Int oldPosition = unit.getGridPosition();

        if (unit.isUnitPoisitionSet())
        {
            unitsAtPositions[oldPosition.x, oldPosition.y] = null;
            tileManager.unitLeftTile(oldPosition);
        }
        
        unitsAtPositions[nowAt.x, nowAt.y] = unit;
        tileManager.unitEnteredTile(nowAt);
        unit.unitMovedTo(nowAt);

        //inform tile
        TileOnBoard tileAt = tileManager.getTile(nowAt);
        tileAt.unitSteppedOn(unit);
    }

    public UnitWorld getUnitAtTile(Vector2Int tile)
    {
        return unitsAtPositions[tile.x, tile.y];
    }

    public void createUnitAt(Vector2Int at)
    {
        UnitWorld newUnit = Instantiate(unitPrefab).GetComponent<UnitWorld>();
        setUnitToPosition(newUnit, at);
    }
}
