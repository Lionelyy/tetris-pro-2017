using UnityEngine;

public class ShapeController : MonoBehaviour
{
    #region Public Variables

    // For visually better rotations, transform.rotate doesn't rotate
    // the shape in the best manner
    public Vector2[] noRotation;
    public Vector2[] quarterRotation;
    public Vector2[] halfRotation;
    public Vector2[] threeQuartersRotation;

    #endregion

    #region Private Variables

    private float _fallTimer = 0f;

    private int _currentRotation = 0;
    private static int _rotateLeft =  90;
    private static int _rotateRight =  -90;

    #endregion

    #region Private Methods

    private void Start()
    {
        // If we spawned a shape and it had no space, it means game is over
        if (!CanMoveToPosition())
        {
            GameManager.Instance.GameOver();
            Destroy(this.gameObject);

            Debug.Log("Game Over");
        }
        else
        {
            SetPositionInGrid();
        }
    }

    private void Update()
    {
        // Stop all movement of the shapes while the grid is mid rotation
        if (GridManager.Instance.IsRotating)
        {
            return;
        }

        // Move shape right
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            Move(Vector3.right);
        }
        // Move shape left
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            Move(Vector3.left);
        }
        // Rotate left
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            Rotate(_rotateLeft);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(_rotateRight);
        }
        // Move shape down (with key press or without)
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) ||
            Time.time - _fallTimer >= 1)
        {
            bool successful = Move(Vector3.down);

            // if we were not able to move down, it means we landed on something
            if (!successful)
            {
                GameManager.Instance.ShapeLanded();

                enabled = false;
            }
            //else
            //{
            //    // this is so we stop immediately
            //    if (HasLanded())
            //    {
            //        GameManager.Instance.ShapeLanded();

            //        enabled = false;
            //    }
            //}

            _fallTimer = Time.time;
        }
    }

    private bool Move(Vector3 direction)
    {
        transform.position += direction;

        if (CanMoveToPosition())
        {
            UpdatePosition();
        }
        else
        {
            // Move shape back
            transform.position -= direction;
            return false;
        }

        return true;
    }

    private bool HasLanded()
    {
        bool hasLanded = false;
        transform.position += Vector3.down;

        if (!CanMoveToPosition())
        {
            hasLanded = true;
        }
        
        transform.position -= Vector3.down;

        return hasLanded;
    }

    private void Rotate(int rotation)
    {
        int newAngle = _currentRotation + rotation;

        newAngle = NormalizeAngle(newAngle);
        MoveBlocks(newAngle);

        if (CanMoveToPosition())
        {
            UpdatePosition();
            _currentRotation = newAngle;
        }
        else
        {
            MoveBlocks(_currentRotation);
        }
    }

    private int NormalizeAngle(int newAngle)
    {
        newAngle = newAngle % 360;

        if (newAngle < 0)
        {
            newAngle += 360;
        }

        return newAngle;
    }

    private void MoveBlocks(int angle)
    {
        if (angle == 0 || angle == 360)
        {
            ChangeBlocksLocalPosition(noRotation);
        }
        else if (angle == 90)
        {
            ChangeBlocksLocalPosition(quarterRotation);
        }
        else if (angle == 180)
        {
            ChangeBlocksLocalPosition(halfRotation);
        }
        else if (angle == 270)
        {
            ChangeBlocksLocalPosition(threeQuartersRotation);
        }
    }

    private void ChangeBlocksLocalPosition(Vector2[] locations)
    {
        if (locations.Length != transform.childCount)
            return;

        int index = 0;
        foreach (Transform child in transform)
        {
            var newLoc = locations[index];
            child.localPosition = newLoc;

            index++;
        }
    }

    private bool CanMoveToPosition()
    {
        // Go through all the child transforms of the shape
        foreach (Transform child in transform)
        {
            // We use local position to get the blocks' position relative to the grid
            // so we can use it while searching the grid.
            // We round the vector because things like rotations can cause coordinates
            // not to be round anymore.
            var inverse = GridManager.Instance.gridTransform.InverseTransformPoint(child.position);
            //Debug.Log("Inverse block " + child.name + ", " + inverse);
            Vector2 roundedPos = RoundVector(inverse);

            // Check whether or not there is a block in the cell and it doesn't belong
            // to the shape
            GridCell cell = GridManager.Instance.GetCell((int)roundedPos.x, (int)roundedPos.y);

            if (cell == null)
                return false;

            if (cell.CellType != CellType.Empty &&
                cell.BlockPosition != null && cell.BlockPosition.parent != transform)
                return false;
        }

        return true;
    }

    private void UpdatePosition()
    {
        // Remove blocks from their old positions in the grid
        for (int x = 0; x < GridManager.Instance.gridWidth; x++)
        {
            for (int y = 0; y < GridManager.Instance.gridHeight; y++)
            {
                GridCell cell = GridManager.Instance.GetCell(x, y);

                if (cell != null && cell.BlockPosition != null &&
                    cell.BlockPosition.parent == transform)
                {
                    cell.SetBlock(null);
                }
            }
        }

        SetPositionInGrid();
    }

    private void SetPositionInGrid()
    {
        foreach (Transform child in transform)
        {
            var inverse = GridManager.Instance.gridTransform.InverseTransformPoint(child.position);
            Vector2 roundedPos = RoundVector(inverse);
            GridManager.Instance.SetCell((int)roundedPos.x, (int)roundedPos.y, child);
        }
    }

    private Vector2 RoundVector(Vector2 vect)
    {
        return new Vector2(Mathf.Floor(vect.x),
                           Mathf.Floor(vect.y));
    }

    #endregion
}
