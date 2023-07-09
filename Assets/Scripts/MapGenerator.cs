using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[Serializable]
public class itemSpawnData
{
    public TileBase tile;
    public int weight;
}

public class MapGenerator : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap itemTilemap;

    public int width = 100;
    public int height = 100;

    public int seed;
    public bool useRandomSeed;

    public float lacunarity = 0.08f; //  振幅衰减系数

    [Range(0, 1f)] public float waterProbability = 0.5f; // 水的比例

    public List<itemSpawnData> itemSpawnDatas;

    public int removeSeparateTileNumberOfTimes = 3; // 移除孤立的地块的次数

    public TileBase groundTile;
    public TileBase waterTile;

    private float[,] mapData; // True: ground, False: water

    public void GenerateMap()
    {
        itemSpawnDatas.Sort((a, b) => a.weight.CompareTo(b.weight));

        GenerateMapData();
        for (int i = 0; i < removeSeparateTileNumberOfTimes; i++)
        {
            if (!removeSeparateTile()) break;
        }

        GenerateTileMap();
    }

    private void GenerateMapData()
    {
        if (!useRandomSeed) seed = Time.time.GetHashCode();

        Random.InitState(seed);
        mapData = new float[width, height];
        float randomOffset = Random.Range(-10000, 10000);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = Mathf.PerlinNoise(x * lacunarity + randomOffset, y * lacunarity + randomOffset);
                mapData[x, y] = noiseValue;

                if (noiseValue < minValue) minValue = noiseValue;
                if (noiseValue > maxValue) maxValue = noiseValue;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mapData[x, y] = Mathf.InverseLerp(minValue, maxValue, mapData[x, y]);
            }
        }
    }

    private bool removeSeparateTile()
    {
        bool hasRemove = false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 如果是地面，且四周只有一个地面，则移除
                if (IsGround(x, y) && GetFourNeighborsGroundCount(x, y) == 1)
                {
                    mapData[x, y] = 0; // 设置成水
                    hasRemove = true;
                }
            }
        }

        return hasRemove;
    }

    private int GetFourNeighborsGroundCount(int x, int y)
    {
        int count = 0;

        // top
        if (IsInMapRange(x, y + 1) && IsGround(x, y + 1)) count++;
        // bottom
        if (IsInMapRange(x, y - 1) && IsGround(x, y - 1)) count++;
        // left
        if (IsInMapRange(x - 1, y) && IsGround(x - 1, y)) count++;
        // right
        if (IsInMapRange(x + 1, y) && IsGround(x + 1, y)) count++;

        return count;
    }

    private int GetEightNeighborsGroundCount(int x, int y)
    {
        int count = 0;

        // top
        if (IsInMapRange(x, y + 1) && IsGround(x, y + 1)) count++;
        // bottom
        if (IsInMapRange(x, y - 1) && IsGround(x, y - 1)) count++;
        // left
        if (IsInMapRange(x - 1, y) && IsGround(x - 1, y)) count++;
        // right
        if (IsInMapRange(x + 1, y) && IsGround(x + 1, y)) count++;

        // top left
        if (IsInMapRange(x - 1, y + 1) && IsGround(x - 1, y + 1)) count++;
        // top right
        if (IsInMapRange(x + 1, y + 1) && IsGround(x + 1, y + 1)) count++;
        // bottom left
        if (IsInMapRange(x - 1, y - 1) && IsGround(x - 1, y - 1)) count++;
        // bottom right
        if (IsInMapRange(x + 1, y - 1) && IsGround(x + 1, y - 1)) count++;

        return count;
    }


    private void GenerateTileMap()
    {
        CleanMap();

        // 地面
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
                groundTilemap.SetTile(new Vector3Int(x, y, 0),
                    IsGround(x, y) ? groundTile : waterTile);
        }

        // 物品
        int weightTotal = itemSpawnDatas.Sum(t => t.weight);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 如果是地面，且地面周围有8个地面，则生成物品
                if (IsGround(x, y) && GetEightNeighborsGroundCount(x, y) == 8)
                {
                    int randomValue = Random.Range(1, weightTotal + 1);
                    int weightSum = 0;
                    foreach (var itemSpawnData in itemSpawnDatas)
                    {
                        weightSum += itemSpawnData.weight;
                        if (randomValue >= weightSum) continue; // 没有命中
                        if (!itemSpawnData.tile) continue; // 没有tile
                        itemTilemap.SetTile(new Vector3Int(x, y, 0), itemSpawnData.tile);
                        break;
                    }
                }
            }
        }
    }

    public bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public bool IsGround(int x, int y)
    {
        return mapData[x, y] > waterProbability;
    }

    public void CleanMap()
    {
        groundTilemap.ClearAllTiles();
        itemTilemap.ClearAllTiles();
    }
}