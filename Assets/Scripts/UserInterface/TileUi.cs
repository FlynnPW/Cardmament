using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileUi : MonoBehaviour
{
    [SerializeField]
    private Image ourImage;
    public delegate void uiTileSelected();
    private uiTileSelected onSelected;

    public void setupCallback(uiTileSelected callback)
    {
        onSelected = callback;
    }

    public void selected()
    {
        onSelected();
    }
}
