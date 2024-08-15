using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchHandler : IMatchHandler
{
    private Grid grid;
    public event Action<int, int> OnCubeRemoved;
    public event Action<int, int, int> OnCubeMoved;
    public event Action OnSpawningNewCubes;
    public event Action<int, int, bool> OnSpawningNewRocket;

    public MatchHandler(Grid grid)
    {
        this.grid = grid;
    }

    public void RemoveMatches(List<Vector2Int> matches, bool isSourceRocket = false)
    {
        var normalCubeCountInMatch = 0;

        foreach (var match in matches)
        {
            if (grid.gridArray[match.x, match.y] >= 1 && grid.gridArray[match.x, match.y] <= 5)
            {
                normalCubeCountInMatch++;
            }
            OnCubeRemoved?.Invoke(match.x, match.y);
        }

        if (normalCubeCountInMatch >= 5 && !isSourceRocket)
        {
            Vector2Int rocketPosition = matches[0];
            System.Random random = new System.Random();

            // Spawn either vertical or horizontal rocket with %50 chance
            if (random.Next(2) == 0)
            {
                grid.gridArray[rocketPosition.x, rocketPosition.y] = 8; // Vertical rocket
                OnSpawningNewRocket?.Invoke(rocketPosition.x, rocketPosition.y, true);
            }
            else
            {
                grid.gridArray[rocketPosition.x, rocketPosition.y] = 9; // Horizontal rocket
                OnSpawningNewRocket?.Invoke(rocketPosition.x, rocketPosition.y, false);
            }
        }

        HandleFallingCubes();
    }

    private void HandleFallingCubes()
    {
        bool needToSpawnNewCubes = false;
        for (int x = 0; x < grid.width; x++)
        {
            int emptySpot = -1;
            for (int y = 0; y < grid.height; y++)
            {
                if (grid.gridArray[x, y] == 0 && emptySpot == -1)
                {
                    emptySpot = y;
                }
                else if (grid.gridArray[x, y] != 0 && emptySpot != -1)
                {
                    grid.gridArray[x, emptySpot] = grid.gridArray[x, y];
                    grid.gridArray[x, y] = 0;
                    OnCubeMoved?.Invoke(x, y, emptySpot);
                    emptySpot++;
                }
                needToSpawnNewCubes = true;
            }
        }
        if (needToSpawnNewCubes)
        {
            OnSpawningNewCubes?.Invoke();
        }
    }
}