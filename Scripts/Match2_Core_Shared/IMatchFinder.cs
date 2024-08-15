using System.Collections.Generic;
using UnityEngine;

public interface IMatchFinder
{
    List<Vector2Int> FindMatches(int x, int y, int cubeType);
}