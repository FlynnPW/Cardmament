using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileManager : MonoBehaviour
{
    private Vector2Int mapSize;
    private TileOnBoard[,] mapTiles;
    private Player[,] playerHomeZone;
    private enum tilePassableStatus { passable, unitBlocked, tileBlocked }
    private tilePassableStatus[,] tilesPassableStatus;
    private delegate void tileSetTo(Vector2Int tile, Tile to);
    private tileSetTo tileSetToCallback;
    private Vector2Int[] neighbours = new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1) };
    [SerializeField]
    private Tile mountainTile;
    [SerializeField]
    private Tile waterTile;
    [SerializeField]
    private Tile grassTile;
    [SerializeField]
    private GameObject tilePrefab;
    private BoardDisplayManager boardDisplayManager;
    private PlayersManager playersManagers;
    //RANDOM GENERATION VALUES
    const int MAX_RIVERS = 2;
    const int MIN_RIVERS = 0;
    const int MAX_MOUNTAINS = 3;
    const int MIN_MOUNTAINS = 1;
    const int MAX_MAP_SIZE = 12;
    const int MIN_MAP_SIZE = 12;
    const int AMOUNT_OF_MANA_CAPTURE_POINTS = 4;
    const int CAPTURE_POINT_BLOCKING_DISTANCE = 1; //both ways so 1 distance = [blocked] [mana point] [blocked]

    private void Start()
    {
        boardDisplayManager = GetComponent<BoardDisplayManager>();
        playersManagers = GetComponent<PlayersManager>();
    }

    public void generateMap(int generateFromSeed)
    {
        Player[] playersOnMap = playersManagers.getAllPlayers();

        Random.InitState(generateFromSeed);

        int size = Random.Range(MIN_MAP_SIZE, MAX_MAP_SIZE);
        mapSize = new Vector2Int(size, size);
        mapTiles = new TileOnBoard[size, size];
        tilesPassableStatus = new tilePassableStatus[size, size];
        playerHomeZone = new Player[size, size];
        List<Vector2Int> emptyTiles = new List<Vector2Int>();
        List<Vector2Int>[] allRiversCoordinates;
        List<Vector2Int>[] allMountainsCoordiantes;
        TileCapturePoint[] allCapturePoints = null;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                emptyTiles.Add(new Vector2Int(x, y));
            }
        }

        Vector2Int getRandomClearTile()
        {
            int randomClearTileIndex = Random.Range(0, emptyTiles.Count - 1);
            Vector2Int tile = emptyTiles[randomClearTileIndex];
            emptyTiles.RemoveAt(randomClearTileIndex);
            return tile;
        }

        void tileSetTo(Vector2Int position, Tile to)
        {
            if (to != null && emptyTiles.Contains(position))
            {
                emptyTiles.Remove(position);
            }
        }

        tileSetToCallback += tileSetTo;

        void createHomeZones()
        {
            if (playersOnMap.Length != 2)
            {
                Debug.LogError("Homezones function only compatible with 2 players.");
            }

            for (int x = 0; x < mapSize.x; x++)
            {
                playerHomeZone[x, 0] = playersOnMap[0];
                playerHomeZone[x, mapSize.y - 1] = playersOnMap[1];
                setTileTo(new Vector2Int(x, 0), grassTile);
                setTileTo(new Vector2Int(x, mapSize.y - 1), grassTile);
            }
        }

        void createCapturePoints()
        {
            List<int> potentialXCoordinatesForCapturePoints = new List<int>();
            List<int> potentialYCoordinatesForCapturePoints = new List<int>();
            allCapturePoints = new TileCapturePoint[AMOUNT_OF_MANA_CAPTURE_POINTS];

            for (int x = 0; x < mapSize.x; x++)
            {
                potentialXCoordinatesForCapturePoints.Add(x);
            }

            for (int y = 1; y < mapSize.y - 1; y++)
            {
                potentialYCoordinatesForCapturePoints.Add(y);
            }

            for (int manaPoint = 0; manaPoint < AMOUNT_OF_MANA_CAPTURE_POINTS; manaPoint++)
            {
                if (potentialXCoordinatesForCapturePoints.Count == 0 || potentialYCoordinatesForCapturePoints.Count == 0)
                {
                    Debug.LogError("Ran out of locations to place capture points!");
                }

                int xCoordinateIndex = Random.Range(0, potentialXCoordinatesForCapturePoints.Count);
                int yCoordinateIndex = Random.Range(0, potentialYCoordinatesForCapturePoints.Count);

                Vector2Int placeCapturePointAt = new Vector2Int(
                    potentialXCoordinatesForCapturePoints[xCoordinateIndex],
                    potentialYCoordinatesForCapturePoints[yCoordinateIndex]);

                if (emptyTiles.Contains(placeCapturePointAt) == false)
                {
                    Debug.LogError("Trying to place capture point at non-empty tile!");
                }

                setTileTo(placeCapturePointAt, grassTile);
                TileCapturePoint newCapturePoint = new ManaCapturePoint(placeCapturePointAt);
                getTile(placeCapturePointAt).setCapturePoint(newCapturePoint); //set to mana point           

                void removeCoordinatesFromList(List<int> removeFrom, int valueToRemoveAround, int size)
                {
                    for (int i = Math.Max(valueToRemoveAround - CAPTURE_POINT_BLOCKING_DISTANCE, 0); i < Math.Min(size, valueToRemoveAround + CAPTURE_POINT_BLOCKING_DISTANCE); i++)
                    {
                        removeFrom.Remove(valueToRemoveAround);
                    }
                }

                removeCoordinatesFromList(potentialXCoordinatesForCapturePoints, placeCapturePointAt.x, mapSize.x);
                removeCoordinatesFromList(potentialYCoordinatesForCapturePoints, placeCapturePointAt.y, mapSize.y);
                allCapturePoints[manaPoint] = newCapturePoint;
            }
        }

        void createRivers()
        {
            int rivers = Random.Range(MIN_RIVERS, MAX_RIVERS + 1);
            allRiversCoordinates = new List<Vector2Int>[rivers];

            for (int i = 0; i < rivers; i++)
            {
                List<Vector2Int> riverCoordinates = new List<Vector2Int>();
                int spawnRiverAtEdge = Random.Range(0, 3);

                Vector2Int getRandomStartingTile(int edge)
                {
                    int spawnRiverAtTile = Random.Range(0, mapSize.x - 1);
                    Vector2Int riverTile;
                    //left=0,bottom=1,right=2,top=3

                    switch (edge)
                    {
                        case 0:
                            riverTile = new Vector2Int(0, spawnRiverAtTile + 1);
                            break;
                        case 1:
                            riverTile = new Vector2Int(spawnRiverAtTile, 0);
                            break;
                        case 2:
                            riverTile = new Vector2Int(mapSize.x - 1, spawnRiverAtTile);
                            break;
                        case 3:
                            riverTile = new Vector2Int(spawnRiverAtTile + 1, mapSize.y - 1);
                            break;
                        default:
                            Debug.LogError("River edge not assigned");
                            riverTile = new Vector2Int();
                            break;
                    }

                    bool leftRightSide = riverTile.x == 0 || riverTile.x == mapSize.x - 1;
                    bool topBottomSide = riverTile.y == 0 || riverTile.y == mapSize.y - 1;

                    if (isTileOfType(riverTile, waterTile) || neighbouringWaterTile(riverTile, topBottomSide, leftRightSide))
                    {
                        return getRandomStartingTile(edge);
                    }

                    print("River started at: " + riverTile);

                    return riverTile;
                }

                void addRiverTile(Vector2Int at)
                {
                    setTileTo(at, waterTile);
                    riverCoordinates.Add(at);
                }

                Vector2Int riverTile = getRandomStartingTile(spawnRiverAtEdge);
                addRiverTile(riverTile);

                bool riverContinue = true;
                int riverLength = 1;

                bool closerToEdge(Vector2Int neighbour, Vector2Int tile, int edge)
                {
                    switch (spawnRiverAtEdge)
                    {
                        case 0:
                            return neighbour.x < tile.x;
                        case 1:
                            return neighbour.y < tile.y;
                        case 2:
                            return neighbour.x > tile.x;
                        case 3:
                            return neighbour.y > tile.y;
                        default:
                            Debug.LogError("River edge not assigned");
                            return false;
                    }
                }

                bool atOppositeEdge(Vector2Int tile, int edge)
                {
                    switch (spawnRiverAtEdge)
                    {
                        case 0:
                            return tile.x == mapSize.x - 1;
                        case 1:
                            return tile.y == mapSize.y - 1;
                        case 2:
                            return tile.x == 0;
                        case 3:
                            return tile.y == 0;
                        default:
                            Debug.LogError("River edge not assigned");
                            return false;
                    }
                }

                while (riverContinue)
                {
                    List<Vector2Int> neighbours = getNeighbouringTiles(riverTile);
                    neighbours.RemoveAll(neighbour =>
                    neighbouringWaterTile(neighbour, neighbour.y != riverTile.y, neighbour.x != riverTile.x) || isTileOfType(neighbour, null) == false || closerToEdge(neighbour, riverTile, spawnRiverAtEdge));

                    if (neighbours.Count == 0)
                    {
                        riverContinue = false;
                        print("river stopped due to no vaiable expansions at " + riverTile);
                        continue;
                    }

                    int randomNeighbourIndex = Random.Range(0, neighbours.Count);
                    Vector2Int chosenNeighbour = neighbours[randomNeighbourIndex];
                    riverTile = chosenNeighbour;
                    addRiverTile(riverTile);
                    riverLength++;

                    if (atOppositeEdge(riverTile, spawnRiverAtEdge))
                    {
                        print("river stopped as we've reached the other side at " + riverTile);
                        riverContinue = false;
                    }
                }

                allRiversCoordinates[i] = riverCoordinates;
            }      
        }

        void createMountains()
        {
            int mountains = Random.Range(MIN_MOUNTAINS, MAX_MOUNTAINS);
            allMountainsCoordiantes = new List<Vector2Int>[mountains];

            for (int i = 0; i < mountains; i++)
            {
                List<Vector2Int> mountainCoordinates = new List<Vector2Int>();
                Vector2Int mountainPosition = getRandomClearTile();

                void addMountainTile(Vector2Int at)
                {
                    setTileTo(at, mountainTile);
                    mountainCoordinates.Add(at);
                }

                addMountainTile(mountainPosition);
                List<Vector2Int> neighbours = getNeighbouringTiles(mountainPosition);

                foreach (Vector2Int potentialExpansion in neighbours)
                {
                    if (isTileOfType(potentialExpansion, null))
                    {
                        if (Random.Range(0, 2) == 1)
                        {
                            addMountainTile(potentialExpansion);
                        }
                    }
                }

                allMountainsCoordiantes[i] = mountainCoordinates;
            }
        }

        void fillInGrass()
        {
            while (emptyTiles.Count > 0)
            {
                Vector2Int emptyTile = emptyTiles[0];
                setTileTo(emptyTile, grassTile);
            }
        }

        createHomeZones();
        createCapturePoints();
        createRivers();
        createMountains();
        fillInGrass();
        tileSetToCallback -= tileSetTo;
        UnitManager.instance.mapSet(mapSize);
        boardDisplayManager.renderMap(allRiversCoordinates, allMountainsCoordiantes, allCapturePoints, mapSize);
    }

    private bool neighbouringWaterTile(Vector2Int at, bool horizontal, bool vertical)
    {
        if (horizontal) //are we above or below?
        {
            return isTileOfType(new Vector2Int(at.x - 1, at.y), waterTile) || isTileOfType(new Vector2Int(at.x + 1, at.y), waterTile); //horizontal to another water tile?
        }
        else if (vertical)
        {
            return isTileOfType(new Vector2Int(at.x, at.y + 1), waterTile) || isTileOfType(new Vector2Int(at.x, at.y + 1), waterTile); //vertical to another water tile?
        }

        return false;
    }

    private void setTileTo(Vector2Int positionToSet, Tile to)
    {
        mapTiles[positionToSet.x, positionToSet.y] = new TileOnBoard(to);
        tilesPassableStatus[positionToSet.x, positionToSet.y] = to.isTilePassable() ? tilePassableStatus.passable : tilePassableStatus.tileBlocked;
        if (tileSetToCallback != null) { tileSetToCallback(positionToSet, to); }
    }

    private List<Vector2Int> getNeighbouringTiles(Vector2Int from)
    {
        List<Vector2Int> neighboursToReturn = new List<Vector2Int>();

        foreach (Vector2Int neighbour in neighbours)
        {
            Vector2Int neighbourPos = from + neighbour;

            if (isTileWithinBounds(neighbourPos))
            {
                neighboursToReturn.Add(neighbourPos);
            }
        }

        return neighboursToReturn;
    }

    private bool isTileOfType(Vector2Int positionOfTile, Tile typeCheck)
    {
        if (isTileWithinBounds(positionOfTile) == false)
        {
            return false;
        }

        if (mapTiles[positionOfTile.x, positionOfTile.y] == null)
        {
            return typeCheck == null;
        }

        return mapTiles[positionOfTile.x, positionOfTile.y].getTile() == typeCheck;
    }

    public bool isTileWithinBounds(Vector2Int toCheck)
    {
        return (toCheck.x < 0 || toCheck.x > mapSize.x - 1 || toCheck.y < 0 || toCheck.y > mapSize.y - 1) == false;
    }

    public TileOnBoard[] getPathTo(Vector2Int from, UnitManager.direction inDirection, int by)
    {
        if (isTileWithinBounds(from + getDirectionVector(inDirection) * by) == false)
        {
            return null;
        }

        Vector2Int at = from;
        TileOnBoard[] tilesInPath = new TileOnBoard[by];

        for (int i = 0; i < by; i++)
        {
            at += getDirectionVector(inDirection);
            
            if (isTileCurrentlyPassable(at))
            {
                tilesInPath[i] = mapTiles[at.x, at.y];
            }
            else
            {
                return null; 
            }
        }

        return tilesInPath;
    }

    public TileOnBoard getTile(Vector2Int at)
    {
        return mapTiles[at.x, at.y];
    }

    //this checks if the tile is specifically passable now (no unit AND is a passable tile)
    public bool isTileCurrentlyPassable(Vector2Int tile)
    {
        if (isTileWithinBounds(tile) == false)
        {
            return false;
        }

        return tilesPassableStatus[tile.x, tile.y] == tilePassableStatus.passable;
    }

    public void unitEnteredTile(Vector2Int tile)
    {
        tilesPassableStatus[tile.x, tile.y] = tilePassableStatus.unitBlocked;
    }

    public void unitLeftTile(Vector2Int tile)
    {
        tilesPassableStatus[tile.x, tile.y] = tilePassableStatus.passable;
    }

    public static Vector2Int getDirectionVector(UnitManager.direction inDirection)
    {
        switch (inDirection)
        {
            case UnitManager.direction.down:
                return new Vector2Int(0, -1);
            case UnitManager.direction.left:
                return new Vector2Int(-1, 0);
            case UnitManager.direction.right:
                return new Vector2Int(1, 0);
            case UnitManager.direction.up:
                return new Vector2Int(0, 1);
            default:
                Debug.LogError("How?");
                return new Vector2Int(0, 0);
        }
    }

    public static UnitManager.direction getVectorDirection(Vector2 vector)
    {
        if (vector == Vector2.up)
        {
            return UnitManager.direction.up;
        }
        else if (vector == Vector2.down)
        {
            return UnitManager.direction.down;
        }
        else if (vector == Vector2.right)
        {
            return UnitManager.direction.right;
        }
        else if (vector == Vector2.left)
        {
            return UnitManager.direction.left;
        }
        else
        {
            Debug.LogError("No direction associated with vector: " + vector);
            return UnitManager.direction.up;
        }
    }
}
