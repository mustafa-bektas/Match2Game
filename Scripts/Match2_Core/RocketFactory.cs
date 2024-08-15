using UnityEngine;

public class RocketFactory
{
    public GameObject[] rocketPrefabs;

    public RocketFactory(GameObject[] rocketPrefabs)
    {
        this.rocketPrefabs = rocketPrefabs;
    }

    public GameObject SpawnRocket(Vector3 position, int rocketTypeIndex)
    {
        GameObject rocketPrefab = rocketPrefabs[rocketTypeIndex];
        GameObject rocket = GameObject.Instantiate(rocketPrefab, position, Quaternion.identity);
        rocket.name = $"Rocket_{position.x}_{position.y}";
        return rocket;
    }
}
