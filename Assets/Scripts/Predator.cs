using UnityEngine;


public class Predator : MonoBehaviour
{
    
    public float moveSpeed = 1f;
    public float huntRadius = 3f;         // predator detection radius area
    public float catchDistance = 0.2f;    // how close predator needs to catch prey

    public float minCatchChance = 0.15f;  // worst case = 15% 
    public float maxCatchChance = 0.90f;  // best case = 90% (not a gauranteed catch)

    public float starveTime = 8f;         // seconds before predator starves
    public float eatCooldown = 4f; // CHANGED: seconds predator cant hunt; add this becuase they were eating everything... 


    
    private Transform targetPrey;
    private float timeSinceLastKill = 0f;
    private float cooldownTimer = 0f;
    private Vector2 wanderDirection;
    private float wanderTimer = 0f;

    void Start()
    {
        PickNewWanderDirection();
    }

    void Update()
    {
        // Starvation check 
        timeSinceLastKill += Time.deltaTime;
        if (timeSinceLastKill >= starveTime)
        {
            Die();
            return;
        }
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            Wander();
            return;
        }
        // Looks for prey if target is not present
        if (targetPrey == null)
        {
            targetPrey = FindNearestPreyInRange();
        }

        if (targetPrey != null)
        {
            ChaseTarget();
        }
        else
        {
            Wander();
        }
    }

    // Find Prey
    Transform FindNearestPreyInRange()
    {
        GameObject[] preyList = GameObject.FindGameObjectsWithTag("Prey");
        Transform nearest = null;
        float nearestDist = huntRadius;

        foreach (GameObject prey in preyList)
        {
            float dist = Vector3.Distance(transform.position, prey.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = prey.transform;
            }
        }
        return nearest;
    }

    // Chase Mechanics 
    void ChaseTarget()
    {
        if (targetPrey == null) return;

        Vector3 direction = (targetPrey.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(transform.position, targetPrey.position);
        if (distance <= catchDistance)
        {
            AttemptCatch();
        }
    }

    // Catch Chance
    void AttemptCatch()
    {
        if (targetPrey == null) return;

        Prey preyScript = targetPrey.GetComponent<Prey>();
        if (preyScript == null)
        {
            targetPrey = null;
            return;
        }

        //CHANGED: big and small pixels now have same chance of capture and then feeds into lerp of 15%-90%
        float normalizedVision = Mathf.InverseLerp(0.1f, 2f, preyScript.visionRange);
        float normalizedSize = Mathf.InverseLerp(0.1f,0.5f, preyScript.specSize);
        float defenseRating = (normalizedVision + normalizedSize) / 2f;
        float catchChance = Mathf.Lerp(maxCatchChance,minCatchChance,defenseRating);
        

        float roll = Random.Range(0f, 1f);

        if (roll <= catchChance)
        {
            // Successful catch.
            preyScript.CaughtByPredator();
            timeSinceLastKill = 0f;
            cooldownTimer = eatCooldown;
        }
        // If the catch fails, prey just lives — no penalty beyond a close call.
        // (No "predator dies on failed attempt" rule anymore — starvation is time-based now.)

        targetPrey = null; // release target either way, re-evaluate next frame
    }

    // WANDER (no prey in range) 
    void Wander()
    {
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
        wanderTimer = Random.Range(1f, 3f);
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
