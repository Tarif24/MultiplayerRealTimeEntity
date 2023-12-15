using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleClick : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    void OnMouseDown()
    {
        NetworkClientProcessing.SendPoppedBalloon(transform.position);
    }
}
