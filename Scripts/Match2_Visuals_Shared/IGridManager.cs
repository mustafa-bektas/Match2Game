using UnityEngine;
public interface IGridManager
{
    Grid Grid { get; }
    GameObject[,] CubeInstances{ get; }
    void InitializeVisualGrid();
    void SetCubeSortingOrder(GameObject cube, int row);
}