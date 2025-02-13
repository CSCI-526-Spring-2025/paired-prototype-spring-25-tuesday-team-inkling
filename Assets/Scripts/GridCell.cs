using UnityEngine;

public class GridCell : MonoBehaviour
{
    // Coordinates in the grid
    public int x;
    public int y; 
    public BuildingType buildingType;

    // Flag to indicate if the cell is occupied
    public bool isOccupied = false;

    // (Optional) You could add more properties such as cell state (normal, disabled, highlighted, etc.)
    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
        // Update visual appearance (e.g., color change) if desired.
    }
}
