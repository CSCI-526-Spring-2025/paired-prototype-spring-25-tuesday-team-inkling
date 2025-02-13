using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using static HexagonGrid;

// Code is refrenced from: https://github.com/kchapelier/hexagrid-relaxing/blob/master/src/hexagrid.js
// Ported into C#
public class HexagonGrid
{
    // == Helper Objects == 
    public struct point
    {
        public float x;
        public float y;
        public bool isSide;
    }

    public struct triangle
    {
        public int index1;
        public int index2;
        public int index3;
        public bool flag; //FIXME: change name later
    }

    // Stores the index number in _points to get location
    public struct quadrilateral
    {
        public int index1;
        public int index2;
        public int index3;
        public int index4;
        public bool flag; //FIXME: change name later
    }

    // == Generation values ==
    private int _sideSize = 8;
    private int _groupCount = 8;
    private int _seed = 1;
    private float _sideLength = 0.8660254037844386f;

    // == Grid Storage ==
    private List<point> _points = new List<point>();
    private List<triangle> _triangles = new List<triangle>();


    private HexagonGrid hexGrid;
    private List<int> selectedTriangles = new List<int>();
    private HashSet<int> allAdjacentTriangles = new HashSet<int>();

    // == Initalization Function ==

    private Vector3 CalculateCentroid(triangle tri)
    {
        point p1 = _points[tri.index1];
        point p2 = _points[tri.index2];
        point p3 = _points[tri.index3];

        // Calculate the centroid: (x1+x2+x3)/3, (y1+y2+y3)/3
        float centroidX = (p1.x + p2.x + p3.x) / 3;
        float centroidY = (p1.y + p2.y + p3.y) / 3;

        return new Vector3(centroidX * 10, centroidY * 10, 0.0f); // Scaling factor to make it visible
    }
    // Function to display text at the centroid of each triangle
    private void DisplayCentroidText(Dictionary<int, List<int>> adjacencyMap)
    {
        foreach (var entry in adjacencyMap)
        {
            int triangleIndex = entry.Key;
            triangle selectedTri = _triangles[triangleIndex];

            // Calculate the centroid of the triangle
            Vector3 centroid = CalculateCentroid(selectedTri);

            // Display the triangle index at the centroid (GUI.Label or TextMesh)
            // Option 1: Use GUI.Label (simple, but works with UI)
            GameObject textObject = new GameObject("TriangleText_" + triangleIndex);
            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = triangleIndex.ToString();
            textMesh.fontSize = 8;
            textMesh.color = Color.white;
            textObject.transform.position = centroid;

        }
    }
    public HexagonGrid(int sideSize, int searchIterationCount)
    {
        if (sideSize < 2)
        {
            Debug.LogError("Hexgrid can not be initalized with size < 2");
        }

        _sideSize = sideSize;
        //_seed = Random.Range(1, 60000);
        _groupCount = searchIterationCount;

        // grid point generation values
        float maxHeight = _sideSize * 2 - 1;
        float maxDeltaHeight = _sideSize - maxHeight * 0.5f;
        float ratio = maxHeight / 2 - maxDeltaHeight;

        // generate points on grid
        for (int x = 0; x < _sideSize * 2 - 1; x++)
        {
            int height = (x < _sideSize) ? _sideSize + x : _sideSize * 3 - 2 - x;
            float deltaHeight = _sideSize - height * 0.5f;
            for (int y = 0; y < height; y++)
            {
                point tempPoint;

                // location stored in self._points
                bool isSide = (x == 0) || x == (_sideSize * 2 - 2) || (y == 0) || (y == height - 1);
                tempPoint.x = (x - _sideSize + 1) * _sideLength / ratio;
                tempPoint.y = (y + deltaHeight - maxHeight / 2.0f) / ratio;
                tempPoint.isSide = isSide;
                Debug.Log("(" + tempPoint.x + ", " + tempPoint.y + ")");
                // Store point
                _points.Add(tempPoint);
            }
        }

        int offset = 0;
        // generate triangle by index refrence points to _points
        for (int x = 0; x < (sideSize * 2 - 2); x++)
        {
            int height = (x < sideSize) ? (sideSize + x) : (sideSize * 3 - 2 - x);
            triangle tempTri;
            if (x < sideSize - 1)
            {
                // left side
                for (int y = 0; y < height; y++)
                {

                    tempTri.index1 = offset + y;
                    tempTri.index2 = offset + y + height;
                    tempTri.index3 = offset + y + height + 1;
                    tempTri.flag = true;
                    _triangles.Add(tempTri);

                    if (y >= height - 1)
                    {
                        break;
                    }

                    tempTri.index1 = offset + y + height + 1;
                    tempTri.index2 =  offset + y + 1;
                    tempTri.index3 =  offset + y;
                    tempTri.flag = true;
                    _triangles.Add(tempTri);
                }
            }
            else
            {
                tempTri.index1 = offset + 0;
                tempTri.index2 = offset + 0 + height;
                tempTri.index3 = offset + 0 + 1;
                tempTri.flag = true;
                _triangles.Add(tempTri);

                tempTri.index1 = offset + height - 2;
                tempTri.index2 = offset + height - 2 + height;
                tempTri.index3 = offset + height - 2 + 1;
                tempTri.flag = true;
                _triangles.Add(tempTri);

                // right side
                for (int y = 0; y < height - 1; y++)
                {
                    tempTri.index1 = offset + y;
                    tempTri.index2 = offset + y + height;
                    tempTri.index3 = offset + y + 1;
                    tempTri.flag = true;
                    //_triangles.Add(tempTri);

                    if (y >= height - 2)
                    {
                        break;
                    }

                    tempTri.index1 = offset + y + 1;
                    tempTri.index2 = offset + y + height;
                    tempTri.index3 = offset + y + height + 1;
                    tempTri.flag = true;
                    _triangles.Add(tempTri);
                }
            }

            offset += height;
        }

        //DEBUGGING USE ONLY
        if (true)
        {
            int temp = 1;

            for (int i = 0; i < _triangles.Count; i++)
            {
                point tempPoint1 = _points[_triangles[i].index1];
                point tempPoint2 = _points[_triangles[i].index2];
                point tempPoint3 = _points[_triangles[i].index3];
                

                int r = 1;
                int g = 1;
                int b = 1;

                Debug.DrawLine(new Vector3(tempPoint1.x * 10, tempPoint1.y * 10, 0.0f), new Vector3(tempPoint2.x * 10, tempPoint2.y * 10, 0.0f), Color.red, 1000f);
                Debug.DrawLine(new Vector3(tempPoint2.x * 10, tempPoint2.y * 10, 0.0f), new Vector3(tempPoint3.x * 10, tempPoint3.y * 10, 0.0f), Color.blue, 1000f);
                Debug.DrawLine(new Vector3(tempPoint3.x * 10, tempPoint3.y * 10, 0.0f), new Vector3(tempPoint1.x * 10, tempPoint1.y * 10, 0.0f), Color.green, 1000f);
                r += 1;
                g += 1;
                b += 1;

            }
            

            int triangleRemoveCount = 10;
            int totalTriangles = _triangles.Count;
            Debug.Log("Size: " + totalTriangles);
            HashSet<int> selectedTriangleIndices = new HashSet<int>();
            while (selectedTriangleIndices.Count < triangleRemoveCount)
            {
                int randomIndex = Random.Range(0, totalTriangles);
                selectedTriangleIndices.Add(randomIndex);
            }
            selectedTriangles.AddRange(selectedTriangleIndices);

            for (int i = 0; i < selectedTriangles.Count; i++)
            {
                point tempPoint1 = _points[_triangles[selectedTriangles[i]].index1];
                point tempPoint2 = _points[_triangles[selectedTriangles[i]].index2];
                point tempPoint3 = _points[_triangles[selectedTriangles[i]].index3];


                int r = 1;
                int g = 1;
                int b = 1;

                Debug.DrawLine(new Vector3(tempPoint1.x * 10, tempPoint1.y * 10, 0.0f), new Vector3(tempPoint2.x * 10, tempPoint2.y * 10, 0.0f), Color.white, 1000f);
                Debug.DrawLine(new Vector3(tempPoint2.x * 10, tempPoint2.y * 10, 0.0f), new Vector3(tempPoint3.x * 10, tempPoint3.y * 10, 0.0f), Color.white, 1000f);
                Debug.DrawLine(new Vector3(tempPoint3.x * 10, tempPoint3.y * 10, 0.0f), new Vector3(tempPoint1.x * 10, tempPoint1.y * 10, 0.0f), Color.white, 1000f);
                r += 1;
                g += 1;
                b += 1;

            }

            Dictionary<int, List<int>> adjacencyMap = new Dictionary<int, List<int>>();

            Debug.Log("Selected Triangle Indices: " + string.Join(", ", selectedTriangles));
            // Iterate through selected triangles
            foreach (int triIndex in selectedTriangleIndices)
            {
                List<int> adjacentTriangles = new List<int>();

                // Get the triangle
                triangle selectedTri = _triangles[triIndex];

                // Iterate through all triangles to find adjacent ones
                for (int j = 0; j < _triangles.Count; j++)
                {
                    if (j == triIndex) continue; // Skip itself

                    triangle otherTri = _triangles[j];

                    // Count shared vertices
                    int sharedVertices = 0;
                    if (selectedTri.index1 == otherTri.index1 || selectedTri.index1 == otherTri.index2 || selectedTri.index1 == otherTri.index3) sharedVertices++;
                    if (selectedTri.index2 == otherTri.index1 || selectedTri.index2 == otherTri.index2 || selectedTri.index2 == otherTri.index3) sharedVertices++;
                    if (selectedTri.index3 == otherTri.index1 || selectedTri.index3 == otherTri.index2 || selectedTri.index3 == otherTri.index3) sharedVertices++;

                    // If two triangles share 2 vertices, they are adjacent
                    if (sharedVertices == 2)
                    {
                        adjacentTriangles.Add(j);
                    }
                }

                // Store the adjacency list for this triangle
                adjacencyMap[triIndex] = adjacentTriangles;
            }


            DisplayCentroidText(adjacencyMap);



            // Debugging: Print adjacency list
            foreach (var entry in adjacencyMap)
            {
                Debug.Log($"Triangle {entry.Key} has {entry.Value.Count} adjacent triangles: {string.Join(", ", entry.Value)}");
            }

        }

    }

