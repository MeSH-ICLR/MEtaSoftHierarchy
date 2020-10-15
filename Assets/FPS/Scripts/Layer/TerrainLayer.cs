using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TerrainLayer : MonoBehaviour
{
    private readonly int maxLevel = 5;
    private readonly int halfSize = 1; // maxSize = halfSize * 2 + 1
    private readonly double rate = 0.2;

    private readonly int height = 8;
    private readonly int length = 45;

    private readonly int[] rampOffsetList = new int[5] { -18, 27, 45, 0, -18 };

    private GameObject rampPrefab = null;
    private GameObject floorPrefab = null;
    private GameObject enemyPrefab = null;

    public NavMeshSurface surface = null;

    private readonly System.Random random = new System.Random();

    void Start()
    {
        rampPrefab = Resources.Load("Prefabs/Ramps/Ramp_8m") as GameObject;
        floorPrefab = Resources.Load("Prefabs/Floors/Floor_9x9") as GameObject;
        enemyPrefab = Resources.Load("Prefabs/Enemies/Enemy_HoverBot") as GameObject;

        int maxSize = halfSize * 2 + 1;
        int[,,] levelInfo = new int[maxLevel, maxSize, maxSize];
        levelInfo[0, halfSize, halfSize] = 1; // start layer position
        CreateFloor(0, halfSize, halfSize);
        for (int level = 0; level < maxLevel - 1; ++level)
        {
            for (int i = 0; i < maxSize; ++i)
            {
                for (int j = 0; j < maxSize; ++j)
                {
                    if (levelInfo[level, i, j] > 0)
                    {
                        List<(int, int)> dirList = new List<(int, int)>();
                        while (dirList.Count == 0)
                        {
                            if (i > 0 && random.NextDouble() < rate)
                            {
                                dirList.Add((-1, 0));
                            }
                            if (i < maxSize - 1 && random.NextDouble() < rate)
                            {
                                dirList.Add((1, 0));
                            }
                            if (j > 0 && random.NextDouble() < rate)
                            {
                                dirList.Add((0, -1));
                            }
                            if (j < maxSize - 1 && random.NextDouble() < rate)
                            {
                                dirList.Add((0, 1));
                            }
                        }
                        foreach ((int, int) dir in dirList)
                        {
                            if (levelInfo[level + 1, i + dir.Item1, j + dir.Item2] == 0)
                            {
                                CreateFloor(level + 1, i + dir.Item1, j + dir.Item2);
                            }
                            CreateRamp(level + 1, i, j, dir.Item1, dir.Item2);
                            levelInfo[level + 1, i + dir.Item1, j + dir.Item2] = 1;
                        }
                    }
                }
            }
        }
        surface.BuildNavMesh();
    }

    void CreateFloor(int level, int i, int j)
    {
        GameObject floor = Instantiate(floorPrefab, transform);
        floor.transform.localPosition = new Vector3(-i * length, level * height, -j * length);
        floor.transform.localEulerAngles = new Vector3(0, 180, 0);

        int count = random.Next(5);
        for (int k = 0; k < count; ++k)
        {
            CreateEnemy(level, i, j);
        }
    }

    void CreateRamp(int level, int i, int j, int di, int dj)
    {
        GameObject ramp = Instantiate(rampPrefab, transform);
        int dir = di == -1 ? 0 : di == 1 ? 2 : dj == 1 ? 1 : 3;
        int x = i * length + rampOffsetList[dir];
        int z = j * length + rampOffsetList[dir + 1];
        ramp.transform.position = new Vector3(-x, level * height, -z);
        ramp.transform.localEulerAngles = new Vector3(0, dir * 90, 0);
    }

    void CreateEnemy(int level, int i, int j)
    {
        GameObject enemy = Instantiate(enemyPrefab, transform);
        int x = i * length + random.Next(5, 21);
        int z = j * length + random.Next(5, 21);
        enemy.transform.position = new Vector3(-x, level * height, -z);
    }
}
