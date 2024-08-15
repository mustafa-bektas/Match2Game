using UnityEngine;
using System.Collections;
public interface IEventHandler
{
    void Initialize(
        IMatchHandler matchHandler, 
        IGridManager gridManager, 
        CubeFactory cubeFactory, 
        RocketFactory rocketFactory, 
        AudioClip cubeDestroySound, 
        AudioClip balloonPopSound, 
        AudioClip duckDestroySound, 
        AudioClip cubeCollectSound, 
        ParticleSystem cubeDestroyParticles, 
        IGoalManager goalManager, 
        ParticleSystem rocketSparkleEffect
    );
    bool AreAnyCubesMoving { get; }
    IEnumerator MoveCube(GameObject cube, Vector3 newPosition);
    IEnumerator SpawnAndFallNewCubes(GameObject[] cubePrefabs);
    void HandleCubeRemoval(int x, int y);
    void HandleCubeMovement(int fromX, int fromY, int toY);
    void HandleSpawningNewCubes();
    void HandleSpawningNewRocket(int x, int y, bool isVertical);
}

