using System.Collections;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    #region Public Variables

    public int gridWidth = 20;
    public int gridHeight = 20;
    public float rotationDuration = 1f;

    public Transform gridTransform;

    public Transform gridHolderTransform;

    public bool IsRotating { get;  private set;}

    public static GridManager Instance { get; private set; }

    #endregion

    #region Private Variables

    private GravityDirection _gravity;
    private Vector2 _currentGravityVect;
    private GridCell[,] _grid;

    #endregion

    #region Private Methods

    private void Awake()
    {
        // If there is already a grid manager instance destroy current
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        InitGrid();
    }

    private void Start()
    {
        //InitGrid();
    }

    private void InitGrid()
    {
        _grid = new GridCell[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                _grid[x, y] = new GridCell();
            }
        }
    }

    private Vector2 GetGravity()
    {
        switch (_gravity)
        {
            case GravityDirection.Down:
                return Vector2.down;
            case GravityDirection.Up:
                return Vector2.up;
            case GravityDirection.Left:
                return Vector2.left;
            case GravityDirection.Right:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }

    private IEnumerator Rotate()
    {
        int zAngle = 0;

        switch (_gravity)
        {
            case GravityDirection.Down:
                zAngle = 0;
                break;
            case GravityDirection.Left:
                zAngle = 90;
                break;
            case GravityDirection.Up:
                zAngle = 180;
                break;
            case GravityDirection.Right:
                zAngle = 270;
                break;
        }

        IsRotating = true;
        Quaternion newRotation = Quaternion.Euler(0, 0, zAngle);
        Quaternion originalRotation = gridHolderTransform.rotation;

        float time = rotationDuration;

        while (time > 0.0f)
        {
            time -= Time.deltaTime;
            gridHolderTransform.rotation =
                Quaternion.Lerp(originalRotation, newRotation, rotationDuration - time);

            yield return null;
        }

        IsRotating = false;
    }

    #region RemoveRow

    // Call these functions to remove a row of blocks
    // There are two different functions since we need to either remove
    // a x row or y row, depending on the gravity direction
    private void RemoveRowX(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            DestoryBlock(x, y);
        }
    }

    private void RemoveRowY(int x)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            DestoryBlock(x, y);
        }
    }

    private void DestoryBlock(int x, int y)
    {
        var block = _grid[x, y].Block;
        _grid[x, y].SetBlock(null);
        Destroy(block);
    }

    #endregion

    #region RowsGravity
    
    // Call these functions after removing a full row to make the 
    // blocks above fall through
    private void RowsGravityX(int y)
    {
        if (_gravity == GravityDirection.Down)
        {
            for (int i = y; i <= gridHeight / 2; i++)
            {
                RowMoveX(i);
            }
        }
        else if (_gravity == GravityDirection.Up)
        {
            for (int i = y; i >= gridHeight / 2; i--)
            {
                RowMoveX(i);
            }
        }
    }

    private void RowsGravityY(int x)
    {
        if (_gravity == GravityDirection.Left)
        {
            for (int i = x; i <= gridWidth / 2; i++)
            {
                RowMoveY(i);
            }
        }
        else if (_gravity == GravityDirection.Up)
        {
            for (int i = x; i >= gridWidth / 2; i--)
            {
                RowMoveY(i);
            }
        }
    }

    #endregion

    #region RowMove

    private void RowMoveX(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            BlockFallThrough(x, y);
        }
    }

    private void RowMoveY(int x)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            BlockFallThrough(x, y);
        }
    }

    private void BlockFallThrough(int x, int y)
    {
        if (_grid[x, y] != null &&
            _grid[x, y].CellType == CellType.Block)
        {
            // todo: change so it will fall through the entire way down with the *remaining* shape
            GridCell cell = _grid[x, y];
            _grid[x + (int)_currentGravityVect.x, y + (int)_currentGravityVect.y]
                .SetBlock(cell.Block);

            // Update the block position, we use world position so it will go down
            // on the y axis
            cell.BlockPosition.position += Vector3.down;
            cell.SetBlock(null);
        }
    }

    #endregion

    #region IsRowFull

    private bool IsRowFullX(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (_grid[x, y] == null || 
                _grid[x, y].CellType == CellType.Empty)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsRowFullY(int x)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            if (_grid[x, y] == null ||
                _grid[x, y].CellType == CellType.Empty)
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region RemoveFullRows by direction

    private int RemoveFullRowsByDirectionX()
    {
        int count = 0;

        // Since we need to remove rows from bottom up we need to consider gravity
        if (_gravity == GravityDirection.Down)
        {
            // The reason we go only to half of the height
            // is because the spawn in the middle and we don't
            // want to remove rows above that may gotten filled
            // between rotations
            for (int y = 0; y <= gridHeight / 2; ++y)
            {
                if (RemoveFullRowX(y))
                {
                    // if we removed a row then it means we moved blocks from the
                    // row above lower, then we need to recheck the current row
                    // in case it got filled
                    y--;
                    count++;
                }
            }
        }
        else if (_gravity == GravityDirection.Up)
        {
            for (int y = gridHeight - 1; y >= gridHeight / 2; --y)
            {
                if (RemoveFullRowX(y))
                {
                    y++;
                    count++;
                }
            }
        }

        return count;
    }

    private bool RemoveFullRowX(int y)
    {
        // If the row is full remove the blocks in the row
        // and apply gravity on the blocks above
        if (IsRowFullX(y))
        {
            RemoveRowX(y);

            // To get to the row above we need to go "against" gravity
            RowsGravityX(y - (int)_currentGravityVect.y);

            return true;
        }

        return false;
    }

    private int RemoveFullRowsByDirectionY()
    {
        int count = 0;

        if (_gravity == GravityDirection.Left)
        {
            for (int x = 0; x <= gridWidth / 2; ++x)
            {
                if (RemoveFullRowY(x))
                {
                    x--;
                    count++;
                }
            }
        }
        else if (_gravity == GravityDirection.Right)
        {
            for (int x = gridWidth - 1; x >= gridWidth / 2; --x)
            {
                if (RemoveFullRowY(x))
                {
                    x++;
                    count++;
                }
            }
        }

        return count;
    }

    private bool RemoveFullRowY(int x)
    {
        if (IsRowFullY(x))
        {
            RemoveRowY(x);
            RowsGravityY(x -(int)_currentGravityVect.x);

            return true;
        }

        return false;
    }

    #endregion

    #endregion

    #region Public Methods

    public void SetGravity(GravityDirection direction)
    {
        if (_gravity != direction)
        {
            _gravity = direction;
            _currentGravityVect = GetGravity();

            StartCoroutine(Rotate());
        }
    }

    // Returns int which represents how many rows were removed
    // For scoring purposes
    public int RemoveFullRows()
    {
        if (_gravity == GravityDirection.Down || 
            _gravity == GravityDirection.Up)
        {
            return RemoveFullRowsByDirectionX();
        }
        else
        {
            return RemoveFullRowsByDirectionY();
        }
    }

    public GridCell GetCell(int x, int y)
    {
        if (IsInsideBorder(x, y)) return _grid[x, y];

        return null;
    }

    public void SetCell(int x, int y, Transform block)
    {
        if (IsInsideBorder(x, y))
        {
            GridCell cell = _grid[x, y];

            if (cell.CellType == CellType.Empty)
            {
                //Debug.Log("Block at " + x + ", " + y + ", " + block.name
                //    + ", " + block.parent.parent.name);
                cell.SetBlock(block.gameObject);
            }
        }
    }

    public bool IsInsideBorder(int x, int y)
    {
        return x >= 0 && x < gridWidth &&
            y >= 0 && y < gridHeight;
    }

    #endregion
}

