using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    private int point;
    public int Point { get { return point; } set { point = value; } }

    float dir;
    public Color GetColor()
    {
        return transform.GetComponent<Renderer>().material.color;
    }

    public void BallExplose()
    {

    }
    public void Move(float _dir)
    {
        dir = _dir;
        StartCoroutine(StartMove());
    }

    IEnumerator StartMove()
    {
        float x = dir;

        float t = 0f;
        while ( true )
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(x, transform.position.y, transform.position.z), t);
            if ( transform.position.x >= 1f )
            {
                x = -1f;
                t = 0.01f;
            }
            else if ( transform.position.x <= -1f )
            {
                x = 1f;
                t = 0.01f;
            }

            t += 0.008f;

            yield return null;
        }
    }
}
