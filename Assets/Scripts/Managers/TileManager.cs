using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileManager : MonoBehaviour
{
    public static TileManager instance { get; private set; }
    public enum tilePassableStatus { passable, unitBlocked, tileBlocked }
    private Vector2Int mapSize;
    private TileOnBoard[,] mapTiles;
    private delegate void tileSetTo(Vector2Int tile, Tile to);
    private delegate void tileFeatureSetTo(Vector2Int tile, TileFeature to);
    private tileSetTo tileSetToCallback;
    private tileFeatureSetTo tileFeatureSetToCallback;
    private Vector2Int[] neighbours = new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1) };
    [SerializeField]
    private Tile[] tiles;
    [SerializeField]
    private TileFeature river;
    [SerializeField]
    private TileFeature mountain;
    [SerializeField]
    private GameObject tilePrefab;
    private BoardDisplayManager boardDisplayManager;
    private PlayersManager playersManagers;
    //RANDOM GENERATION VALUES
    const int MAX_RIVERS = 2;
    const int MIN_RIVERS = 0;
    const int MAX_MOUNTAINS = 3;
    const int MIN_MOUNTAINS = 1;
    const int MAX_MAP_SIZE = 8;
    const int MIN_MAP_SIZE = 8;
    const int AMOUNT_OF_MANA_CAPTURE_POINTS = 4;
    const float BIOME_THRESHOLD = 0.5f;
    const float PERLIN_NOISE_SLICE = 0.1f;

    private void Start()
    {
        instance = GetComponent<TileManager>();
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
        List<Vector2Int> emptyTiles = new List<Vector2Int>();
        List<Vector2Int>[] allRiversCoordinates;
        List<Vector2Int>[] allMountainsCoordiantes;
        TileCapturePoint[] allCapturePoints = null;
        Dictionary<Tile, List<Vector2Int>> tileGroups = new Dictionary<Tile, List<Vector2Int>>();

        foreach (Tile tile in tiles)
        {
            tileGroups.Add(tile, new List<Vector2Int>());
        }

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                emptyTiles.Add(new Vector2Int(x, y));
            }
        }

        Vector2Int getRandomClearTile(List<Vector2Int> sublist)
        {
            if (sublist == null)
            {
                sublist = emptyTiles;
            }

            int randomClearTileIndex = Random.Range(0, sublist.Count - 1);
            Vector2Int tile = sublist[randomClearTileIndex];
            emptyTiles.Remove(tile);
            return tile;
        }

        void tileFeatureSetTo(Vector2Int position, TileFeature to)
        {
            if (to != null && emptyTiles.Contains(position))
            {
                emptyTiles.Remove(position);
            }
        }

        tileFeatureSetToCallback += tileFeatureSetTo;

        void createTiles()
        {
            List<Vector2Int> unvisited = new List<Vector2Int>();

            for (int x = 0; x < mapSize.x; x++) //probably should add a list with all coords that is just copied but do this for now
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    unvisited.Add(new Vector2Int(x, y));
                }
            }

            bool belowThreshold(Vector2Int coordinate)
            {
                float getRandomPosition()
                {
                    return Random.Range(0, 1 - PERLIN_NOISE_SLICE / 2);
                }

                Vector2 getFromPosition = new Vector2(getRandomPosition(), getRandomPosition());
                float xPosition = coordinate.x / mapSize.x;
                float yPosition = coordinate.y / mapSize.y;
                return BIOME_THRESHOLD > Mathf.PerlinNoise(getFromPosition.x + xPosition * PERLIN_NOISE_SLICE, getFromPosition.y + yPosition * PERLIN_NOISE_SLICE);
            }

            List<List<Vector2Int>> tilesOfEachBiome = new List<List<Vector2Int>>();

            //we will traverse through each tile of the map, but split it into individual biomes
            while (unvisited.Count > 0)
            {
                List<Vector2Int> toiVsit = new List<Vector2Int>();
                Vector2Int root = unvisited[0];
                toiVsit.Add(root);
                bool rootTileBelowThreshold = belowThreshold(root);

                tilesOfEachBiome.Add(new List<Vector2Int>());
                List<Vector2Int> addToList = tilesOfEachBiome.Last();

                while (toiVsit.Count > 0)
                {
                    Vector2Int tileAt = toiVsit[0];
                    unvisited.Remove(tileAt);
                    addToList.Add(tileAt);
                    toiVsit.RemoveAt(0);

                    foreach (Vector2Int neighbour in getNeighbouringTiles(tileAt))
                    {
                        if (belowThreshold(neighbour) != rootTileBelowThreshold || addToList.Contains(neighbour) || toiVsit.Contains(neighbour))
                        {
                            continue;
                        }

                        toiVsit.Add(neighbour);
                    }
                }
            }

            for (int i = 0; i < tilesOfEachBiome.Count; i++)
            {
                int randomBiomeIndex = Random.Range(0, tiles.Length);

                Tile biomeTile = tiles[randomBiomeIndex];
                List<Vector2Int> biomesCoordinates = tilesOfEachBiome[i];
                tileGroups[biomeTile].AddRange(biomesCoordinates);

                foreach (Vector2Int setToTile in biomesCoordinates)
                {
                    setTileTo(setToTile, biomeTile);
                }
            }
        }

        void createHomeZones()
        {
            if (playersOnMap.Length != 2)
            {
                Debug.LogError("Homezones function only compatible with 2 players.");
            }

            for (int x = 0; x < mapSize.x; x++)
            {
                getTile(new Vector2Int(x, 0)).setHomeZoneOf(playersOnMap[0]);
                getTile(new Vector2Int(x, mapSize.y - 1)).setHomeZoneOf(playersOnMap[1]);
            }
        }

        void createCapturePoints()
        {
            List<Vector2Int> potentialCapturePointTiles = new List<Vector2Int>();
            potentialCapturePointTiles.AddRange(emptyTiles);
            potentialCapturePointTiles.RemoveAll(potentialCapturePointTile => getTile(potentialCapturePointTile).getHomeZoneOf() != null); //we should not place on the home zones

            allCapturePoints = new TileCapturePoint[AMOUNT_OF_MANA_CAPTURE_POINTS];

            for (int manaPoint = 0; manaPoint < AMOUNT_OF_MANA_CAPTURE_POINTS; manaPoint++)
            {
                foreach (Vector2Int tile in potentialCapturePointTiles)
                {
                    if (emptyTiles.Contains(tile) == false)
                    {
                        print("wtf?");
                        print(tile);
                    }
                }

                if (potentialCapturePointTiles.Count == 0)
                {
                    Debug.LogError("Ran out of locations to place capture points!");
                }

                Vector2Int placeCapturePointAt = getRandomClearTile(potentialCapturePointTiles);

                TileCapturePoint newCapturePoint = new ManaCapturePoint(placeCapturePointAt);
                getTile(placeCapturePointAt).setCapturePoint(newCapturePoint); //set to mana point           

                void removeCoordinatesFromList(Vector2Int removeAround)
                {
                    potentialCapturePointTiles.RemoveAll(potentialCapturePointTile =>
                    potentialCapturePointTile.x == removeAround.x || potentialCapturePointTile.y == removeAround.y ||
                    (math.abs(potentialCapturePointTile.x - removeAround.x) == 1 && math.abs(potentialCapturePointTile.y - removeAround.y) == 1));
                }

                removeCoordinatesFromList(placeCapturePointAt);

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

                Vector2Int getRandomStartingTile()
                {
                    bool isTileAtEdge(Vector2Int tile)
                    {
                        return tile.x == 0 || tile.x == mapSize.x - 1 || tile.y == 0 || tile.y == mapSize.y - 1;
                    }

                    List<Vector2Int> potentialTiles = new List<Vector2Int>();
                    potentialTiles.AddRange(emptyTiles);

                    potentialTiles.RemoveAll(potentialTile =>
                    getTile(potentialTile).getHomeZoneOf() != null ||
                    isTileAtEdge(potentialTile) == false ||
                    isTileFeatureOfType(potentialTile, null) == false
                    );

                    if (potentialTiles.Count == 0)
                    {
                        Debug.Log("No tiles spots remaining!");
                        return new Vector2Int(-1, -1);
                    }

                    return getRandomClearTile(potentialTiles);
                }

                void addRiverToTile(Vector2Int at)
                {
                    setTileFeatureTo(at, river);
                    riverCoordinates.Add(at);
                }

                Vector2Int riverTile = getRandomStartingTile();

                if (riverTile == new Vector2Int(-1, -1))
                {
                    Debug.LogError("Cannot find place to start river");
                    return;
                }

                UnitManager.direction riverEdge = UnitManager.direction.down;

                if (riverTile.x == 0)
                {
                    riverEdge = UnitManager.direction.left;
                }
                else if (riverTile.x == mapSize.x - 1)
                {
                    riverEdge = UnitManager.direction.right;
                }
                else if (riverTile.y == 0)
                {
                    riverEdge = UnitManager.direction.down;
                }
                else if (riverTile.y == mapSize.y - 1)
                {
                    riverEdge = UnitManager.direction.up;
                }

                addRiverToTile(riverTile);

                bool riverContinue = true;
                int riverLength = 1;

                bool closerToEdge(Vector2Int neighbour, Vector2Int tile, UnitManager.direction edge)
                {
                    switch (edge)
                    {
                        case UnitManager.direction.left:
                            return neighbour.x < tile.x;
                        case UnitManager.direction.down:
                            return neighbour.y < tile.y;
                        case UnitManager.direction.right:
                            return neighbour.x > tile.x;
                        case UnitManager.direction.up:
                            return neighbour.y > tile.y;
                        default:
                            Debug.LogError("River edge not assigned");
                            return false;
                    }
                }

                bool atOppositeEdge(Vector2Int tile, UnitManager.direction edge)
                {
                    switch (edge)
                    {
                        case UnitManager.direction.left:
                            return tile.x == mapSize.x - 1;
                        case UnitManager.direction.down:
                            return tile.y == mapSize.y - 1;
                        case UnitManager.direction.right:
                            return tile.x == 0;
                        case UnitManager.direction.up:
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
                    neighbouringWaterTile(neighbour, neighbour.y != riverTile.y, neighbour.x != riverTile.x) || isTileFeatureOfType(neighbour, null) == false || closerToEdge(neighbour, riverTile, riverEdge));

                    if (neighbours.Count == 0)
                    {
                        riverContinue = false;
                        print("river stopped due to no vaiable expansions at " + riverTile);
                        continue;
                    }

                    int randomNeighbourIndex = Random.Range(0, neighbours.Count);
                    Vector2Int chosenNeighbour = neighbours[randomNeighbourIndex];
                    riverTile = chosenNeighbour;
                    addRiverToTile(riverTile);
                    riverLength++;

                    if (atOppositeEdge(riverTile, riverEdge))
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
                Vector2Int mountainPosition = getRandomClearTile(null);

                void addMountainTile(Vector2Int at)
                {
                    setTileFeatureTo(at, mountain);
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

        createTiles();
        createHomeZones();
        createRivers();
        createMountains();
        createCapturePoints();
        tileFeatureSetToCallback -= tileFeatureSetTo;
        UnitManager.instance.mapSet(mapSize);
        boardDisplayManager.renderMap(allRiversCoordinates, allMountainsCoordiantes, allCapturePoints, mapSize, tileGroups);
    }

    public bool isAvaliableTileToPlace(Vector2Int at, Player toCheck, bool emptyValid)
    {
        return (mapTiles[at.x, at.y].getHomeZoneOf() == toCheck) || (mapTiles[at.x, at.y].getHomeZoneOf() == null && emptyValid);
    }

    private bool neighbouringWaterTile(Vector2Int at, bool horizontal, bool vertical)
    {
        if (horizontal) //are we above or below?
        {
            return isTileFeatureOfType(new Vector2Int(at.x - 1, at.y), river) || isTileFeatureOfType(new Vector2Int(at.x + 1, at.y), river); //horizontal to another water tile?
        }
        else if (vertical)
        {
            return isTileFeatureOfType(new Vector2Int(at.x, at.y + 1), river) || isTileFeatureOfType(new Vector2Int(at.x, at.y + 1), river); //vertical to another water tile?
        }

        return false;
    }

    private void setTileTo(Vector2Int positionToSet, Tile to)
    {
        mapTiles[positionToSet.x, positionToSet.y] = new TileOnBoard(to);
    }

    private void setTileFeatureTo(Vector2Int position, TileFeature to)
    {
        TileOnBoard tile = getTile(position);
        tile.addFeature(to);
        tileFeatureSetToCallback(position, to);
    }

    public List<Vector2Int> getNeighbouringTiles(Vector2Int from)
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

    private bool isTileFeatureOfType(Vector2Int positionOfTile, TileFeature featureCheck)
    {
        if (isTileWithinBounds(positionOfTile) == false)
        {
            return false;
        }

        return mapTiles[positionOfTile.x, positionOfTile.y].getFeature() == featureCheck;
    }

    public bool isTileWithinBounds(Vector2Int toCheck)
    {
        return (toCheck.x < 0 || toCheck.x > mapSize.x - 1 || toCheck.y < 0 || toCheck.y > mapSize.y - 1) == false;
    }

    public Vector2Int getMapSize()
    {
        return mapSize;
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
    public bool isTileCurrentlyPassable(Vector2Int tilePosition)
    {
        if (isTileWithinBounds(tilePosition) == false)
        {
            return false;
        }

        TileOnBoard tileCheck = getTile(tilePosition);
        return tileCheck.isTilePassable(); 
    }

    public void unitEnteredTile(Vector2Int tilePosition)
    {
        TileOnBoard tileEntered = getTile(tilePosition);
        tileEntered.unitNowOn();
    }

    public void unitLeftTile(Vector2Int tilePosition)
    {
        TileOnBoard tileLeft = getTile(tilePosition);
        tileLeft.unitNowOff();
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
