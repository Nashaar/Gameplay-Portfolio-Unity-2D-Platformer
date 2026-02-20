using UnityEngine;

public class SeekerSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject seekerPrefab;
    public EnemySeeker.SeekerType seekerType;

    [Header("Spanw Area")]
    public Vector2 spawnMin;
    public Vector2 spawnMax;

    [Header("Timer")]
    public float spawningWait;
    public float seekerAgroRange;
    public float spawnerPushForce;
    public float spawnerJumpForce;
    public float spawnerSeekerSpeed;
    public int maxSpawn;
    
    private int spawnCounter = 0;
    private float spawnTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnTimer = spawningWait;
    }

    // Update is called once per frame
    void Update()
    {
        SpawnSeeker();
    }

    private void SpawnSeeker()
    {
        if(spawnCounter >= maxSpawn)
        {
            return;
        }
        spawnTimer -= Time.deltaTime;
        if(spawnTimer <= 0)
        {
            spawnCounter++;

            float xSpawnCoordinates = Random.Range(spawnMin.x, spawnMax.x);
            float ySpawnCoordinates = Random.Range(spawnMin.y, spawnMax.y);
            Vector2 spawnPosition = new Vector2(xSpawnCoordinates, ySpawnCoordinates);
            spawnPosition += (Vector2)transform.position;

            GameObject seekerInstance = Instantiate(seekerPrefab, spawnPosition, Quaternion.identity);
            EnemySeeker enemySeeker = seekerInstance.GetComponent<EnemySeeker>();
            enemySeeker.SetSeekerType(seekerType);
            enemySeeker.aggroRange = seekerAgroRange;
            enemySeeker.jumpForce = spawnerJumpForce;
            enemySeeker.pushForce = spawnerPushForce;
            enemySeeker.speed = spawnerSeekerSpeed;

            spawnTimer = spawningWait;
        }
    }
}
