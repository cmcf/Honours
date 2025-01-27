using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public Room room;
    [System.Serializable]
    public struct Grid
    {
        public int columns, rows;
        public float verticalOffset, horizontalOffset;
    }

    public Grid grid;
    public GameObject gridTile;
    public List<Vector2> availablePoints = new List<Vector2>();

    void Awake()
    {
        room = GetComponentInParent<Room>();
        grid.columns = room.width - 2;
        grid.rows = room.height - 2;
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        // Adjust offsets to account for room's position in world space
        grid.verticalOffset += room.transform.localPosition.y;
        grid.horizontalOffset += room.transform.localPosition.x;

        // Loops through the grid dimensions to place tiles
        for (int y = 0; y < grid.rows; y++)
        {
            for (int x = 0; x < grid.columns; x++)
            {
                // Checks if grid tile is set
                if (gridTile != null)
                {
                    // Instantiate a new grid tile at the calculated position
                    GameObject go = Instantiate(gridTile, transform);
                    go.transform.localPosition = new Vector2(x - (grid.columns - grid.horizontalOffset),
                                                            y - (grid.rows - grid.verticalOffset));
                    go.name = "X: " + x + "Y: " + y;

                    // Add the position to the available points list for object placement
                    availablePoints.Add(go.transform.position);

                    // Set the tile as inactive
                    go.SetActive(false);
                }
            }
        }

        // Trigger object spawning after grid has been generated
        GetComponentInParent<ObjectRoomSpawner>().InitialiseObjectSpawning();
    }
}
