using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileCapturePoint
{
    protected Vector2Int position;
    protected Player playerControlling;
    protected CapturePointWorld ourCapturePointWorld;
    public enum capturePointType { ManaCapturePoint }

    public TileCapturePoint(Vector2Int positionOfCapturePoint)
    {
        this.position = positionOfCapturePoint;
    }

    public void unitAtPoint(UnitWorld unit)
    {
        playerCaptured(unit.getAllegiance());
    }

    public Vector2Int getPosition()
    {
        return position;
    }

    private void playerCaptured(Player capturedBy)
    {
        playerNowControls(capturedBy);
        if (playerControlling != null) { playerNoLongerControls(playerControlling); }
        playerControlling = capturedBy;
    }

    private void setWorldRepresentation(CapturePointWorld to)
    {
        ourCapturePointWorld = to;
    }

    protected abstract void playerNowControls(Player player);

    protected abstract void playerNoLongerControls(Player player);

    public abstract capturePointType getCapturePointType();
}
