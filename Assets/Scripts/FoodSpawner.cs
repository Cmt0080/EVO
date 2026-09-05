using UnityEngine;


public class FoodSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject foodPrefab;
    public Vector2 fieldMin = new Vector2(-8f, -4.5f); // bottom-left corner
    public Vector2 fieldMax = new Vector2(8f, 4.5f);   // top-right corner 

    [Header("Base Spawn Settings")]
    public float baseSpawnInterval = 1.5f; // the seconds between spawns under normal conditions
    public int foodPerSpawn = 6;           // how many food items appear each spawn tick speed 

    // INTERNAL STATE
    private float spawnTimer = 0f;
    private bool scarcityActive = false;
    private bool droughtActive = false;

    void Update()
    {
        // If drought is active, food stops spawning entirely.
        if (droughtActive) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            int amountToSpawn = scarcityActive ? Mathf.Max(1, foodPerSpawn / 2) : foodPerSpawn;

            for (int i = 0; i < amountToSpawn; i++)
            {
                SpawnFood();
            }

            spawnTimer = baseSpawnInterval;
        }
    }

    void SpawnFood()
    {
        float x = Random.Range(fieldMin.x, fieldMax.x);
        float y = Random.Range(fieldMin.y, fieldMax.y);
        Instantiate(foodPrefab, new Vector3(x, y, 0f), Quaternion.identity);
    }

    // CALLED BY SIM MANAGER 
    public void SetScarcity(bool isActive)
    {
        scarcityActive = isActive;
    }

    public void SetDrought(bool isActive)
    {
        droughtActive = isActive;
    }
}
