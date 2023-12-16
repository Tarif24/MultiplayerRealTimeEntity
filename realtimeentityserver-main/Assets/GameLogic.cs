using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    float durationUntilNextBalloon;
    int intervalBetweenSpawns = 3;
    public bool isClientIn = false;

    public List<Vector2> allBallons = new List<Vector2>();

    void Start()
    {
        NetworkServerProcessing.SetGameLogic(this);
        durationUntilNextBalloon = intervalBetweenSpawns;
    }

    void Update()
    {
        durationUntilNextBalloon -= Time.deltaTime;

        if (durationUntilNextBalloon < 0 && isClientIn)
        {
            durationUntilNextBalloon = intervalBetweenSpawns;

            float screenPositionXPercent = Random.Range(0.0f, 1.0f);
            float screenPositionYPercent = Random.Range(0.0f, 1.0f);
            Vector2 screenPercentage = new Vector2(screenPositionXPercent, screenPositionYPercent);

            NetworkServerProcessing.SendNewBalloonToClient(screenPercentage);

            allBallons.Add(screenPercentage);
        }

    }

}
