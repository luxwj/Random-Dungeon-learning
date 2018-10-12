using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public int width;
    public int height;

    public string seed;
    public bool use_random_seed;

    [Range(0, 100)]
    public int random_fill_percent;
    [Header("The higher, the smoother")]
    [Range(0, 10)]
    public int map_smoothness;

    int[,] map;

    private void Start() {
        GenerateMap();
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            GenerateMap();
        }
    }

    void GenerateMap() {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < map_smoothness; ++i) {
            SmoothMap();
        }
    }

    void RandomFillMap() {
        if (use_random_seed) {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int i = 0; i < width; ++i) {
            for (int j = 0; j < height; ++j) {
                if (i == 0 || i == width - 1 || j == 0 || j == height - 1) {
                    map[i, j] = 1;
                } else {
                    map[i, j] = (pseudoRandom.Next(0, 100) < random_fill_percent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap() {
        for (int i = 0; i < width; ++i) {
            for (int j = 0; j < height; ++j) {
                int neighborWallTiles = GetSurroundingWallCount(i, j);

                if (neighborWallTiles > 4) {
                    map[i, j] = 1;
                }else if (neighborWallTiles < 4){
                    map[i, j] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount (int gridX, int gridY) {
        int wall_count = 0;
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; ++neighborX) {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; ++neighborY) {
                if (neighborX < 0 || neighborX >= width || neighborY < 0 || neighborY >= height) {
                    ++wall_count;
                } else {
                    if (neighborX == gridX && neighborY == gridY) continue;
                    wall_count += map[neighborX, neighborY];
                }
            }
        }
        return wall_count;
    }

    private void OnDrawGizmos() {
        if (map != null) {
            for (int i = 0; i < width; ++i) {
                for (int j = 0; j < height; ++j) {
                    Gizmos.color = (map[i, j] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector2(-width / 2 + i + .5f, -height / 2 + j + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }

}
