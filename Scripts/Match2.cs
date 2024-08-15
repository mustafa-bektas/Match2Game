using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public class Match2 : MonoBehaviour
{
    [SerializeField]
    private GameObject[] cubePrefabs;
    [SerializeField]
    private GameObject[] rocketPrefabs;
    private IGridManager gridManager;
    private IInputHandler inputHandler;
    private IEventHandler eventHandler;
    private CubeFactory cubeFactory;
    private IClickProcessor clickProcessor;
    private IMatchFinder matchFinder;
    private IMatchHandler matchHandler;
    private RocketFactory rocketFactory;
    private float aspectRatioConstant;
    [SerializeField]
    private AudioClip cubeDestroySound;
    [SerializeField]
    private AudioClip balloonPopSound;
    [SerializeField]
    private AudioClip duckDestroySound;
    [SerializeField]
    private AudioClip cubeCollectSound;
    [SerializeField]
    private ParticleSystem cubeDestroyParticles;
    private IGoalManager goalManager;
    private int goal1CubeType;
    private int goal2CubeType;
    [SerializeField]
    private ParticleSystem rocketSparkleEffect;


    void Start()
    {
        LevelData levelData = LoadLevelData();
        InitializeGameComponents(levelData);
        aspectRatioConstant = ((float)Screen.height / (float)Screen.width) / 1.77f;
        UpdateGoalCubeSprites();
        CenterCameraOnGrid(gridManager.Grid);
        SetGameBorderSize();
    }

    private LevelData LoadLevelData()
    {
        string path = Application.streamingAssetsPath + "/levelData.json";
        string json;

        if (Application.platform == RuntimePlatform.Android)
        {
            // Android does not use file:// for paths, so UnityWebRequest is used to read the file
            UnityWebRequest reader = UnityWebRequest.Get(path);
            reader.SendWebRequest();

            while (!reader.isDone) { }

            json = reader.downloadHandler.text;
        }
        else
        {
            json = File.ReadAllText(path);
        }
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        Debug.Log("Level Data Loaded");
        return levelData;
    }

    private void InitializeGameComponents(LevelData levelData)
    {
        goal1CubeType = levelData.goal1CubeTypeIndex;
        goal2CubeType = levelData.goal2CubeTypeIndex;
        int totalMoves = levelData.totalMoves;
        int goal1Count = levelData.goal1Count;
        int goal2Count = levelData.goal2Count;

        int[,] grid2D = Convert1DArrayTo2D(levelData.grid, levelData.width, levelData.height);

        goalManager = new GoalManager(totalMoves, new GoalCubeAndCount[] { new GoalCubeAndCount(goal1CubeType, goal1Count), new GoalCubeAndCount(goal2CubeType, goal2Count) });
        cubeFactory = new CubeFactory(cubePrefabs);
        rocketFactory = new RocketFactory(rocketPrefabs);
        gridManager = new GridManager(levelData.width, levelData.height, cubeFactory, grid2D);
        matchFinder = new MatchFinder(gridManager.Grid);
        matchHandler = new MatchHandler(gridManager.Grid);
        clickProcessor = new ClickProcessor(matchFinder, matchHandler, gridManager.Grid, goalManager);
        inputHandler = new InputHandler(clickProcessor);
        eventHandler = gameObject.AddComponent<EventHandler>();
        eventHandler.Initialize(matchHandler, gridManager, cubeFactory, rocketFactory, cubeDestroySound, balloonPopSound, duckDestroySound, cubeCollectSound, cubeDestroyParticles, goalManager, rocketSparkleEffect);
    }

    private int[,] Convert1DArrayTo2D(int[] array1D, int width, int height)
    {
        int[,] array2D = new int[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                array2D[i, j] = array1D[i * height + j];
            }
        }
        return array2D;
    }

    void Update()
    {
        if(goalManager.IsGameOver == false)
        {
            inputHandler.HandleInput();
        }
        else
        {
            Debug.Log("Game Over");
        }
        Application.targetFrameRate = 60;
    }

    private void CenterCameraOnGrid(Grid grid)
    {
        Vector3 gridCenter = new Vector3((grid.width - 1) / 2.0f, (grid.height - 1) / 2.0f, -10);
        Camera.main.transform.position = gridCenter;
        Camera.main.orthographicSize = grid.height > grid.width ? grid.height * aspectRatioConstant : grid.width * aspectRatioConstant;
    }

    private void SetGameBorderSize()
    {
        var border = GameObject.Find("GameBorder");
        var rectTransform = border.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(1800, 1800);
    }

    private void UpdateGoalCubeSprites()
    {
        var goal1 = GameObject.Find("Goal1");
        var goal2 = GameObject.Find("Goal2");
        var goal1Image = goal1.GetComponent<UnityEngine.UI.Image>();
        var goal2Image = goal2.GetComponent<UnityEngine.UI.Image>();
        goal1Image.sprite = cubePrefabs[goal1CubeType].GetComponent<SpriteRenderer>().sprite;
        goal2Image.sprite = cubePrefabs[goal2CubeType].GetComponent<SpriteRenderer>().sprite;
    }
}