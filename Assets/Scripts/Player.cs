using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public ParticleSystem particle;

    private Renderer rend;
    private TrailRenderer trailRenderer;
    Camera followCamera;
    float speed = 17f;

    int[] xPositons;
    int positionIndex;
    float alpha = 1.0f;

    bool right, left;
    Color myColor;

    int score;
    int coinCount;
    Vector3 direction;

    private Vector3 startTouchPositon;
    private Vector3 currentTouchPositon;
    private bool stopTouch = false;
    void Start()
    {
        score = 0;
        coinCount = PlayerPrefs.GetInt("coin");
        int skinIndex = Random.Range(0, Maneger.Instance.colors.Length);

        followCamera = Camera.main;

        //followCamera.transform.parent = transform;
        direction = followCamera.transform.position - transform.position;

        rend = GetComponent<Renderer>();
        trailRenderer = GetComponent<TrailRenderer>();

        myColor = Maneger.Instance.GiveColor(skinIndex);
        rend.material.SetColor("_Color", myColor);

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Maneger.Instance.GiveColor(skinIndex), 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        trailRenderer.colorGradient = gradient;

        xPositons = new int[3];
        xPositons[0] = -1;
        xPositons[1] = 0;
        xPositons[2] = 1;

        positionIndex = 1;

        right = false;
        left = false;
    }


    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        followCamera.transform.position = transform.position + direction;
        if ( Input.GetKeyDown(KeyCode.D) && positionIndex != 2 )
        {
            positionIndex++;
            right = true;
            left = false;
        }
        if ( Input.GetKeyDown(KeyCode.A) && positionIndex != 0 )
        {
            positionIndex--;
            left = true;
            right = false;
        }

        if ( Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began )
        {
            startTouchPositon = Input.GetTouch(0).position;
        }

        if ( Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved )
        {
            currentTouchPositon = Input.GetTouch(0).position;
            Vector3 distance = currentTouchPositon - startTouchPositon;
            if ( !stopTouch )
            {
                if ( distance.x > 50 && positionIndex != 2 )
                {
                    positionIndex++;
                    right = true;
                    left = false;
                    stopTouch = true;
                    Maneger.Instance.SetCoinText(46);
                }
                else if ( distance.x < -50 && positionIndex != 0 )
                {
                    positionIndex--;
                    left = true;
                    right = false;
                    stopTouch = true;
                }
            }  
        }
        if ( Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended )
        {
            stopTouch = false;
        }

        if ( right ) //&& transform.position.x != positionIndex )
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(xPositons[positionIndex], transform.position.y, transform.position.z), Time.deltaTime * speed);
        }

        if ( left )// && transform.position.x != positionIndex )
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(xPositons[positionIndex], transform.position.y, transform.position.z), Time.deltaTime * speed);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if ( collision.gameObject.name == "ramp" )
        {
            Color rampColor = collision.gameObject.GetComponent<Renderer>().material.color;
            rend.material.SetColor("_Color", rampColor);
            myColor = rampColor;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(rampColor, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
            trailRenderer.colorGradient = gradient;

            Maneger.Instance.ChangePath();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if ( other.CompareTag("Ball") )
        {

            Ball ball = other.gameObject.GetComponent<Ball>();
            Color ballColor = ball.GetColor();

            ParticleSystem par = Instantiate(particle, ball.transform.position, particle.transform.rotation);
            var settings = par.main;

            Color color = new Color32(9, 251, 122, 128);
            settings.startColor = color;
            par.Play();
            Destroy(par.gameObject, 1f);
            if ( myColor == ballColor )
            {
                score += ball.Point;
                Maneger.Instance.SetText(score);
                Destroy(other.gameObject);
            }
            else
            {
                Maneger.Instance.restartButton.gameObject.SetActive(true);

                if ( score > PlayerPrefs.GetInt("bestScore") )
                {
                    PlayerPrefs.SetInt("bestScore", score);
                    Maneger.Instance.SetBestText();
                }
                Destroy(gameObject);
            }


        }
        else if ( other.CompareTag("Coin") )
        {
            coinCount++;
            PlayerPrefs.SetInt("coin", coinCount);
            Maneger.Instance.SetCoinText(coinCount);
            Destroy(other.gameObject);
        }
    }


}
