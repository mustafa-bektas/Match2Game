using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class EventHandler : MonoBehaviour, IEventHandler
{
    public AudioSource audioSource;
    private AudioClip cubeDestroySound;
    private AudioClip balloonPopSound;
    private AudioClip duckDestroySound;
    private AudioClip cubeCollectSound;
    public bool AreAnyCubesMoving { get; private set; }
    private IMatchHandler matchHandler;
    private IGridManager gridManager;
    private CubeFactory cubeFactory;
    private RocketFactory rocketFactory;
    private ParticleSystem cubeDestroyParticles;
    private IGoalManager goalManager;
    private TMPro.TextMeshProUGUI movesLeftText;
    private TMPro.TextMeshProUGUI goal1Text;
    private TMPro.TextMeshProUGUI goal2Text;
    private Vector3 goal1Position;
    private Vector3 goal2Position;
    private ParticleSystem rocketSparkleEffect;


    public void Initialize(
        IMatchHandler matchHandler, 
        IGridManager gridManager, 
        CubeFactory cubeFactory, 
        RocketFactory rocketFactory, 
        AudioClip cubeDestroySound, 
        AudioClip balloonPopSound, 
        AudioClip duckDestroySound, 
        AudioClip cubeCollectSound,
        ParticleSystem cubeDestroyParticles, 
        IGoalManager goalManager, 
        ParticleSystem rocketSparkleEffect
    )
    {
        this.matchHandler = matchHandler;
        this.gridManager = gridManager;
        this.cubeFactory = cubeFactory;
        this.rocketFactory = rocketFactory;
        this.audioSource = gameObject.AddComponent<AudioSource>();
        this.cubeDestroySound = cubeDestroySound;
        this.balloonPopSound = balloonPopSound;
        this.duckDestroySound = duckDestroySound;
        this.cubeCollectSound = cubeCollectSound;
        this.cubeDestroyParticles = cubeDestroyParticles;
        this.goalManager = goalManager;
        this.rocketSparkleEffect = rocketSparkleEffect;
        movesLeftText = GameObject.Find("MovesLeft").GetComponent<TMPro.TextMeshProUGUI>();
        goal1Text = GameObject.Find("Goal1Left").GetComponent<TMPro.TextMeshProUGUI>();
        goal2Text = GameObject.Find("Goal2Left").GetComponent<TMPro.TextMeshProUGUI>();
        matchHandler.OnCubeRemoved += HandleCubeRemoval;
        matchHandler.OnSpawningNewCubes += HandleSpawningNewCubes;
        matchHandler.OnCubeMoved += HandleCubeMovement;
        matchHandler.OnSpawningNewRocket += HandleSpawningNewRocket;
        UpdateMovesLeftAndGoalTexts();
    }

    public IEnumerator MoveCube(GameObject cube, Vector3 newPosition)
    {
        Vector3 startPosition = cube.transform.position;
        cube.transform.name = $"Cube_{(int)startPosition.x}_{(int)newPosition.y}";
        gridManager.SetCubeSortingOrder(cube, (int)newPosition.y);
        AreAnyCubesMoving = true;
        float timeToMove = 0.1f;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            cube.transform.position = Vector3.Lerp(startPosition, newPosition, t);
            yield return null;
        }
        AreAnyCubesMoving = false;

        RemoveDucksInLowestRow();
    }

    public IEnumerator SpawnAndFallNewCubes(GameObject[] cubePrefabs)
    {
        List<GameObject> newCubes = new List<GameObject>();
        List<Vector3> finalPositions = new List<Vector3>();

        for (int x = 0; x < gridManager.Grid.width; x++)
        {
            int startY = FindFirstEmptyY(x);
            if (startY < gridManager.Grid.height)
            {
                for (int y = startY; y < gridManager.Grid.height; y++)
                {
                    int cubeTypeIndex = UnityEngine.Random.Range(0, cubePrefabs.Length-1);
                    Vector3 position = new Vector3(x, gridManager.Grid.height, 0);

                    GameObject newCube = cubeFactory.SpawnCube(position, cubeTypeIndex);
                    gridManager.CubeInstances[x, y] = newCube;

                    gridManager.Grid.gridArray[x, y] = cubeTypeIndex + 1;

                    newCubes.Add(newCube);
                    finalPositions.Add(new Vector3(x, y, 0));
                }
            }
        }

        yield return null;

        for (int i = 0; i < newCubes.Count; i++)
        {
            StartCoroutine(MoveCube(newCubes[i], finalPositions[i]));
        }
    }

    private int FindFirstEmptyY(int x)
    {
        for (int y = 0; y < gridManager.Grid.height; y++)
        {
            if (gridManager.CubeInstances[x, y] == null)
                return y;
        }
        return gridManager.Grid.height;
    }

    public void HandleCubeRemoval(int x, int y)
    {
        GameObject cubeToDestroy = gridManager.CubeInstances[x, y];
        bool destroyAfterAnimation = false;

        if (cubeToDestroy != null)
        {
            int cubeType = gridManager.Grid.gridArray[x, y];
            if (cubeType >= 1 && cubeType <= 5)
            {
                audioSource.PlayOneShot(cubeDestroySound,1);
                ParticleSystem effect = Instantiate(cubeDestroyParticles, cubeToDestroy.transform.position, Quaternion.Euler(-90, 0, 0));
                SetParticleColorBasedOnCubeType(effect, cubeType);
                effect.Play();
                var main = effect.main;
                main.stopAction = ParticleSystemStopAction.Destroy;

                if ((cubeType == goalManager.GoalCubeAndCounts[0].cubeType+1 && goalManager.GoalCubeAndCounts[0].count > 0) || (cubeType == goalManager.GoalCubeAndCounts[1].cubeType+1 && goalManager.GoalCubeAndCounts[1].count > 0))
                {
                    destroyAfterAnimation = true;
                    StartCoroutine(MoveCubeTowardsGoal(cubeToDestroy, cubeType));
                }
            }
            else
            {
                switch (cubeType)
                {
                    case 6: // Balloon
                        audioSource.PlayOneShot(balloonPopSound, 0.25f);
                        break;
                    case 7: // Duck
                        audioSource.PlayOneShot(duckDestroySound, 0.25f);
                        break;
                    case 8: // Vertical rocket
                        destroyAfterAnimation = true;
                        StartCoroutine(DestroyVerticalRocket(cubeToDestroy));
                        break;
                    case 9: // Horizontal rocket
                        destroyAfterAnimation = true;
                        StartCoroutine(DestroyHorizontalRocket(cubeToDestroy));
                        break;
                    default:
                        break;
                }
            }
            
            goalManager.UpdateGoal(cubeType-1);
            UpdateMovesLeftAndGoalTexts();
            gridManager.Grid.gridArray[x, y] = 0;
            gridManager.CubeInstances[x, y] = null;

            if(!destroyAfterAnimation)
            {
                Destroy(cubeToDestroy);
            }
        }
    }

    public void HandleCubeMovement(int fromX, int fromY, int toY)
    {
        GameObject cubeToMove = gridManager.CubeInstances[fromX, fromY];
        if (cubeToMove != null)
        {
            gridManager.CubeInstances[fromX, toY] = cubeToMove;
            gridManager.CubeInstances[fromX, fromY] = null;
            StartCoroutine(MoveCube(cubeToMove, new Vector3(fromX, toY, 0)));
        }
    }

    public void HandleSpawningNewCubes()
    {
        StartCoroutine(WaitForCubesToSettleThenSpawn());
    }

    private IEnumerator WaitForCubesToSettleThenSpawn()
    {
        // Wait while any cubes are still moving
        yield return new WaitUntil(() => !AreAnyCubesMoving);
        StartCoroutine(SpawnAndFallNewCubes(cubeFactory.cubePrefabs));
    }
    
    public void HandleSpawningNewRocket(int x, int y, bool isVertical)
    {
        var rocketTypeIndex = isVertical ? 0 : 1;
        GameObject rocket = rocketFactory.SpawnRocket(new Vector3(x, y, 0), rocketTypeIndex);
        gridManager.CubeInstances[x, y] = rocket;
    }

    private void RemoveDucksInLowestRow()
    {
        for (int x = 0; x < gridManager.Grid.width; x++)
        {
            if (gridManager.Grid.gridArray[x, 0] == 7) // 7 is the duck
            {
                matchHandler.RemoveMatches(new List<Vector2Int> { new Vector2Int(x, 0) }, false);
            }
        }
    }

    private void SetParticleColorBasedOnCubeType(ParticleSystem effect, int cubeType)
    {
        var main = effect.main;
        switch (cubeType)
        {
            case 1:
                main.startColor = (Color)new Color32(41, 142, 220, 255);
                break;
            case 2:
                main.startColor = Color.green;
                break;
            case 3:
                main.startColor = Color.magenta;
                break;
            case 4:
                main.startColor = Color.red;
                break;
            case 5:
                main.startColor = Color.yellow;
                break;
            default:
                break;
        }
    }

    private void UpdateMovesLeftAndGoalTexts()
    {
        movesLeftText.text = goalManager.MovesLeft.ToString();
        goal1Text.text = goalManager.GoalCubeAndCounts[0].count.ToString();
        goal2Text.text = goalManager.GoalCubeAndCounts[1].count.ToString();

    }

    private IEnumerator MoveCubeTowardsGoal(GameObject cube, int cubeType)
    {
        goal1Position = GameObject.Find("Goal1").GetComponent<RectTransform>().position;
        goal2Position = GameObject.Find("Goal2").GetComponent<RectTransform>().position;

        Vector3 goalPosition = (cubeType == goalManager.GoalCubeAndCounts[0].cubeType+1) ? goal1Position : goal2Position;
        while (cube.transform.position != goalPosition)
        {
            cube.transform.position = Vector3.MoveTowards(cube.transform.position, goalPosition, Time.deltaTime * 15);
            yield return null;
        }

        audioSource.PlayOneShot(cubeCollectSound, 1);
        Destroy(cube);
    }

    private IEnumerator DestroyVerticalRocket(GameObject rocket)
    {
        Transform topPart = rocket.transform.Find("rocket_up");
        Transform bottomPart = rocket.transform.Find("rocket_down");

        Vector3 topDirection = new Vector3(0, 1, 0);
        Vector3 bottomDirection = new Vector3(0, -1, 0);

        ParticleSystem topSparkleEffect = Instantiate(rocketSparkleEffect, topPart.position, Quaternion.identity);
        ParticleSystem bottomSparkleEffect = Instantiate(rocketSparkleEffect, bottomPart.position, Quaternion.identity);

        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            topPart.position += topDirection * Time.deltaTime * 15;
            bottomPart.position += bottomDirection * Time.deltaTime * 15;

            topSparkleEffect.transform.position = topPart.position;
            bottomSparkleEffect.transform.position = bottomPart.position;

            yield return null;
        }

        Destroy(rocket);
        Destroy(topSparkleEffect.gameObject);
        Destroy(bottomSparkleEffect.gameObject);
    }

    private IEnumerator DestroyHorizontalRocket(GameObject rocket)
    {
        Transform leftPart = rocket.transform.Find("rocket_left");
        Transform rightPart = rocket.transform.Find("rocket_right");

        Vector3 leftDirection = new Vector3(-1, 0, 0);
        Vector3 rightDirection = new Vector3(1, 0, 0);

        ParticleSystem leftSparkleEffect = Instantiate(rocketSparkleEffect, leftPart.position, Quaternion.identity);
        ParticleSystem rightSparkleEffect = Instantiate(rocketSparkleEffect, rightPart.position, Quaternion.identity);

        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            leftPart.position += leftDirection * Time.deltaTime * 15;
            rightPart.position += rightDirection * Time.deltaTime * 15;

            leftSparkleEffect.transform.position = leftPart.position;
            rightSparkleEffect.transform.position = rightPart.position;

            yield return null;
        }

        Destroy(rocket);
        Destroy(leftSparkleEffect.gameObject);
        Destroy(rightSparkleEffect.gameObject);
    }
}