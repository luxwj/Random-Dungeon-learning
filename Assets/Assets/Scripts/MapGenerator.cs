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

        ProcessMap();

        int border_size = 5;
        int[,] bordered_map = new int[width + border_size * 2, height + border_size * 2];

        for (int i = 0; i < bordered_map.GetLength(0); ++i){
            for (int j = 0; j < bordered_map.GetLength(1); ++j){
                if (i >= border_size && i < width + border_size && j >= border_size && j < height + border_size){
                    bordered_map[i, j] = map[i - border_size, j - border_size];
                }else{
                    bordered_map[i, j] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(bordered_map, 1);
    }

    void ProcessMap(){
        List<List<Coord>> wall_regions = GetRegions(1);

        int wall_threshold_size = 50;
        foreach (List<Coord> wall_region in wall_regions){
            if (wall_region.Count < wall_threshold_size){
                foreach (Coord tile in wall_region){
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> room_regions = GetRegions(0);

        int room_threshold_size = 50;
        foreach (List<Coord> room_region in room_regions)
        {
            if (room_region.Count < room_threshold_size)
            {
                foreach (Coord tile in room_region)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
        }
    }

    List<List<Coord>> GetRegions(int tile_type){
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] map_flags = new int[width, height];

        for (int x = 0; x < width; ++x){
            for (int y = 0; y < height; ++y){
                if (map_flags[x, y] == 0 && map[x, y] == tile_type){
                    List<Coord> new_region = GetRegionTiles(x, y);
                    regions.Add(new_region);

                    foreach (Coord tile in new_region){
                        map_flags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles (int startX, int startY){
        List<Coord> tiles = new List<Coord>();
        int[,] map_flags = new int[width, height];
        int tile_type = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        map_flags[startX, startY] = 1;
        while (queue.Count > 0){
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; ++x){
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; ++y){
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX )){
                        if (map_flags[x, y] == 0 && map[x, y] == tile_type){
                            map_flags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x, int y){
        return x >= 0 && x < width && y >= 0 && y < height;
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
                if (IsInMapRange(neighborX, neighborY)){
                    if (neighborX != gridX || neighborY != gridY){
                        wall_count += map[neighborX, neighborY];
                    }
                }
            }
        }
        return wall_count;
    }

    struct Coord {
        public int tileX;
        public int tileY;

        public Coord(int x, int y){
            tileX = x;
            tileY = y;
        }
    }

    //private void OnDrawGizmos() {
    //    if (map != null) {
    //        for (int i = 0; i < width; ++i) {
    //            for (int j = 0; j < height; ++j) {
    //                Gizmos.color = (map[i, j] == 1) ? Color.black : Color.white;
    //                Vector3 pos = new Vector2(-width / 2 + i + .5f, -height / 2 + j + .5f);
    //                Gizmos.DrawCube(pos, Vector3.one);
    //            }
    //        }
    //    }
    //}

}
