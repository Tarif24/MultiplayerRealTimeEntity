using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleClick : MonoBehaviour
{
    public Vector2 ScreenPercentage = new Vector2();

    void Start()
    {
        
    }
    void Update()
    {
        
    }
    void OnMouseDown()
    {
        NetworkClientProcessing.SendPoppedBalloon(ScreenPercentage);
    }
}
