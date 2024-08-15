using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    public GameObject[] cubePrefabs;
    private CubeFactory cubeFactory;
    private float aspectRatioConstant;
    [SerializeField]
    private int gridWidth;
    [SerializeField]
    private int gridHeight;
    private Grid grid;
    private int goal1CubeTypeIndex = 3;
    private int goal2CubeTypeIndex = 0;
    private int totalMoves;
    private TMPro.TextMeshProUGUI movesLeftText;
    private TMPro.TextMeshProUGUI goal1Text;
    private TMPro.TextMeshProUGUI goal2Text;
    private int goal1Count;
    private int goal2Count;

    private void Awake()
    {
        cubeFactory = new CubeFactory(cubePrefabs);
    }

    private void Start()
    {
        InitializeUIElements();
        aspectRatioConstant = ((float)Screen.height / (float)Screen.width) / 1.77f;
        CenterCameraOnGrid();
        SetGameBorderSize();
        InitializeGrid();
    }

    private void Update()
    {
        HandleInputForLevelEditor();
        Application.targetFrameRate = 60;
    }

    private void InitializeUIElements()
    {
        movesLeftText = GameObject.Find("MovesLeft").GetComponent<TMPro.TextMeshProUGUI>();
        totalMoves = string.IsNullOrEmpty(movesLeftText.text) ? 0 : int.Parse(movesLeftText.text);
        goal1Text = GameObject.Find("Goal1Left").GetComponent<TMPro.TextMeshProUGUI>();
        goal1Count = string.IsNullOrEmpty(goal1Text.text) ? 0 : int.Parse(goal1Text.text);
        goal2Text = GameObject.Find("Goal2Left").GetComponent<TMPro.TextMeshProUGUI>();
        goal2Count = string.IsNullOrEmpty(goal2Text.text) ? 0 : int.Parse(goal2Text.text);
    }

    private void CenterCameraOnGrid()
    {
        Vector3 gridCenter = new Vector3((gridWidth - 1) / 2.0f, (gridHeight - 1) / 2.0f, -10);
        Camera.main.transform.position = gridCenter;
        Camera.main.orthographicSize = gridHeight > gridWidth ? gridHeight * aspectRatioConstant : gridWidth * aspectRatioConstant;
    }

    private void SetGameBorderSize()
    {
        var border = GameObject.Find("GameBorder");
        var rectTransform = border.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(1800, 1800);
    }

    private void InitializeGrid()
    {
        grid = new Grid(gridWidth, gridHeight);
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid.gridArray[x, y] = 1;
                int cubeTypeIndex = grid.gridArray[x, y] - 1;
                Vector3 position = new Vector3(x, y, 0);
                GameObject cube = cubeFactory.SpawnCube(position, cubeTypeIndex);
                SetCubeSortingOrder(cube, y);
            }
        }
    }

    private void SetCubeSortingOrder(GameObject cube, int row)
    {
        SpriteRenderer spriteRenderer = cube.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = row - gridHeight; // Higher rows have lower sorting order
        }
    }

    public void HandleInputForLevelEditor()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 touchPos = new Vector2(worldPoint.x, worldPoint.y);
            RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject touchedObject = hit.collider.gameObject;
                
                var actions = new Dictionary<string, Action>
                {
                    { "Goal1", () => UpdateGoalCubeSprites(touchedObject) },
                    { "Goal2", () => UpdateGoalCubeSprites(touchedObject) },
                    { "IncreaseTotalMoves", () => IncreaseTotalMoves() },
                    { "DecreaseTotalMoves", () => DecreaseTotalMoves() },
                    { "IncreaseGoal1", () => IncreaseGoal1() },
                    { "DecreaseGoal1", () => DecreaseGoal1() },
                    { "IncreaseGoal2", () => IncreaseGoal2() },
                    { "DecreaseGoal2", () => DecreaseGoal2() },
                };

                if (actions.ContainsKey(touchedObject.name))
                {
                    actions[touchedObject.name]();
                    return;
                }

                string[] parts = touchedObject.name.Split('_');
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);

                ProcessClickForLevelEditor(x, y);
            }
        }
    }

    private void IncreaseTotalMoves()
    {
        totalMoves++;
        movesLeftText.text = totalMoves.ToString();
    }

    private void DecreaseTotalMoves()
    {
        totalMoves = Math.Max(0, totalMoves - 1);
        movesLeftText.text = totalMoves.ToString();
    }

    private void IncreaseGoal1()
    {
        goal1Count++;
        goal1Text.text = goal1Count.ToString();
    }

    private void DecreaseGoal1()
    {
        goal1Count = Math.Max(0, goal1Count - 1);
        goal1Text.text = goal1Count.ToString();
    }

    private void IncreaseGoal2()
    {
        goal2Count++;
        goal2Text.text = goal2Count.ToString();
    }

    private void DecreaseGoal2()
    {
        goal2Count = Math.Max(0, goal2Count - 1);
        goal2Text.text = goal2Count.ToString();
    }

    private void ProcessClickForLevelEditor(int x, int y)
    {
        GameObject cube = GetCubeAtPosition(x, y);
        int cubeTypeIndex = GetNextCubeType(x, y);
        Vector3 position = new Vector3(x, y, 0);
        Destroy(cube);
        GameObject newCube = cubeFactory.SpawnCube(position, cubeTypeIndex);
        grid.gridArray[x, y] = cubeTypeIndex + 1;
        SetCubeSortingOrder(newCube, y);
    }

    private GameObject GetCubeAtPosition(int x, int y)
    {
        GameObject cube = GameObject.Find($"Cube_{x}_{y}");
        return cube;
    }

    private int GetNextCubeType(int x, int y)
    {
        int currentCubeTypeIndex = grid.gridArray[x, y] - 1;
        int nextCubeTypeIndex = (currentCubeTypeIndex + 1) % cubePrefabs.Length;
        return nextCubeTypeIndex;
    }

    private void UpdateGoalCubeSprites(GameObject touchedObject)
    {
        if (touchedObject.name == "Goal1" || touchedObject.name == "Goal2")
        {
            int cubeTypeIndex = touchedObject.name == "Goal1" ? goal1CubeTypeIndex : goal2CubeTypeIndex;
            int originalCubeTypeIndex = cubeTypeIndex;
            do
            {
                cubeTypeIndex = (cubeTypeIndex + 1) % cubePrefabs.Length;
            } while ((cubeTypeIndex == goal1CubeTypeIndex || cubeTypeIndex == goal2CubeTypeIndex) && cubeTypeIndex != originalCubeTypeIndex);

            if (cubeTypeIndex == originalCubeTypeIndex)
            {
                return;
            }

            var touchedObjectImage = touchedObject.GetComponent<UnityEngine.UI.Image>();
            touchedObjectImage.sprite = cubePrefabs[cubeTypeIndex].GetComponent<SpriteRenderer>().sprite;

            if (touchedObject.name == "Goal1")
            {
                goal1CubeTypeIndex = cubeTypeIndex;
                Debug.Log("goal1CubeTypeIndex: " + goal1CubeTypeIndex);
            }
            else
            {
                goal2CubeTypeIndex = cubeTypeIndex;
                Debug.Log("goal2CubeTypeIndex: " + goal2CubeTypeIndex);
            }
        }
    }

    public void SaveLevel()
    {
        Debug.Log("Saving level");

        // Convert 2D array to 1D array
        int[] grid1D = new int[grid.gridArray.GetLength(0) * grid.gridArray.GetLength(1)];
        for (int i = 0; i < grid.gridArray.GetLength(0); i++)
        {
            for (int j = 0; j < grid.gridArray.GetLength(1); j++)
            {
                grid1D[i * grid.gridArray.GetLength(1) + j] = grid.gridArray[i, j];
            }
        }

        LevelData levelData = new LevelData
        {
            grid = grid1D,
            width = grid.gridArray.GetLength(0),
            height = grid.gridArray.GetLength(1),
            goal1CubeTypeIndex = goal1CubeTypeIndex,
            goal2CubeTypeIndex = goal2CubeTypeIndex,
            totalMoves = totalMoves,
            goal1Count = goal1Count,
            goal2Count = goal2Count
        };

        string json = JsonUtility.ToJson(levelData);
        System.IO.File.WriteAllText(Application.streamingAssetsPath + "/levelData.json", json);
    }
}

[Serializable]
public class LevelData
{
    public int[] grid;
    public int width;
    public int height;
    public int goal1CubeTypeIndex;
    public int goal2CubeTypeIndex;
    public int totalMoves;
    public int goal1Count;
    public int goal2Count;
}