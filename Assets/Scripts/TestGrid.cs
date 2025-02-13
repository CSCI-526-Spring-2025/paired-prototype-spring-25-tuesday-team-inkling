using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrid : MonoBehaviour
{
    public int rows = 5;  // Number of rows
    public int cols = 5;  // Number of columns
    public float cellSize = 2f; // Distance between cells
    public GameObject cellPrefab; // Assign a prefab (like a Cube or UI Text)

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        int id = 0; // Unique ID for each grid cell

        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                // Calculate the position
                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);

                // Instantiate a cell (Prefab required)
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);

                // Assign a unique name
                cell.name = $"Cell_{id}";

                // Display the ID (if UI Text is used)
                TextMesh textMesh = cell.GetComponentInChildren<TextMesh>();
                if (textMesh != null)
                {
                    textMesh.text = $"ID: {id}";
                }

                id++; // Increment ID
            }
        }
    }
}