#region Helper Classes

public enum CellType
{
    Empty,
    Block,
    // For future purposes
    Wall
}

public class GridCell
{
    #region Public Variables

    public Transform BlockPosition
    {
        get
        {
            if (_block != null) return _block.transform;
            return null;
        }
    }

    // We need a reference to the shape so we'll know how to make the blocks
    // fall through after a row removal, for future purposes
    //public GameObject Shape {  get { return _shape; } }

    public GameObject Block { get { return _block; } }

    public CellType CellType { get { return _cellType; } }

    #endregion

    #region Private Variables

    private GameObject _shape;
    private GameObject _block;
    private CellType _cellType;

    #endregion

    #region Constructor

    public GridCell()
    {
        _cellType = CellType.Empty;
    }

    // For future purposes
    public GridCell(CellType cellType)
    {
        _cellType = cellType;
    }

    public GridCell(/*GameObject shape,*/ GameObject block)
    {
        SetBlock(/*shape,*/ block);
    }

    #endregion

    #region Public Methods

    public void SetBlock(/*GameObject shape,*/ GameObject block)
    {
        //_shape = shape;
        _block = block;

        if (block == null)
        {
            _cellType = CellType.Empty;
        }
        else
        {
            _cellType = CellType.Block;
        }
    }

    #endregion
}

#endregion
