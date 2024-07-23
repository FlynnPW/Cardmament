using UnityEngine;

public class Flag : MonoBehaviour
{
    private LineRenderer ourLinerenderer;
    private float currentFlagFlapTime;
    const float FLAG_LENGTH = 0.5f;
    const float FLAG_FLAP_DISTANCE = 0.05f;
    const float FLAG_FLAP_TIME = 0.5f;

    void Start()
    {
        ourLinerenderer = GetComponent<LineRenderer>();
        ourLinerenderer.SetPosition(0, transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 1; i < ourLinerenderer.positionCount; i++)
        {
            float positionInSinCurve = (currentFlagFlapTime / FLAG_FLAP_TIME) + ((float)i / ourLinerenderer.positionCount);
            ourLinerenderer.SetPosition(i, new Vector2(
            transform.position.x + (FLAG_LENGTH / ourLinerenderer.positionCount) * i, 
            transform.position.y + Mathf.Sin(Mathf.PI * 2 * positionInSinCurve) * FLAG_FLAP_DISTANCE));
        }

        currentFlagFlapTime += Time.deltaTime;

        if (currentFlagFlapTime > FLAG_FLAP_TIME)
        {
            currentFlagFlapTime -= FLAG_FLAP_TIME;
        }
    }
}
