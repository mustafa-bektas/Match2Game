using System.Collections.Generic;
using UnityEngine;

public class MatchFinder : IMatchFinder
{
    private Grid grid;

    public MatchFinder(Grid grid)
    {
        this.grid = grid;
    }

    public List<Vector2Int> FindMatches(int x, int y, int cubeType)
    {
        // Flood fill implementation using a stack (DFS)
        var matchedCubes = new List<Vector2Int>();
        var stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(x, y));

        while (stack.Count > 0)
        {
            var pos = stack.Pop();
            if (!matchedCubes.Contains(pos))
            {
                matchedCubes.Add(pos);
                var neighbours = new List<Vector2Int>
                {
                    new(pos.x + 1, pos.y), new(pos.x - 1, pos.y),
                    new(pos.x, pos.y + 1), new(pos.x, pos.y - 1)
                };

                foreach (var neighbour in neighbours)
                {
                    if (IsWithinGrid(neighbour) && (grid.gridArray[neighbour.x, neighbour.y] == cubeType) && !matchedCubes.Contains(neighbour))
                    {
                        stack.Push(neighbour);
                    }
                }
            }
        }

        return matchedCubes;
    }

    private bool IsWithinGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < grid.width && position.y >= 0 && position.y < grid.height;
    }
}