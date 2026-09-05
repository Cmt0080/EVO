using UnityEngine;


public class Prey : MonoBehaviour
{
   
    public float moveSpeed;       // how fast this pixel moves
    public float visionRange;     // how far it can detect predators called by Predator.cs
    public float foodEfficiency;// how much value is intaken by food eaten; future problem
    public float specSize; // basic tradeoff trait, bigger = more food

   
    private enum State { Seeking, MovingToFood, Eating, Reproducing }
    private State currentState = State.Seeking;

   
    private float hunger = 1f;           // 1 = full, 0 = starving. Ticks down over time.
    private float hungerDrainRate = 0.002f; // how fast hunger drops per second
    private float reproduceThreshold = 1.1f; // hunger value needed to trigger duplication
    private Transform targetFood;        // the food object this pixel is currently walking to
    private Vector2 wanderDirection;     // current random direction moving when no food is targeted
    private float wanderTimer = 0f;

    void Start()
    {
        // If traits weren't inherited
        // give it randomized starting traits.
        if (moveSpeed <= 0f)
        {
            moveSpeed = Random.Range(0.5f, 1.5f);
            visionRange = Random.Range(0.5f, 1.5f);
            foodEfficiency = Random.Range(0.5f, 1.5f);
            specSize = Random.Range(0.1f, 0.5f);
        }

        // Visual size change QOL
        float normalizedSize = Mathf.InverseLerp(0.1f, 0.5f, specSize);
        float visualScaleFactor = Mathf.Lerp(0.7f, 1.4f, normalizedSize);
        transform.localScale = baseScale * visualScaleFactor;

        PickNewWanderDirection();
    }

    void Update()
    {
        // Hunger always drains! If it hits zero, pixel dies.
        hunger -= hungerDrainRate * Time.deltaTime;
        if (hunger <= 0f)
        {
            Die();
            return;
        }

        switch (currentState)
        {
            case State.Seeking:
                SeekingBehavior();
                break;
            case State.MovingToFood:
                MoveTowardFood();
                break;
            case State.Eating:
                // Eating is instant in this version; could add animation in future
                currentState = State.Seeking;
                break;
            case State.Reproducing:
                Reproduce();
                break;
        }

        // Check if pixel is well-fed and can duplicated; FAIL SAFE
        if (hunger >= reproduceThreshold)
        {
            currentState = State.Reproducing;
        }
    }

    //Seeking State
    // No food targeted yet: wander randomly, but keep checking for nearby food when possible
    void SeekingBehavior()
    {
        Transform nearestFood = FindNearestFood();
        if (nearestFood != null)
        {
            targetFood = nearestFood;
            currentState = State.MovingToFood;
            return;
        }

        // Random wander move one direction then pick new and change to that direction
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            PickNewWanderDirection();
        }

        transform.position += (Vector3)(wanderDirection * moveSpeed * Time.deltaTime);
    }

    void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        wanderTimer = Random.Range(1f, 3f); // wander in this direction for 1-3 seconds
    }

    // 
    void MoveTowardFood()
    {
        if (targetFood == null)
        {
            // food eaten by another pixel  return to seeking state. 
            currentState = State.Seeking;
            return;
        }

        Vector3 direction = (targetFood.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(transform.position, targetFood.position);
        if (distance < 0.2f)
        {
            // Reached the food — eat it.
            hunger += 0.5f * foodEfficiency; // sets food efficiency higher; pixels gain more from feeding
            Destroy(targetFood.gameObject);
            targetFood = null;
            currentState = State.Eating;
        }
    }

    // FIND FOOD 
    Transform FindNearestFood()
    {
        GameObject[] foodItems = GameObject.FindGameObjectsWithTag("Food");
        Transform nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (GameObject food in foodItems)
        {
            float dist = Vector3.Distance(transform.position, food.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = food.transform;
            }
        }

        float detectionRadius = visionRange * 3f;
        if(nearestDist > detectionRadius)
        {
            return null;
        }
        
        return nearest;
    }

    //  Duplicate through reproduction
    void Reproduce()
    {
        hunger -= 1f; // reproducing costs hunger, this to prevent a million freaking pixels and crashes

        Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
        Vector3 spawnPosition = transform.position + (Vector3)randomOffset; // added to prevent clumping of species

        GameObject offspring = Instantiate(gameObject, spawnPosition,Quaternion.identity);
        Prey offspringScript = offspring.GetComponent<Prey>();

        // Offspring traits = parent traits + small random mutation(mSpeed,vRange, and fEfficiency)
        offspringScript.moveSpeed = Mutate(moveSpeed);
        offspringScript.visionRange = Mutate(visionRange);
        offspringScript.foodEfficiency = Mutate(foodEfficiency);

        offspringScript.specSize = Mutate(specSize, 0.1f,0.5f);
        offspringScript.generation = generation + 1;

        currentState = State.Seeking;
    }

    float Mutate(float traitValue, float minClamp =0.1f, float maxClamp = 2f)
    {
        float mutationAmount = Random.Range(-0.1f, 0.1f);
        return Mathf.Clamp(traitValue + mutationAmount, minClamp, maxClamp);

    }

    //Generation tracking
    public int generation = 0; // inital population starts at = 0 


    private Vector3 baseScale = new Vector3(0.2f, 0.2f, 1f); // CHANGED: FIXED ORIGNAL SIZE 

    private float effectiveHungerDrainRate; //adjusted for pixel size


    //DEATH STATE
    void Die()
    {
        Destroy(gameObject);
    }

    // Called by Predator.cs when a catch attempt succeeds.
    public void CaughtByPredator()
    {
        Die();
    }
}