    private int getAdjacentTriangles(int triIndex, List<int> adjacents)
    {
        adjacents.Add( triIndex );

        for (int i = 0; i <  _triangles.Count; i++)
        {
            triangle currentTriangle = _triangles[i];
            if (i == triIndex || currentTriangle.flag != true)
            {
                continue;
            }

            int shareCount = 0;
            // Check if we share index on point 1
            if (currentTriangle.index1 == _triangles[triIndex].index1)
            {
                shareCount++;
            }
            if (currentTriangle.index1 == _triangles[triIndex].index2)
            {
                shareCount++;
            }
            if (currentTriangle.index1 == _triangles[triIndex].index3)
            {
                shareCount++;
            }
            // Check if we share index on point 2
            if (currentTriangle.index2 == _triangles[triIndex].index1)
            {
                shareCount++;
            }
            if (currentTriangle.index2 == _triangles[triIndex].index2)
            {
                shareCount++;
            }
            if (currentTriangle.index2 == _triangles[triIndex].index3)
            {
                shareCount++;
            }
            // Check if we share index on point 3
            if (currentTriangle.index3 == _triangles[triIndex].index1)
            {
                shareCount++;
            }
            if (currentTriangle.index3 == _triangles[triIndex].index2)
            {
                shareCount++;
            }
            if (currentTriangle.index3 == _triangles[triIndex].index3)
            {
                shareCount++;
            }

            //assert(shareCount < 3);
            if (shareCount == 2)
            {
                adjacents.Add(i);
            }
        }

        return adjacents.Count;
    }

}
