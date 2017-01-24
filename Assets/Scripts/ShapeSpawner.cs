using UnityEngine;

public class ShapeSpawner : MonoBehaviour
{
    #region Public Variables

    // An array of all the shape prefabs to spawn randomly from
    public GameObject[] shapePrefabs;
    public Sprite[] shapeSprites;

    #endregion

    #region Private Variables

    private int _nextSpawn;

    #endregion

    #region Private Methods

    private void Start()
    {
        _nextSpawn = Random.Range(0, shapePrefabs.Length);
        SpawnNext();
    }

    #endregion

    #region Public Methods

    public Sprite GetNextSpawn()
    {
        if (shapeSprites.Length > 0 && 
            shapeSprites.Length >= _nextSpawn)
        {
            return shapeSprites[_nextSpawn];
        }

        return null;
    }

    public void SpawnNext()
    {
        // Spawn a random shape at the spawners' position
        if (shapePrefabs.Length > 0)
        {
            GameObject shape = Instantiate(shapePrefabs[_nextSpawn],
                        transform.position,
                        Quaternion.identity);

            // We make the shape a child of the grid to make gravity management easier
            shape.transform.parent = transform.parent;

            _nextSpawn = Random.Range(0, shapePrefabs.Length);
        }
    }

    #endregion
}
