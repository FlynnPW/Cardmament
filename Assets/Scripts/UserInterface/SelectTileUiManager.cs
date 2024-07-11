using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectTileUiManager : MonoBehaviour
{
    public static SelectTileUiManager instance { get; private set; }
    [SerializeField]
    private Canvas worldSpaceCanvas;
    [SerializeField]
    private GameObject tileSelectionPrefab;
    private List<TileUi> tileUis = new List<TileUi>();

    void Start()
    {
        instance = GetComponent<SelectTileUiManager>();
    }

    public void createTileSelectionPrefab(Vector2Int position, TileUi.uiTileSelected callback)
    {
        TileUi newTileUi = Instantiate(tileSelectionPrefab, worldSpaceCanvas.transform).GetComponent<TileUi>();
        newTileUi.setupCallback(callback);
        newTileUi.transform.position = (Vector2)position;
        tileUis.Add(newTileUi);
    }

    public void clearTilesUis()
    {
        foreach (TileUi tileUi in tileUis)
        {
            Destroy(tileUi.gameObject);
        }

        tileUis.Clear();
    }
}
