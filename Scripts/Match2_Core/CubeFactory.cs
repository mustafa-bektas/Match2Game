using UnityEngine;

public class CubeFactory
{
    public GameObject[] cubePrefabs;

    public CubeFactory(GameObject[] cubePrefabs)
    {
        this.cubePrefabs = cubePrefabs;
    }

    public GameObject SpawnCube(Vector3 position, int cubeTypeIndex)
    {
        GameObject cubePrefab = cubePrefabs[cubeTypeIndex];
        GameObject cube = GameObject.Instantiate(cubePrefab, position, Quaternion.identity);
        cube.name = $"Cube_{position.x}_{position.y}";
        return cube;
    }
}
