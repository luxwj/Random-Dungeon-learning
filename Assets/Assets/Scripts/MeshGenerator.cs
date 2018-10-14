using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    public SquareGrid square_grid;
    public MeshFilter walls;
    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangle_dictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checked_vertices = new HashSet<int>();
    
    public void GenerateMesh(int[,] map, float square_size) {

        triangle_dictionary.Clear();
        outlines.Clear();
        checked_vertices.Clear();

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

        CreateWallMesh();

    }

    void CreateWallMesh(){

        CalculateMeshOutlines();
        List<Vector3> wall_vertices = new List<Vector3>();
        List<int> wall_triangles = new List<int>();
        Mesh wall_mesh = new Mesh();
        float wall_height = 5;

        foreach(List<int> outline in outlines){
            for (int i = 0; i < outline.Count - 1; ++i){
                int start_index = wall_vertices.Count;
                wall_vertices.Add(vertices[outline[i]]);    //left
                wall_vertices.Add(vertices[outline[i + 1]]);    //left
                wall_vertices.Add(vertices[outline[i]] - Vector3.up * wall_height);    //bottom left
                wall_vertices.Add(vertices[outline[i + 1]] - Vector3.up * wall_height);    //bottom left

                wall_triangles.Add(start_index + 0);
                wall_triangles.Add(start_index + 2);
                wall_triangles.Add(start_index + 3);

                wall_triangles.Add(start_index + 3);
                wall_triangles.Add(start_index + 1);
                wall_triangles.Add(start_index + 0);
            }
        }
        wall_mesh.vertices = wall_vertices.ToArray();
        wall_mesh.triangles = wall_triangles.ToArray();
        walls.mesh = wall_mesh;
    }

    void TriangulateSquare (Square square) {
        switch (square.configuration) {
            case 0:
                break;
            case 1:
                MeshFromPoints(square.center_left, square.center_bottom, square.bottom_left);
                break;
            case 2:
                MeshFromPoints(square.bottom_right, square.center_bottom, square.center_right);
                break;
            case 4:
                MeshFromPoints(square.top_right, square.center_right, square.center_top);
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
                checked_vertices.Add(square.top_left.vertex_index);
                checked_vertices.Add(square.top_right.vertex_index);
                checked_vertices.Add(square.bottom_right.vertex_index);
                checked_vertices.Add(square.bottom_left.vertex_index);
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

        Triangle triangle = new Triangle(a.vertex_index, b.vertex_index, c.vertex_index);
        AddTriangleToDictionary(triangle.vertex_indexA, triangle);
        AddTriangleToDictionary(triangle.vertex_indexB, triangle);
        AddTriangleToDictionary(triangle.vertex_indexC, triangle);
    }

    void AddTriangleToDictionary(int vertex_index_key, Triangle triangle){
        if (triangle_dictionary.ContainsKey (vertex_index_key)){
            triangle_dictionary[vertex_index_key].Add(triangle);
        }else{
            List<Triangle> triangle_list = new List<Triangle>();
            triangle_list.Add(triangle);
            triangle_dictionary.Add(vertex_index_key, triangle_list);
        }
    }

    void CalculateMeshOutlines(){
        for (int vertex_index = 0; vertex_index < vertices.Count; ++vertex_index){
            if (!checked_vertices.Contains(vertex_index)){
                int new_outline_vertex = GetConnectOutlineVertex(vertex_index);
                if (new_outline_vertex != -1) {
                    checked_vertices.Add(vertex_index);

                    List<int> new_outline = new List<int>();
                    new_outline.Add(vertex_index);
                    outlines.Add(new_outline);
                    FollowOutlines(new_outline_vertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertex_index);
                }
            }
        }
    }

    void FollowOutlines(int vertex_index, int outline_index){
        outlines[outline_index].Add(vertex_index);
        checked_vertices.Add(vertex_index);
        int next_vertex_index = GetConnectOutlineVertex(vertex_index);

        if (next_vertex_index != -1){
            FollowOutlines(next_vertex_index, outline_index);
        }
    }

    int GetConnectOutlineVertex(int vertex_index){
        List <Triangle>triangles_containing_vertex = triangle_dictionary[vertex_index];

        for (int i = 0; i < triangles_containing_vertex.Count; ++i){
            Triangle triangle = triangles_containing_vertex[i];

            for (int j = 0; j < 3; ++j){
                int vertexB = triangle[j];
                if (vertexB != vertex_index && !checked_vertices.Contains(vertexB)){
                    if (IsOutlineEdge(vertex_index, vertexB)){
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB){
        List<Triangle> trianglesContainingVertexA = triangle_dictionary[vertexA];
        int shared_triangle_count = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; ++i){
            if (trianglesContainingVertexA[i].Contains(vertexB)){
                ++shared_triangle_count;
                if (shared_triangle_count > 1){
                    break;
                }
            }
        }

        return shared_triangle_count == 1;
    }

    struct Triangle
    {
        public int vertex_indexA;
        public int vertex_indexB;
        public int vertex_indexC;
        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertex_indexA = a;
            vertex_indexB = b;
            vertex_indexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i] {
            get{
                return vertices[i];
            }
        }

        public bool Contains (int vertex_index){
            return vertex_index == vertex_indexA || vertex_index == vertex_indexB || vertex_index == vertex_indexC;
        }
    }

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
