using UnityEngine;

public class GridManager : IGridManager
{
    public Grid Grid { get; private set; }
    public GameObject[,] CubeInstances { get; private set; }
    private readonly CubeFactory cubeFactory;

    public GridManager(int width, int height, CubeFactory cubeFactory, int[,] gridArray)
    {
        this.cubeFactory = cubeFactory;
        Grid = new Grid(width, height, gridArray);
        InitializeVisualGrid();
    }

    public void InitializeVisualGrid()
    {
        CubeInstances = new GameObject[Grid.width, Grid.height];
        for (int x = 0; x < Grid.width; x++)
        {
            for (int y = 0; y < Grid.height; y++)
            {
                int cubeTypeIndex = Grid.gridArray[x, y] - 1;
                Vector3 position = new Vector3(x, y, 0);

                GameObject cube = cubeFactory.SpawnCube(position, cubeTypeIndex);
                CubeInstances[x, y] = cube;

                SetCubeSortingOrder(cube, y);
            }
        }
    }

    public void SetCubeSortingOrder(GameObject cube, int row)
    {
        SpriteRenderer spriteRenderer = cube.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = row - Grid.height;
        }
    }
}