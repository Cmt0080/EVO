using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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
    public GameObject startScreenPanel;

    [Header("Sound Effects")]
    public AudioSource clickSoundSource;
    public AudioSource musicSource;
    public AudioClip clickSound;
    public AudioClip gameOverSound;
    public AudioClip winSound;
    private bool extinctionAnnounced = false;
    private bool gameStarted = false;


    [Header("Overpopulation Warning")]
    public int overpopulationThreshhold = 300;
    private bool overpopulationWarned = false;

    [Header("UI")]
    public TMP_Text populationCountText;
    public TMP_Text traitAveragesText;
    public TMP_Text notificationText;
    public CanvasGroup notificationCanvasGroup;
    private Coroutine activeNotificationCoroutine;


    private List<GameObject> activePredators = new List<GameObject>();


    void Start()
    {

        SetupTraitPanelLayout();// CHANGED: Now doesnt spawn anything until play! 

    }

    public void Play()
    {
        if (startScreenPanel != null)
        {
            startScreenPanel.SetActive(false);
        }
        SpawnInitialPrey();
        gameStarted = true;
    }
    public void RestartSimulator()
    {
    clickSoundSource.PlayOneShot(clickSound);

    // Destroy every prey, predator, and food object currently in the scene
    foreach (GameObject prey in GameObject.FindGameObjectsWithTag("Prey"))
    {
        Destroy(prey);
    }
    foreach (GameObject predator in GameObject.FindGameObjectsWithTag("Predator"))
    {
        Destroy(predator);
    }
    foreach (GameObject food in GameObject.FindGameObjectsWithTag("Food"))
    {
        Destroy(food);
    }

    activePredators.Clear();

    // resets toggle states to original positions
    scarcityToggled = false;
    droughtToggled = false;
    if (foodSpawner != null)
    {
        foodSpawner.SetScarcity(false);
        foodSpawner.SetDrought(false);
    }
        overpopulationWarned = false;
        extinctionAnnounced = false;

        if (musicSource != null)
        {
            musicSource.Play();
        }

        SpawnInitialPrey();

        ShowNotification("Simulation Restarted");
}
    void Update()
    {
        UpdatePopulationUI();
        updateTraitAveragesUI();
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
    public void TogglePredators() // CHANGED: Toggle now is as many as you want; this way they die of starvation still but lead to overpopulation
    {
        clickSoundSource.PlayOneShot(clickSound);

        for (int i = 0; i < predatorCountWhenActive; i++)
        {
            Vector3 pos = RandomPointInField();
            GameObject predator = Instantiate(predatorPrefab, pos, Quaternion.identity);
            activePredators.Add(predator);
        }

        ShowNotification("Predators Added!");
    }

    // Toggle Button =  SCARCITY
    private bool scarcityToggled = false;
    public void ToggleScarcity()
    {
        clickSoundSource.PlayOneShot(clickSound);

        scarcityToggled = !scarcityToggled;
        if (foodSpawner != null)
        {
            foodSpawner.SetScarcity(scarcityToggled);
        }
        ShowNotification(scarcityToggled ? "Food Scarcity ON!" : "Food Scarcity OFF!");
    }

    // Toggle Button = DROUGHT
    private bool droughtToggled = false;
    public void ToggleDrought()
    {
        clickSoundSource.PlayOneShot(clickSound);

        droughtToggled = !droughtToggled;
        if (foodSpawner != null)
        {
            foodSpawner.SetDrought(droughtToggled);
        }
        ShowNotification(droughtToggled ? "Drought ON!" : "Drought OFF!");
    }

    // UI
    void UpdatePopulationUI()
    {
        if (populationCountText == null) return;

        int preyCount = GameObject.FindGameObjectsWithTag("Prey").Length;
        int predatorCount = GameObject.FindGameObjectsWithTag("Predator").Length;

        populationCountText.text = "Prey: " + preyCount + "   Predators: " + predatorCount;

        // overpopulation warning added for more realism
        if (predatorCount == 0 && preyCount >= overpopulationThreshhold && !overpopulationWarned)
        {
            ShowNotification("Overpopulated! Must Add Predators.");
            overpopulationWarned = true;
        }
        else if (preyCount < 150 && overpopulationWarned)
        {
            clickSoundSource.PlayOneShot(winSound);
            ShowNotification("Population Levels Normal");
            overpopulationWarned = false;
        }
    }

    // Adjusts trait readout's position and size so it doesn't overlap the header becuase it was doing it bad! 
    void SetupTraitPanelLayout()
    {
        if (traitAveragesText == null) return;

        RectTransform rect = traitAveragesText.GetComponent<RectTransform>();
        if (rect != null)
        {
            
            Vector2 pos = rect.anchoredPosition;
            pos.y -= 200f;
            rect.anchoredPosition = pos;
        }

        traitAveragesText.fontSize = 70f;
    }

    void updateTraitAveragesUI()
    {
        if (traitAveragesText == null) return;



        GameObject[] preyList = GameObject.FindGameObjectsWithTag("Prey");
        if (gameStarted && preyList.Length == 0)
        {
            traitAveragesText.text = "Population has gone Extinct Please Retry";

            if (!extinctionAnnounced)
            {
                clickSoundSource.PlayOneShot(gameOverSound);
                if (musicSource != null)
                {
                    musicSource.Stop();
                }
                extinctionAnnounced = true;
            }

            return;
        }
        else if (!gameStarted)
        {
            return; // nothing to show before the game has actually started
        }

        float totalSpeed = 0f;
        float totalVision = 0f;
        float totalFoodEfficiency = 0f;
        float totalSize = 0f;
        int highestGeneration = 0;
        int smallCount = 0;
        int mediumCount = 0;
        int largeCount = 0;

        foreach (GameObject preyObj in preyList)
        {
            Prey p = preyObj.GetComponent<Prey>();
            if (p == null) continue;

            totalSpeed += p.moveSpeed;
            totalVision += p.visionRange;
            totalFoodEfficiency += p.foodEfficiency;
            totalSize += p.specSize;

            // Sorts prey into a size category based on its specSize (range is 0.1 to 0.5)
            if (p.specSize < 0.23f)
            {
                smallCount++;
            }
            else if (p.specSize < 0.37f)
            {
                mediumCount++;
            }
            else
            {
                largeCount++;
            }

            if (p.generation > highestGeneration)
            {
                highestGeneration = p.generation;
            }

        }

        float avgSpeed = totalSpeed / preyList.Length;
        float avgVision = totalVision / preyList.Length;
        float avgFoodEfficiency = totalFoodEfficiency / preyList.Length;
        float avgSize = totalSize / preyList.Length;

        float speedPercent = Mathf.InverseLerp(0.1f, 2f, avgSpeed) * 100f;
        float visionPercent = Mathf.InverseLerp(0.1f, 2f, avgVision) * 100f;
        float foodPercent = Mathf.InverseLerp(0.1f, 2f, avgFoodEfficiency) * 100f;
        float sizePercent = Mathf.InverseLerp(0.1f, 0.5f, avgSize) * 100f;

        traitAveragesText.text =
        "Speed: " + speedPercent.ToString("F0") + "%\n\n" +
        "Vision: " + visionPercent.ToString("F0") + "%\n\n" +
        "Food Efficiency: " + foodPercent.ToString("F0") + "%\n\n" +
        "Generation: " + highestGeneration + "\n\n" +
        "Small: " + smallCount + "  Medium: " + mediumCount + "  Large: " + largeCount;
    }

    //show message for a few seconds and then fade...
    public void ShowNotification(string message)
    {
        if (notificationText == null || notificationCanvasGroup == null) return;

        notificationText.text = message;

        if (activeNotificationCoroutine != null)
        {
            StopCoroutine(activeNotificationCoroutine);
        }
        activeNotificationCoroutine = StartCoroutine(NotificationFadeRoutine());
    }

    private IEnumerator NotificationFadeRoutine()
    {
        notificationCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(1.5f);

        float fadeDuration = 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            notificationCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        notificationCanvasGroup.alpha = 0f;
    }
}
