using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ClickProcessor : IClickProcessor
{
    private readonly IMatchFinder matchFinder;
    private readonly IMatchHandler matchHandler;
    private readonly Grid grid;
    private IGoalManager goalManager;
    private HashSet<Vector2Int> processedCubes = new HashSet<Vector2Int>();

    public ClickProcessor(IMatchFinder matchFinder, IMatchHandler matchHandler, Grid grid, IGoalManager goalManager)
    {
        this.matchFinder = matchFinder;
        this.matchHandler = matchHandler;
        this.grid = grid;
        this.goalManager = goalManager;
    }
    
    public void ProcessClick(int x, int y)
    {
        ResetProcessedCubes();
        var matchedCubes = new List<Vector2Int>();
        bool isSourceRocket = ProcessCube(x, y, matchedCubes);
        RemoveMatchesIfValid(matchedCubes, isSourceRocket);
    }

    private void ResetProcessedCubes()
    {
        processedCubes.Clear();
    }

    private void RemoveMatchesIfValid(List<Vector2Int> matchedCubes, bool isSourceRocket)
    {
        matchedCubes = matchedCubes.Distinct().ToList();
        if (matchedCubes.Count >= 2)
        {
            goalManager.UpdateMovesLeft();
            matchHandler.RemoveMatches(matchedCubes, isSourceRocket);
        }
    }

    private bool ProcessCube(int x, int y, List<Vector2Int> matchedCubes, bool sourceIsRocket = false)
    {
        Vector2Int currentCube = new Vector2Int(x, y);

        if (IsCubeProcessed(currentCube))
        {
            return false;
        }

        MarkCubeAsProcessed(currentCube);

        if (IsCubeEmpty(x, y))
            return false;

        int cubeType = grid.gridArray[x, y];
        bool isRocket = IsRocket(cubeType);
        sourceIsRocket = sourceIsRocket || isRocket;

        if (IsNormalCube(cubeType) && !sourceIsRocket)
        {
            matchedCubes.AddRange(matchFinder.FindMatches(x, y, cubeType));
            
            AddAdjacentBalloonsToMatchedCubes(matchedCubes);
        }
        else if (IsVerticalRocket(cubeType))
        {
            ProcessVerticalRocket(x, matchedCubes, sourceIsRocket);
        }
        else if (IsHorizontalRocket(cubeType))
        {
            ProcessHorizontalRocket(y, matchedCubes, sourceIsRocket);
        }

        return isRocket;
    }

    private bool IsCubeProcessed(Vector2Int cube)
    {
        return processedCubes.Contains(cube);
    }

    private void MarkCubeAsProcessed(Vector2Int cube)
    {
        processedCubes.Add(cube);
    }

    private bool IsCubeEmpty(int x, int y)
    {
        return grid.gridArray[x, y] == 0;
    }

    private bool IsRocket(int cubeType)
    {
        return cubeType == 8 || cubeType == 9;
    }

    private bool IsNormalCube(int cubeType)
    {
        return cubeType >= 1 && cubeType <= 5;
    }

    private bool IsVerticalRocket(int cubeType)
    {
        return cubeType == 8;
    }

    private bool IsHorizontalRocket(int cubeType)
    {
        return cubeType == 9;
    }

    private void ProcessVerticalRocket(int x, List<Vector2Int> matchedCubes, bool sourceIsRocket)
    {
        for (int i = 0; i < grid.height; i++)
        {
            matchedCubes.Add(new Vector2Int(x, i));
            ProcessCube(x, i, matchedCubes, sourceIsRocket);
        }
    }

    private void ProcessHorizontalRocket(int y, List<Vector2Int> matchedCubes, bool sourceIsRocket)
    {
        for (int i = 0; i < grid.width; i++)
        {
            matchedCubes.Add(new Vector2Int(i, y));
            ProcessCube(i, y, matchedCubes, sourceIsRocket);
        }
    }

    private void AddAdjacentBalloonsToMatchedCubes(List<Vector2Int> matchedCubes)
    {
        if (matchedCubes.Count < 2)
        {
            return;
        }

        List<Vector2Int> balloonsToAdd = new List<Vector2Int>();

        foreach (var match in matchedCubes)
        {
            // Check the four adjacent cubes
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                new Vector2Int(match.x - 1, match.y),
                new Vector2Int(match.x + 1, match.y),
                new Vector2Int(match.x, match.y - 1),
                new Vector2Int(match.x, match.y + 1)
            };

            foreach (Vector2Int position in adjacentPositions)
            {
                if (IsWithinGrid(position) && IsBalloon(grid.gridArray[position.x, position.y]))
                {
                    balloonsToAdd.Add(position);
                }
            }
        }

        matchedCubes.AddRange(balloonsToAdd);
    }

    private bool IsWithinGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < grid.width && position.y >= 0 && position.y < grid.height;
    }

    private bool IsBalloon(int cubeType)
    {
        return cubeType == 6;
    }
}