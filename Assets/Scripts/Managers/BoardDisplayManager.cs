using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BoardDisplayManager : MonoBehaviour
{
    [SerializeField]
    private GameObject manaCapturePointPrefab;
    [SerializeField]
    private GameObject meshPrefab;
    [SerializeField]
    private GameObject boardParent;
    [SerializeField]
    private Material riverMaterial;
    [SerializeField]
    private Material mountainMaterial;
    [SerializeField]
    private Material groundMaterial;
    [SerializeField]
    private Material territoryMaterial;
    [SerializeField]
    private Camera mainCamera;
    private Dictionary<Player, territoryMesh> playerTerritoryMeshes;
    const float RIVER_MINIMUM_WIDTH = 0.2f; //from the middle so actual width will be * 2
    const float RIVER_MAXIMUM_WIDTH = 0.4f;
    const int AMOUNT_OF_MOUNTAINS_PER_TILE_MAX = 3;
    const int AMOUNT_OF_MOUNTAINS_PER_TILE_MIN = 1;
    const float MOUNTAIN_WIDTH_MAX = 0.2f;
    const float MOUNTAIN_WIDTH_MIN = 0.125f;
    const float MOUNTAIN_MAX_DISTANCE_FROM_TILE_CENTER = 0.25f;
    const int GROUND_LAYER = 0;
    const int RIVER_LAYER = 1;
    const int MOUNTAIN_LAYER = 2;

    struct territoryMesh
    {
        public MeshFilter meshFilter;
        public int[,] indexForTile;
        public bool modifiedThisFrame;
        public List<Vector2Int> removeTerritory;
        public List<Vector2Int> addTerritory;

        public territoryMesh(MeshFilter mesh)
        {
            this.meshFilter = mesh;
            Vector2Int mapSize = TileManager.instance.getMapSize();
            indexForTile = new int[mapSize.x, mapSize.y];
            modifiedThisFrame = false;
            removeTerritory = new List<Vector2Int>();
            addTerritory = new List<Vector2Int>();
        }
    }

    private void Start()
    {
        playerTerritoryMeshes = new Dictionary<Player, territoryMesh>();
    }

    private void Update()
    {
        foreach (territoryMesh territory in playerTerritoryMeshes.Values)
        {
            Vector3[] vertices = new Vector3[territory.meshFilter.mesh.vertexCount];
            int[] triangles = new int[territory.meshFilter.mesh.triangles.Length];
            Vector2[] uv = new Vector2[territory.meshFilter.mesh.uv.Length];

            if (territory.modifiedThisFrame == false)
            {
                return;
            }
            
            foreach (Vector2Int remove in territory.removeTerritory)
            {
                int index = territory.indexForTile[remove.x, remove.y];
               // vertices[index * 4] = ;
            }
        }
    }

    public void renderMap(List<Vector2Int>[] riversCoordinates, List<Vector2Int>[] mountainCoordinates, TileCapturePoint[] capturePoints, Vector2Int mapSize, Dictionary<Tile, List<Vector2Int>> mapTiles)
    {
        foreach (List<Vector2Int> river in riversCoordinates)
        {
            createRiver(river);
        }

        foreach (List<Vector2Int> mountain in mountainCoordinates)
        {
            createMountain(mountain);
        }

        createGround(mapSize, mapTiles);

        foreach (TileCapturePoint capturePoint in capturePoints)
        {
            createManaCapturePoint(capturePoint);
        }

        mainCamera.transform.position = new Vector3((mapSize.x - 0.5f) / 2, (mapSize.y - 0.5f) / 2, -10);
    }

    public void playerAddedToDisplay(Player player, Color colourOfPlayer)
    {
        MeshFilter territoryMeshFilter = Instantiate(meshPrefab, boardParent.transform).GetComponent<MeshFilter>();
        territoryMeshFilter.GetComponent<MeshRenderer>().material = territoryMaterial;
        Mesh playerTerritoryMesh = new Mesh();
        territoryMeshFilter.mesh = playerTerritoryMesh;
        territoryMesh territoryMesh = new territoryMesh(territoryMeshFilter);
        playerTerritoryMeshes.Add(player, territoryMesh);
    }

    void createManaCapturePoint(TileCapturePoint capturePoint)
    {
        switch (capturePoint.getCapturePointType())
        {
            case TileCapturePoint.capturePointType.ManaCapturePoint:
                CapturePointDisplay capturePointWorld = Instantiate(manaCapturePointPrefab, boardParent.transform).GetComponent<CapturePointDisplay>();
                capturePointWorld.transform.position = (Vector2)capturePoint.getPosition();
                break;
        }
    }

    void createGround(Vector2Int mapSize, Dictionary<Tile, List<Vector2Int>> tiles)
    {
        //we have one extra row and colum of vertices
        //0,1 -- 1,1
        // |  0,0 |
        //0,0 -- 1,0
        Vector2[,] globalVertices = new Vector2[mapSize.x + 1, mapSize.y + 1];

        for (int x = 0; x < mapSize.x + 1; x++)
        {
            for (int y = 0; y < mapSize.y + 1; y++)
            {
                globalVertices[x, y] = new Vector2(x - 0.5f, y - 0.5f);
            }
        }

        foreach (Tile createMeshFor in tiles.Keys)
        {
            MeshFilter groundToApplyMesh = Instantiate(meshPrefab, boardParent.transform).GetComponent<MeshFilter>();
            groundToApplyMesh.GetComponent<MeshRenderer>().material = createMeshFor.getMaterial();
            groundToApplyMesh.GetComponent<SortingGroup>().sortingOrder = GROUND_LAYER;

            List<Vector2Int> tileCoordinates = tiles[createMeshFor];
            Mesh Mesh = new Mesh();
            int quads = tileCoordinates.Count;
            Vector3[] vertices = new Vector3[4 * quads];
            Vector2[] uv = new Vector2[4 * quads];
            int[] triangles = new int[6 * quads];

            for(int i = 0; i < tileCoordinates.Count; i++)
            {
                int QuadOn = i;
                Vector2Int tileCoordinate = tileCoordinates[QuadOn];

                int bottomLeft = QuadOn * 4;
                int topLeft = QuadOn * 4 + 1;
                int topRight = QuadOn * 4 + 2;
                int bottomRight = QuadOn * 4 + 3;

                vertices[bottomLeft] = globalVertices[tileCoordinate.x, tileCoordinate.y];
                vertices[topLeft] = globalVertices[tileCoordinate.x, tileCoordinate.y + 1];
                vertices[topRight] = globalVertices[tileCoordinate.x + 1, tileCoordinate.y + 1];
                vertices[bottomRight] = globalVertices[tileCoordinate.x + 1, tileCoordinate.y];

                uv[QuadOn * 4] = new Vector3(0, 0);
                uv[QuadOn * 4 + 1] = new Vector3(0, 1);
                uv[QuadOn * 4 + 2] = new Vector3(1, 1);
                uv[QuadOn * 4 + 3] = new Vector3(1, 0);

                triangles[QuadOn * 6] = bottomLeft;
                triangles[QuadOn * 6 + 1] = topLeft;
                triangles[QuadOn * 6 + 2] = topRight;
                triangles[QuadOn * 6 + 3] = bottomLeft;
                triangles[QuadOn * 6 + 4] = topRight;
                triangles[QuadOn * 6 + 5] = bottomRight;
            }

            Mesh.vertices = vertices;
            Mesh.uv = uv;
            Mesh.triangles = triangles;

            groundToApplyMesh.mesh = Mesh;
        }
    }

    void createRiver(List<Vector2Int> riverCoordinates)
    {
        MeshFilter riverToApplyMesh = Instantiate(meshPrefab, boardParent.transform).GetComponent<MeshFilter>();
        riverToApplyMesh.GetComponent<MeshRenderer>().material = riverMaterial;
        riverToApplyMesh.GetComponent<SortingGroup>().sortingOrder = RIVER_LAYER;

        Mesh Mesh = new Mesh();
        tileRiverVertices[] riverVertices = new tileRiverVertices[riverCoordinates.Count];
        //mesh variables
        int quads = riverCoordinates.Count * 2 - 1;
        Vector3[] vertices = new Vector3[4 * quads];
        Vector2[] uv = new Vector2[4 * quads];
        int[] triangles = new int[6 * quads];

        for (int i = 0; i < riverCoordinates.Count; i++)
        {
            Vector2Int riverCoordinate = riverCoordinates[i];
            int bottomLeft = i * 8;
            int topLeft = i * 8 + 1;
            int topRight = i * 8 + 2;
            int bottomRight = i * 8 + 3;

            vertices[bottomLeft] = riverCoordinate + new Vector2(-Random.Range(RIVER_MINIMUM_WIDTH, RIVER_MAXIMUM_WIDTH), -Random.Range(RIVER_MINIMUM_WIDTH, RIVER_MAXIMUM_WIDTH));
            vertices[topLeft] = riverCoordinate + new Vector2(-Random.Range(RIVER_MINIMUM_WIDTH, RIVER_MAXIMUM_WIDTH), Random.Range(RIVER_MINIMUM_WIDTH, RIVER_MAXIMUM_WIDTH));
            vertices[topRight] = riverCoordinate + new Vector2(Random.Range(RIVER_MINIMUM_WIDTH, RIVER_MAXIMUM_WIDTH), Random.Range(RIVER_MINIMUM_WIDTH, RIVER_MAXIMUM_WIDTH));
            vertices[bottomRight] = riverCoordinate + new Vector2(Random.Range(RIVER_MINIMUM_WIDTH, RIVER_MAXIMUM_WIDTH), -Random.Range(RIVER_MINIMUM_WIDTH, RIVER_MAXIMUM_WIDTH));

            riverVertices[i] = new tileRiverVertices(topLeft, topRight, bottomLeft, bottomRight, vertices[topLeft], vertices[topRight], vertices[bottomLeft], vertices[bottomRight]);

            uv[i * 8] = new Vector3(0, 0);
            uv[i * 8 + 1] = new Vector3(0, 1);
            uv[i * 8 + 2] = new Vector3(1, 1);
            uv[i * 8 + 3] = new Vector3(1, 0);

            triangles[i * 12] = bottomLeft;
            triangles[i * 12 + 1] = topLeft;
            triangles[i * 12 + 2] = topRight;
            triangles[i * 12 + 3] = bottomLeft;
            triangles[i * 12 + 4] = topRight;
            triangles[i * 12 + 5] = bottomRight;

            if (i != 0)
            {
                UnitManager.direction directionRiverMoves = TileManager.getVectorDirection(riverCoordinates[i] - riverCoordinates[i - 1]);

                uv[(i - 1) * 8 + 4] = new Vector3(0, 0);
                uv[(i - 1) * 8 + 5] = new Vector3(0, 1);
                uv[(i - 1) * 8 + 6] = new Vector3(1, 1);
                uv[(i - 1) * 8 + 7] = new Vector3(1, 0);

                triangles[(i - 1) * 12 + 6] = (i - 1) * 8 + 4;
                triangles[(i - 1) * 12 + 7] = (i - 1) * 8 + 5;
                triangles[(i - 1) * 12 + 8] = (i - 1) * 8 + 6;
                triangles[(i - 1) * 12 + 9] = (i - 1) * 8 + 4;
                triangles[(i - 1) * 12 + 10] = (i - 1) * 8 + 6;
                triangles[(i - 1) * 12 + 11] = (i - 1) * 8 + 7;

                switch (directionRiverMoves)
                {
                    case UnitManager.direction.up:
                        vertices[(i - 1) * 8 + 4] = riverVertices[i - 1].topLeftVertexPosition;
                        vertices[(i - 1) * 8 + 5] = vertices[bottomLeft];
                        vertices[(i - 1) * 8 + 6] = vertices[bottomRight];
                        vertices[(i - 1) * 8 + 7] = riverVertices[i - 1].topRightVertexPosition;
                        break;
                    case UnitManager.direction.down:
                        vertices[(i - 1) * 8 + 4] = vertices[topLeft];
                        vertices[(i - 1) * 8 + 5] = riverVertices[i - 1].bottomLeftVertexPosition;
                        vertices[(i - 1) * 8 + 6] = riverVertices[i - 1].bottomRightVertexPosition;
                        vertices[(i - 1) * 8 + 7] = vertices[topRight];
                        break;
                    case UnitManager.direction.left:
                        vertices[(i - 1) * 8 + 4] = vertices[bottomRight];
                        vertices[(i - 1) * 8 + 5] = vertices[topRight];
                        vertices[(i - 1) * 8 + 6] = riverVertices[i - 1].topLeftVertexPosition;
                        vertices[(i - 1) * 8 + 7] = riverVertices[i - 1].bottomLeftVertexPosition;
                        break;
                    case UnitManager.direction.right:
                        vertices[(i - 1) * 8 + 4] = riverVertices[i - 1].bottomRightVertexPosition;
                        vertices[(i - 1) * 8 + 5] = riverVertices[i - 1].topRightVertexPosition;
                        vertices[(i - 1) * 8 + 6] = vertices[topLeft];
                        vertices[(i - 1) * 8 + 7] = vertices[bottomLeft];
                        break;
                }
            }
        }

        Mesh.vertices = vertices;
        Mesh.uv = uv;
        Mesh.triangles = triangles;

        riverToApplyMesh.mesh = Mesh;
    }

    void createMountain(List<Vector2Int> mountainCoordinates)
    {
        MeshFilter mountainToApplyMesh = Instantiate(meshPrefab, boardParent.transform).GetComponent<MeshFilter>();
        mountainToApplyMesh.GetComponent<MeshRenderer>().material = mountainMaterial;
        mountainToApplyMesh.GetComponent<SortingGroup>().sortingOrder = MOUNTAIN_LAYER;

        Mesh Mesh = new Mesh();
        int[] trianglesOnEachMountainTile = new int[mountainCoordinates.Count];
        int trianglesTotal = 0;

        for (int i = 0; i < trianglesOnEachMountainTile.Length; i++)
        {
            int localTriangles = Random.Range(AMOUNT_OF_MOUNTAINS_PER_TILE_MIN, AMOUNT_OF_MOUNTAINS_PER_TILE_MAX + 1);
            trianglesOnEachMountainTile[i] = localTriangles;
            trianglesTotal += localTriangles;
        }

        
        Vector3[] vertices = new Vector3[3 * trianglesTotal];
        Vector2[] uv = new Vector2[3 * trianglesTotal];
        int[] triangles = new int[3 * trianglesTotal];
        int triangleOn = 0;

        for (int mountainTileIndex = 0; mountainTileIndex < mountainCoordinates.Count; mountainTileIndex++)
        {
            int amountOfMountainsOnTile = trianglesOnEachMountainTile[mountainTileIndex];

            for (int mountianOnTileIndex = 0; mountianOnTileIndex < amountOfMountainsOnTile; mountianOnTileIndex++)
            {
                Vector2 offset = mountainCoordinates[mountainTileIndex] + new Vector2(
                Random.Range(-MOUNTAIN_MAX_DISTANCE_FROM_TILE_CENTER, MOUNTAIN_MAX_DISTANCE_FROM_TILE_CENTER),
                Random.Range(-MOUNTAIN_MAX_DISTANCE_FROM_TILE_CENTER, MOUNTAIN_MAX_DISTANCE_FROM_TILE_CENTER));
                float width = Random.Range(MOUNTAIN_WIDTH_MIN, MOUNTAIN_WIDTH_MAX);
                float height = Random.Range(MOUNTAIN_WIDTH_MIN, MOUNTAIN_WIDTH_MAX);
                vertices[triangleOn * 3] = new Vector2(-width, -height) + offset;
                vertices[triangleOn * 3 + 1] = new Vector2(0, height) + offset;
                vertices[triangleOn * 3 + 2] = new Vector2(width, -height) + offset;
                uv[triangleOn * 3] = new Vector3(0, 0);
                uv[triangleOn * 3] = new Vector3(0.5f, 1);
                uv[triangleOn * 3] = new Vector3(1, 0);
                triangles[triangleOn * 3] = triangleOn * 3;
                triangles[triangleOn * 3 + 1] = triangleOn * 3 + 1;
                triangles[triangleOn * 3 + 2] = triangleOn * 3 + 2;
                triangleOn++;
            } 
        }

        Mesh.vertices = vertices;
        Mesh.uv = uv;
        Mesh.triangles = triangles;

        mountainToApplyMesh.mesh = Mesh;
    }

    public void territoryExpanded(Player whoExpanded, Vector2Int where)
    {
        territoryMesh expandMesh = playerTerritoryMeshes[whoExpanded];
        expandMesh.addTerritory.Add(where);
        expandMesh.modifiedThisFrame = true;
    }

    public void territoryShrunk(Player whoShrunk, Vector2Int where)
    {
        territoryMesh expandMesh = playerTerritoryMeshes[whoShrunk];
        expandMesh.removeTerritory.Add(where);
        expandMesh.modifiedThisFrame = true;
    }

    struct tileRiverVertices
    {
        public int topLeftVertex;
        public int topRightVertex;
        public int bottomLeftVertex;
        public int bottomRightVertex;
        public Vector2 topLeftVertexPosition;
        public Vector2 topRightVertexPosition;
        public Vector2 bottomLeftVertexPosition;
        public Vector2 bottomRightVertexPosition;

        public tileRiverVertices(int topLeftVertex, int topRightVertex, int bottomLeftVertex, int bottomRightVertex, Vector2 topLeftVertexPosition, Vector2 topRightVertexPosition, Vector2 bottomLeftVertexPosition, Vector2 bottomRightVertexPosition)
        {
            this.topLeftVertex = topLeftVertex;
            this.topRightVertex = topRightVertex;
            this.bottomLeftVertex = bottomLeftVertex;
            this.bottomRightVertex = bottomRightVertex;
            this.topLeftVertexPosition = topLeftVertexPosition;
            this.topRightVertexPosition = topRightVertexPosition;
            this.bottomLeftVertexPosition = bottomLeftVertexPosition;
            this.bottomRightVertexPosition = bottomRightVertexPosition;
        }
    }
}
