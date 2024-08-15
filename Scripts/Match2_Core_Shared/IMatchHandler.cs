using System.Collections.Generic;
using System;
using UnityEngine;
public interface IMatchHandler
{
    event Action<int, int> OnCubeRemoved;
    event Action<int, int, int> OnCubeMoved;
    event Action OnSpawningNewCubes;
    event Action<int, int, bool> OnSpawningNewRocket;
    void RemoveMatches(List<Vector2Int> matchedCubes, bool isSourceRocket = false);
}