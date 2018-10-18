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
        List<Room> surviving_rooms = new List<Room>();

        foreach (List<Coord> room_region in room_regions) {
            if (room_region.Count < room_threshold_size) {
                foreach (Coord tile in room_region) {
                    map[tile.tileX, tile.tileY] = 1;
                }
            } else {
                surviving_rooms.Add(new Room(room_region, map));
            }
        }

        ConnectClosestRooms(surviving_rooms);
    }

    void ConnectClosestRooms(List<Room> all_rooms) {

        int best_dist = 0;
        Coord best_tileA = new Coord();
        Coord best_tileB = new Coord();
        Room best_roomA = new Room();
        Room best_roomB = new Room();
        bool possible_connection_found = false;

        foreach (Room roomA in all_rooms) {
            possible_connection_found = false;
            foreach (Room roomB in all_rooms) {

                if (roomA == roomB) continue;
                if (roomA.IsConnected(roomB)) {
                    possible_connection_found = false;
                    break;
                }

                for (int tile_indexA = 0; tile_indexA < roomA.edge_tiles.Count; ++tile_indexA) {
                    for (int tile_indexB = 0; tile_indexB < roomB.edge_tiles.Count; ++tile_indexB) {
                        Coord tileA = roomA.edge_tiles[tile_indexA];
                        Coord tileB = roomB.edge_tiles[tile_indexB];
                        int dist_between_rooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (dist_between_rooms < best_dist || !possible_connection_found) {
                            best_dist = dist_between_rooms;
                            possible_connection_found = true;
                            best_tileA = tileA;
                            best_tileB = tileB;
                            best_roomA = roomA;
                            best_roomB = roomB;
                        }
                    }
                }

                if (possible_connection_found) {
                    CreatePassage(best_roomA, best_roomB, best_tileA, best_tileB);
                }
            }
        }
    }

    void CreatePassage (Room roomA, Room roomB, Coord tileA, Coord tileB) {
        Room.ConnectRooms(roomA, roomB);
        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100f);
    }

    Vector3 CoordToWorldPoint(Coord tile) {
        return new Vector3(-width / 2 + 0.5f + tile.tileX, 2, -height / 2 + 0.5f + tile.tileY);
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

    class Room {
        public List<Coord> tiles;
        public List<Coord> edge_tiles;
        public List<Room> connected_rooms;
        public int room_size;

        public Room() { }
        
        public Room(List<Coord> room_tiles, int[,] map) {
            tiles = room_tiles;
            room_size = tiles.Count;
            connected_rooms = new List<Room>();

            edge_tiles = new List<Coord>();
            foreach (Coord tile in tiles) {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; ++x) {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; ++y) {
                        if (x < map.GetLength(0) && y < map.GetLength(1)) {
                            if (x == tile.tileX || y == tile.tileY) {
                                if (map[x, y] == 1) {
                                    edge_tiles.Add(tile);
                                    break;
                                }
                            }
                            
                        } 
                    }
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB) {
            roomA.connected_rooms.Add(roomB);
            roomB.connected_rooms.Add(roomA);
        }

        public bool IsConnected(Room other_room) {
            return connected_rooms.Contains(other_room);
        }
    }

}
