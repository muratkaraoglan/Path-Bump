using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class Maneger : MonoBehaviour
{
    private static Maneger instance;
    private System.Random random = new System.Random();
    public static Maneger Instance { get { return instance; } }


    [Header("UI Texts")]
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI coinText;
    public TMPro.TextMeshProUGUI bestScoreText;

    [Header("Player Settings")]
    public Transform player;
    public Vector3 startPoint;

    [Header("Menu Item")]
    public Button playButton;
    public Button restartButton;
    public Button settingsButton;

    [Header("Path Item")]
    public Vector3 pathPositionOffset;
    public Transform path;

    [Header("Collectable Items")]
    public Transform ball;
    public Transform coin;

    [Header("Colors")]
    public Color[] colors;

    Queue<Transform> pathQueue = new Queue<Transform>();
    Camera cameraDefault;
    Transform first, second, third;
    Vector3 cameraDefaultPosition;
    int colorIndex;
    int ballZ;
    enum SpawnType { Single, Multi }


    private void Awake()
    {
        if ( instance != null && instance != this )
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }


    void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClick);
        restartButton.onClick.AddListener(OnRestartButtonClick);
        cameraDefault = Camera.main;
        cameraDefaultPosition = cameraDefault.transform.position;

        if ( !PlayerPrefs.HasKey("coin") )
        {
            PlayerPrefs.SetInt("coin", 0);
        }

        SetCoinText(PlayerPrefs.GetInt("coin"));

        if ( !PlayerPrefs.HasKey("bestScore") )
        {
            PlayerPrefs.SetInt("bestScore", 0);
        }
        SetBestText();

        InitialPath();
    }

    public void InitializeSettings()
    {
        pathQueue.Clear();

        Destroy(first.gameObject);
        Destroy(second.gameObject);
        Destroy(third.gameObject);

        StopAllCoroutines();
        SetText(0);
        cameraDefault.transform.position = cameraDefaultPosition;
        InitialPath();
        Instantiate(player, startPoint, Quaternion.identity);
    }

    void InitialPath()
    {
        ballZ = 0;

        first = Instantiate(path, pathPositionOffset, Quaternion.identity);
        colorIndex = Random.Range(0, colors.Length);
        first.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", GiveColor(colorIndex));
        pathQueue.Enqueue(first);
        ballSpawn(first, 1);

        second = Instantiate(path, first.position + Vector3.forward * 100, Quaternion.identity);
        colorIndex = Random.Range(0, colors.Length);
        second.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", GiveColor(colorIndex));
        pathQueue.Enqueue(second);
        ballSpawn(second, 1);

        third = Instantiate(path, second.position + Vector3.forward * 100, Quaternion.identity);
        colorIndex = Random.Range(0, colors.Length);
        third.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", GiveColor(colorIndex));
        pathQueue.Enqueue(third);
        ballSpawn(third, 1);
    }

    void OnPlayButtonClick()
    {
        Instantiate(player, startPoint, Quaternion.identity);
        playButton.gameObject.SetActive(false);
    }

    void OnRestartButtonClick()
    {
        InitializeSettings();
        restartButton.gameObject.SetActive(false);
    }

    public void OnExitButtonClick()
    {
        Application.Quit();
    }

    public void SetText(int score)
    {
        scoreText.text = score.ToString();
    }

    public void SetBestText()
    {
        bestScoreText.text = PlayerPrefs.GetInt("bestScore").ToString();
    }
    public void SetCoinText(int coin)
    {
        coinText.SetText(coin.ToString());
    }

    public void ChangePath()
    {
        StartCoroutine("NewPath");
    }

    IEnumerator NewPath()
    {
        yield return new WaitForSeconds(0.5f);
        colorIndex = Random.Range(0, colors.Length);

        Transform next = pathQueue.Dequeue();
        Destroy(next.GetChild(1).gameObject);
        next.position = next.position + Vector3.forward * 300;
        next.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", colors[colorIndex]);
        ballSpawn(next, Random.Range(0, 2));
        pathQueue.Enqueue(next);
    }


    public Color GiveColor(int index)
    {
        return colors[index];
    }

    void ballSpawn(Transform parent, int spawnIndex)
    {
        int zIndex = ballZ * 100 + 10;
        int spawnType = spawnIndex;
        Color[] shuffledColors = colors;

        GameObject g = new GameObject("p");
        Transform t;
        int coinOne = Random.Range(0, 7);
        int coinTwo = Random.Range(0, 7);


        if ( spawnType == (int)SpawnType.Single )
        {
            Color singleColor = colors[Random.Range(0, 3)];

            float dir = -1f;
            for ( int i = 0; i < 7; i++ )
            {
                t = Instantiate(ball, new Vector3(dir, 0.5f, (i + 1) * 12 + zIndex), Quaternion.identity);
                t.transform.GetComponent<Renderer>().material.color = singleColor;
                Ball b = t.transform.GetComponent<Ball>();
                b.Point = 4;
                b.Move(dir);
                dir *= -1;
                t.parent = g.transform;
            }
        }
        else if ( spawnType == (int)SpawnType.Multi )
        {
            for ( int i = 0; i < 7; i++ )
            {
                shuffledColors = ShuffleColors(shuffledColors);
                for ( int j = -1; j < 2; j++ )
                {
                    t = Instantiate(ball, new Vector3(j, 0.5f, (i + 1) * 12 + zIndex), Quaternion.identity);
                    t.transform.GetComponent<Renderer>().material.color = shuffledColors[j + 1];
                    t.transform.GetComponent<Ball>().Point = 1;
                    t.parent = g.transform;
                }
            }
        }
        if ( ballZ > 3 )
        {
            Transform c1 = Instantiate(coin, new Vector3(0, 0.5f, (coinOne + 1) * 10 + zIndex - 5f), Quaternion.identity);
            c1.parent = g.transform;
            Transform c2 = Instantiate(coin, new Vector3(0, 0.5f, (coinTwo + 1) * 10 + zIndex - 5f), Quaternion.identity);
            c2.parent = g.transform;
        }
        g.transform.parent = parent;
        ballZ++;
    }

    Color[] ShuffleColors(Color[] arr)
    {
        int len = arr.Length;

        for ( int i = len - 1; i > 0; i-- )
        {
            int r = random.Next(0, i);
            Color c = arr[r];
            arr[r] = arr[i];
            arr[i] = c;
        }
        return arr;
    }
}
