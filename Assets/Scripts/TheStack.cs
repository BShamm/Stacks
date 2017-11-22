using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TheStack : MonoBehaviour
{
    public Text scoreText;
    public Color32[] gameColors = new Color32[4];
    public Material stackMat;
    public GameObject endPanel;

    private const float boundsSize = 3.5f;
    private const float stackMovingSpeed = 5.0f;
    private const float errorMargin = 0.1f;
    private const float stackMulti = 0.25f;
    private const int comboStartMulti = 3;

    private int scoreCount = 0;
    private int stackIndex;
    private int combo = 0;

    private float tileTransition = 0.0f;
    private float tileSpeed = 2f;
    private float secondaryPosition;

    private bool isMovingOnX = true;
    private bool GameOver = false;

    private Vector2 stackBound = new Vector2(boundsSize, boundsSize);

    private Vector3 desiredPosition;
    private Vector3 lastTilePostion;

    private GameObject[] theStack;

	void Start ()
    {
        theStack = new GameObject[transform.childCount];
        for(int i =0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i).gameObject;
            ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
        }

        stackIndex = transform.childCount - 1;

	}

	void Update ()
    {
        if(GameOver)
        {
            return;
        }

		if(Input.GetMouseButtonDown(0))
        {
            if(PlaceTile())
            {
                SpawnTile();
                scoreCount++;
                scoreText.text = scoreCount.ToString();
            }
            else
            {
                EndGame();
            }
        }

        MoveTile();

        //Move Stack
        transform.position = Vector3.Lerp(transform.position, desiredPosition, stackMovingSpeed * Time.deltaTime);

	}

    private void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();
        go.GetComponent<MeshRenderer>().material = stackMat;

        ColorMesh(go.GetComponent<MeshFilter>().mesh);


    }

    private void MoveTile()
    {

        tileTransition += Time.deltaTime * tileSpeed;
        if(isMovingOnX)
        {
            theStack[stackIndex].transform.localPosition = new Vector3(Mathf.Sin (tileTransition) * boundsSize, scoreCount, secondaryPosition);
        }
        else
        {
            theStack[stackIndex].transform.localPosition = new Vector3(secondaryPosition, scoreCount, Mathf.Sin(tileTransition) * boundsSize);

        }
    }

    private void SpawnTile()
    {
        lastTilePostion = theStack[stackIndex].transform.localPosition;
        stackIndex--;
        if(stackIndex < 0)
        {
            stackIndex = transform.childCount - 1; 
        }

        desiredPosition = (Vector3.down) * scoreCount;
        theStack[stackIndex].transform.localPosition = new Vector3(0, scoreCount, 0);
        theStack[stackIndex].transform.localScale = new Vector3(stackBound.x, 1, stackBound.y);

        ColorMesh(theStack[stackIndex].GetComponent<MeshFilter> ().mesh); 

    }

    private bool PlaceTile()
    {
        Transform t = theStack[stackIndex].transform;

        if(isMovingOnX)
        {
            float deltaX = lastTilePostion.x - t.position.x;
            if(Mathf.Abs(deltaX) > errorMargin)
            {
                // Cut the Tile
                combo = 0;
                stackBound.x -= Mathf.Abs(deltaX);
                if (stackBound.x <= 0)
                {
                    return false;
                }

                float middle = lastTilePostion.x + t.localPosition.x / 2;
                t.localScale = new Vector3(stackBound.x, 1, stackBound.y);
                CreateRubble
                (
                    new Vector3((t.position.x > 0)
                        ? t.position.x + (t.localScale.x / 2)
                        : t.position.x - (t.localScale.x / 2)
                        , t.position.y
                        ,t.position.z), 
                    new Vector3(Mathf.Abs(deltaX), 1 ,t.localScale.z)
                 );
                t.localPosition = new Vector3(middle - (lastTilePostion.x / 2), scoreCount, lastTilePostion.z);
            }
            else
            {
                if(combo > comboStartMulti)
                {
                    stackBound.x += stackMulti;
                    if(stackBound.x > boundsSize)
                    {
                        stackBound.x = boundsSize;
                    }

                    float middle = lastTilePostion.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(stackBound.x, 1, stackBound.y);
                    t.localPosition = new Vector3(middle - (lastTilePostion.x / 2), scoreCount, lastTilePostion.z);
                }
                combo++;
                t.localPosition = new Vector3(lastTilePostion.x, scoreCount, lastTilePostion.z);

            }
        }
        else
        {
            float deltaZ = lastTilePostion.z - t.position.z;
            if (Mathf.Abs(deltaZ) > errorMargin)
            {
                // Cut the Tile
                combo = 0;
                stackBound.y -= Mathf.Abs(deltaZ);
                if (stackBound.y <= 0)
                {
                    return false;
                }

                float middle = lastTilePostion.z + t.localPosition.z / 2;
                t.localScale = new Vector3(stackBound.x, 1, stackBound.y);
                CreateRubble
                (
                    new Vector3(t.position.x
                        , t.position.y
                        , (t.position.z > 0)
                        ? t.position.z + (t.localScale.z / 2)
                        : t.position.z - (t.localScale.z / 2)),
                    new Vector3(t.localScale.x, 1, Mathf.Abs(deltaZ))
                );
                t.localPosition = new Vector3(lastTilePostion.x, scoreCount, middle - (lastTilePostion.z / 2));
            }
            else
            {
                if (combo > comboStartMulti)
                {
                    stackBound.y+= stackMulti;
                    if (stackBound.y > boundsSize)
                    {              
                        stackBound.y = boundsSize;
                    }
                    float middle = lastTilePostion.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(stackBound.x, 1, stackBound.y);
                    t.localPosition = new Vector3(lastTilePostion.x, scoreCount, middle - (lastTilePostion.z / 2));
                }

                combo++;
                t.localPosition = new Vector3(lastTilePostion.x, scoreCount, lastTilePostion.z);

            }
        }

        secondaryPosition = (isMovingOnX) ? t.localPosition.x : t.localPosition.z;
        isMovingOnX = !isMovingOnX;
        

        return true;
    }

    private void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        float f = Mathf.Sin(scoreCount * 0.25f);

        for(int i = 0; i < vertices.Length; i++)
        {
            colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);
        }

        mesh.colors32 = colors;
    }

    private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < 0.33f)
        {
            return Color.Lerp(a, b, t / 0.33f);
        }
        else if (t < 0.66f)
        {
            return Color.Lerp(b, c, (t - 0.33f) / 0.33f);
        }
        else
            return Color.Lerp(c, d, (t - 0.66f) / 0.66f);
    }

    private void EndGame()
    {
        if(PlayerPrefs.GetInt("score") < scoreCount)
        {
            PlayerPrefs.SetInt("score", scoreCount);
        }
        GameOver = true;
        endPanel.SetActive(true);
        theStack[stackIndex].AddComponent<Rigidbody>();
    }

    public void OnButtonClick(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
