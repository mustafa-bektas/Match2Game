using UnityEngine;

public class Grid
{
    public int width;
    public int height;
    public int[,] gridArray;

    public Grid(int width, int height)
    {
        this.width = width;
        this.height = height;
        gridArray = new int[width, height]; // 0 means empty, 1-5 represents different cube types, 6 means balloon, 7 means duck, 8 means vertical rocket, 9 means horizontal rocket
        InitializeGrid();
    }

    public Grid(int width, int height, int[,] gridArray)
    {
        this.width = width;
        this.height = height;
        this.gridArray = gridArray;
    }

    private void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridArray[x, y] = Random.Range(1, 8); // Randomly assign cube types
            }
        }
    }
}
