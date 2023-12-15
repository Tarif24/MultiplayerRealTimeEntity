using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    float durationUntilNextBalloon;
    Sprite circleTexture;
    public Dictionary<Vector2, GameObject> allBallons = new Dictionary<Vector2, GameObject>();

    void Start()
    {
        NetworkClientProcessing.SetGameLogic(this);
    }

    void Update()
    {
        
    }

    public void SpawnNewBalloon(Vector2 screenPosition)
    {
        if (circleTexture == null)
            circleTexture = Resources.Load<Sprite>("Circle");

        GameObject balloon = new GameObject("Balloon");

        balloon.AddComponent<SpriteRenderer>();
        balloon.GetComponent<SpriteRenderer>().sprite = circleTexture;
        balloon.AddComponent<CircleClick>();
        balloon.AddComponent<CircleCollider2D>();

        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        pos.z = 0;
        balloon.transform.position = pos;

        allBallons.Add(pos, balloon);
        //go.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
    }

    public void DestroyBalloon(Vector2 location)
    {
        Destroy(allBallons[location]);
    }
}
