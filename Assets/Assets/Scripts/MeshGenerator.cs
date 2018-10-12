using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    public SquareGrid square_grid;
    List<Vector3> vertices;
    List<int> triangles;
    
    public void GenerateMesh(int[,] map, float square_size) {
        square_grid = new SquareGrid(map, square_size);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        int width = square_grid.squares.GetLength(0);
        int height = square_grid.squares.GetLength(1);
        for (int i = 0; i < width; ++i) {
            for (int j = 0; j < height; ++j) {
                TriangulateSquare(square_grid.squares[i, j]);
            }
        }

        Mesh m_mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_mesh;

        m_mesh.vertices = vertices.ToArray();
        m_mesh.triangles = triangles.ToArray();
        m_mesh.RecalculateNormals();

    }

    void TriangulateSquare (Square square) {
        switch (square.configuration) {
            case 0:
                break;
            case 1:
                MeshFromPoints(square.center_bottom, square.bottom_left, square.center_left);
                break;
            case 2:
                MeshFromPoints(square.center_right, square.bottom_right, square.center_bottom);
                break;
            case 4:
                MeshFromPoints(square.center_top, square.top_right, square.center_right);
                break;
            case 8:
                MeshFromPoints(square.top_left, square.center_top, square.center_left);
                break;

            case 3:
                MeshFromPoints(square.center_right, square.bottom_right, square.bottom_left, square.center_left);
                break;
            case 6:
                MeshFromPoints(square.center_top, square.top_right, square.bottom_right, square.center_bottom);
                break;
            case 9:
                MeshFromPoints(square.top_left, square.center_top, square.center_bottom, square.bottom_left);
                break;
            case 12:
                MeshFromPoints(square.top_left, square.top_right, square.center_right, square.center_left);
                break;
            case 5:
                MeshFromPoints(square.center_top, square.top_right, square.center_right, square.center_bottom, square.bottom_left, square.center_left);
                break;
            case 10:
                MeshFromPoints(square.top_left, square.center_top, square.center_right, square.bottom_right, square.center_bottom, square.center_left);
                break;

            case 7:
                MeshFromPoints(square.center_top, square.top_right, square.bottom_right, square.bottom_left,  square.center_left);
                break;
            case 11:
                MeshFromPoints(square.top_left, square.center_top, square.center_right, square.bottom_right, square.bottom_left);
                break;
            case 13:
                MeshFromPoints(square.top_left, square.top_right, square.center_right, square.center_bottom, square.bottom_left);
                break;
            case 14:
                MeshFromPoints(square.top_left, square.top_right, square.bottom_right, square.center_bottom, square.center_left);
                break;

            case 15:
                MeshFromPoints(square.top_left, square.top_right, square.bottom_right, square.bottom_left);
                break;
        }
    }

    void MeshFromPoints (params Node[] points) {
        AssignVertices(points);

        if (points.Length >= 3) {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4) {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5) {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6) {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices (Node[] points) {
        for (int i = 0; i < points.Length; ++i) {
            if (points[i].vertex_index == -1) {
                points[i].vertex_index = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle (Node a, Node b, Node c) {
        triangles.Add(a.vertex_index);
        triangles.Add(b.vertex_index);
        triangles.Add(c.vertex_index);
    }

    //private void OnDrawGizmos() {
    //    if (square_grid != null) {
    //        int width = square_grid.squares.GetLength(0);
    //        int height = square_grid.squares.GetLength(1);
    //        for (int i = 0; i < width; ++i) {
    //            for (int j = 0; j < height; ++j) {
    //                Gizmos.color = (square_grid.squares[i, j].top_left.active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(square_grid.squares[i, j].top_left.position, Vector3.one * 0.4f);

    //                Gizmos.color = (square_grid.squares[i, j].top_right.active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(square_grid.squares[i, j].top_right.position, Vector3.one * 0.4f);

    //                Gizmos.color = (square_grid.squares[i, j].bottom_left.active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(square_grid.squares[i, j].bottom_left.position, Vector3.one * 0.4f);

    //                Gizmos.color = (square_grid.squares[i, j].bottom_right.active) ? Color.black : Color.white;
    //                Gizmos.DrawCube(square_grid.squares[i, j].bottom_right.position, Vector3.one * 0.4f);

    //                Gizmos.color = Color.grey;
    //                Gizmos.DrawCube(square_grid.squares[i, j].center_top.position, Vector3.one * 0.15f);
    //                Gizmos.DrawCube(square_grid.squares[i, j].center_right.position, Vector3.one * 0.15f);
    //                Gizmos.DrawCube(square_grid.squares[i, j].center_bottom.position, Vector3.one * 0.15f);
    //                Gizmos.DrawCube(square_grid.squares[i, j].center_left.position, Vector3.one * 0.15f);
    //            }
    //        }
    //    }
    //}

    public class SquareGrid {
        public Square[,] squares;

        public SquareGrid(int[,] map, float square_size) {
            int node_countX = map.GetLength(0);
            int node_countY = map.GetLength(1);
            float map_width = node_countX * square_size;
            float map_height = node_countY * square_size;

            ControlNode[,] control_nodes = new ControlNode[node_countX, node_countY];

            for (int i = 0; i < node_countX; ++i) {
                for (int j = 0; j < node_countY; ++j) {
                    Vector3 pos = new Vector3(-map_width / 2 + i * square_size + square_size / 2, 0, -map_height / 2 + j * square_size + square_size / 2);
                    control_nodes[i, j] = new ControlNode(pos, map[i, j] == 1, square_size);
                }
            }

            squares = new Square[node_countX - 1, node_countY - 1];
            for (int i = 0; i < node_countX - 1; ++i) {
                for (int j = 0; j < node_countY - 1; ++j) {
                    squares[i, j] = new Square(control_nodes[i, j + 1], control_nodes[i + 1, j + 1], control_nodes[i, j], control_nodes[i + 1, j]);
                }
            }
        }
    }

    public class Square {
        public ControlNode top_left, top_right, bottom_left, bottom_right;
        public Node center_top, center_right, center_bottom, center_left;
        public int configuration;

        public Square (ControlNode _topleft, ControlNode _topright, ControlNode _bottomleft, ControlNode _bottomright) {
            top_left = _topleft;
            top_right = _topright;
            bottom_left = _bottomleft;
            bottom_right = _bottomright;

            center_top = top_left.right;
            center_right = bottom_right.above;
            center_bottom = bottom_left.right;
            center_left = bottom_left.above;

            if (top_left.active) configuration += 8;
            if (top_right.active) configuration += 4;
            if (bottom_right.active) configuration += 2;
            if (bottom_left.active) configuration += 1;


        }
    }

	public class Node {
        public Vector3 position;
        public int vertex_index = -1;

        public Node (Vector3 _pos) {
            position = _pos;
        }
    }

    public class ControlNode : Node {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float square_size) : base (_pos) {
            active = _active;
            above = new Node(position + Vector3.forward * square_size / 2f);
            right = new Node(position + Vector3.right * square_size / 2f);
        }
    }
}
