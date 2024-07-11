using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaCapturePoint : TileCapturePoint
{
    const int ChangeManaIncomeBy = 10;

    public ManaCapturePoint(Vector2Int positionOfCapturePoint) : base (positionOfCapturePoint)
    {

    }

    protected override void playerNowControls(Player player)
    {
        player.changeManaIncome(ChangeManaIncomeBy);
    }

    protected override void playerNoLongerControls(Player player)
    {
        player.changeManaIncome(-ChangeManaIncomeBy);
    }

    public override capturePointType getCapturePointType()
    {
        return capturePointType.ManaCapturePoint;
    }
}
