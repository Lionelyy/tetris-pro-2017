using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Public Variables

    // Should be per shape, based on shape difficulty
    public int shapePlacementPoints = 25;
    public int rowRemovalPoints = 100;
    public int fourRowsRemoval = 800;
    public ShapeSpawner spawner;
    public float rotationFrequency = 10f;

    public GameObject gameOverText;

    public Text scoreText;
    public Text linesText;
    public Text rotationText;
    public Image nextSpawnImage;

    public static GameManager Instance { get; private set; }

    #endregion

    #region Private Variables

    private int _score;
    private int _lines;
    private float _rotationTimer = 0;
    private GravityDirection _direction;
    private bool _isGameOver = false;

    #endregion

    #region Private Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _direction = GravityDirection.Down;
        GridManager.Instance.SetGravity(_direction);

        UpdateNextSpawn();
    }

    private void Update()
    {
        _rotationTimer += Time.deltaTime;

        if (_rotationTimer >= rotationFrequency && !_isGameOver)
        {
            _direction = ChangeGravity();

            GridManager.Instance.SetGravity(_direction);

            _rotationTimer = 0f;
        }

        if (rotationText)
        {
            rotationText.text = string.Format("{0} Seconds", Mathf.Ceil(rotationFrequency - _rotationTimer));
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }

    private GravityDirection ChangeGravity()
    {
        switch (_direction)
        {
            case GravityDirection.Down:
                return GravityDirection.Left;
            case GravityDirection.Left:
                return GravityDirection.Up;
            case GravityDirection.Up:
                return GravityDirection.Right;
            case GravityDirection.Right:
                return GravityDirection.Down;
            default:
                return GravityDirection.Down;
        }
    }

    private void UpdateNextSpawn()
    {
        Sprite prefab = spawner.GetNextSpawn();

        if (prefab != null && nextSpawnImage != null)
        {
            nextSpawnImage.type = Image.Type.Simple;
            nextSpawnImage.preserveAspect = true;

            nextSpawnImage.sprite = prefab;
        }
    }

    private void UpdateUI()
    {
        if (scoreText)
        {
            scoreText.text = _score.ToString();
        }

        if (linesText)
        {
            linesText.text = _lines.ToString();
        }
    }

    #endregion

    #region Public Methods

    public void ShapeLanded()
    {
        int rowsRemoved = GridManager.Instance.RemoveFullRows();
        int pointsToAdd = rowsRemoved * rowRemovalPoints;

        // Award for removing several rows
        if (rowsRemoved > 4)
        {
            pointsToAdd = (rowsRemoved - 4) * rowRemovalPoints + fourRowsRemoval;
        }

        if (rowsRemoved == 0)
        {
            pointsToAdd = shapePlacementPoints;
        }

        _lines += rowsRemoved;
        _score += pointsToAdd;

        if (spawner != null && !_isGameOver)
        {
            spawner.SpawnNext();
            UpdateNextSpawn();
        }

        UpdateUI();
    }

    public void GameOver()
    {
        _isGameOver = true;
        gameOverText.SetActive(true);
    }

    #endregion
}

public enum GravityDirection
{
    Down,
    Up,
    Left,
    Right
}
