using UnityEngine;
public enum BuildingType
{
    House,
    Factory
}

[System.Serializable]
public class Card
{
    public BuildingType buildingType;
    // You can add more properties (e.g., cost, range, special effects)
}

public class GridManager : MonoBehaviour
{
    // Reference to the cell prefab
    public GameObject cellPrefab;

    // Grid dimensions (initially 5x5)
    public int gridWidth = 5;
    public int gridHeight = 5;

    // 2D array to store references to the grid cells
    private GridCell[,] gridCells;

    // Offset between cells (adjust based on your sprite size)
    public float cellSpacing = 1.1f;

    void Start()
    {
        CreateGrid();
    }

    // Create the grid at the start of the game
    void CreateGrid()
    {
        gridCells = new GridCell[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 cellPos = new Vector3(x * cellSpacing, y * cellSpacing, 0);
                GameObject cellGO = Instantiate(cellPrefab, cellPos, Quaternion.identity, transform);
                GridCell cell = cellGO.GetComponent<GridCell>();

                // Set coordinates for reference
                cell.x = x;
                cell.y = y;

                gridCells[x, y] = cell;
            }
        }
    }

    // Utility method to get a cell by its grid coordinates
    public GridCell GetCell(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return gridCells[x, y];
        else
            return null;
    }

    // Example: update adjacent cells when a building is placed
    public void UpdateAdjacentCells(int x, int y, BuildingType buildingType)
    {
        // Define directions for adjacent cells (up, down, left, right)
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (var dir in directions)
        {
            GridCell adjacent = GetCell(x + dir.x, y + dir.y);
            if (adjacent != null && !adjacent.isOccupied && !adjacent.isOccupied)
            {
                if (buildingType == BuildingType.Factory)
                {
                    // 🏭 Rule: Occupy adjacent cells when placing a Factory
                    adjacent.isOccupied = true;
                    adjacent.buildingType = BuildingType.Factory;
                    adjacent.GetComponent<SpriteRenderer>().color = Color.red;  // Change color to show it's occupied
                }
            }
        }
    }

    // (Optional) Method to expand the grid during a level change
    public void ExpandGrid(int addWidth, int addHeight)
    {
        // For a dynamic grid expansion, you might:
        // 1. Create a new array with new dimensions.
        // 2. Copy existing cells over.
        // 3. Instantiate new cells for the added area.
        // This example simply logs a message.
        Debug.Log("Expanding grid by: " + addWidth + " x " + addHeight);
    }
}
         