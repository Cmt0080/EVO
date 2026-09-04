using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject preyPrefab;
    public GameObject predatorPrefab;

    [Header("Play Field Bounds")]
    public Vector2 fieldMin = new Vector2(-8f, -4.5f);
    public Vector2 fieldMax = new Vector2(8f, 4.5f);

    [Header("Initial Population")]
    public int initialPreyCount = 20;

    [Header("Predator Toggle Settings")]
    public int predatorCountWhenActive = 4;

    [Header("References")]
    public FoodSpawner foodSpawner;

    [Header("UI")]
    public Text populationCountText; 

   
    private List<GameObject> activePredators = new List<GameObject>();
    private bool predatorsToggled = false;

    void Start()
    {
        SpawnInitialPrey();
    }

    void Update()
    {
        UpdatePopulationUI();
    }

   // Initial Spawn
    void SpawnInitialPrey()
    {
        for (int i = 0; i < initialPreyCount; i++)
        {
            Vector3 pos = RandomPointInField();
            Instantiate(preyPrefab, pos, Quaternion.identity);
        }
    }

    Vector3 RandomPointInField()
    {
        float x = Random.Range(fieldMin.x, fieldMax.x);
        float y = Random.Range(fieldMin.y, fieldMax.y);
        return new Vector3(x, y, 0f);
    }

    // Toggle Button =  PREDATORS
    public void TogglePredators()
    {
        predatorsToggled = !predatorsToggled;

        if (predatorsToggled)
        {
            for (int i = 0; i < predatorCountWhenActive; i++)
            {
                Vector3 pos = RandomPointInField();
                GameObject predator = Instantiate(predatorPrefab, pos, Quaternion.identity);
                activePredators.Add(predator);
            }
        }
        else
        {
            // Deletes predators from memory immediately
            foreach (GameObject predator in activePredators)
            {
                if (predator != null) Destroy(predator);
            }
            activePredators.Clear();
        }
    }

    // Toggle Button =  SCARCITY
    private bool scarcityToggled = false;
    public void ToggleScarcity()
    {
        scarcityToggled = !scarcityToggled;
        if (foodSpawner != null)
        {
            foodSpawner.SetScarcity(scarcityToggled);
        }
    }

    // Toggle Button = DROUGHT
    private bool droughtToggled = false;
    public void ToggleDrought()
    {
        droughtToggled = !droughtToggled;
        if (foodSpawner != null)
        {
            foodSpawner.SetDrought(droughtToggled);
        }
    }

    // UI
    void UpdatePopulationUI()
    {
        if (populationCountText == null) return;

        int preyCount = GameObject.FindGameObjectsWithTag("Prey").Length;
        int predatorCount = GameObject.FindGameObjectsWithTag("Predator").Length;

        populationCountText.text = "Prey: " + preyCount + "   Predators: " + predatorCount;
    }
}
