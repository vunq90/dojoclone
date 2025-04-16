// EnemySpawner.cs
// Spawns 4 enemies at a time around the player between a ring (inner/outer radius)

using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform player;
    
    private float innerRadius = 2f;
    private float outerRadius = 4f;
    private float spawnInterval = 2f;
    
    public int maxEnemies = 20;
    public int enemiesPerSpawn = 4;

    private float timer = 0f;

    void Update()
    {
        if (player == null || enemyPrefab == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval && CountEnemies() < maxEnemies)
        {
            for (int i = 0; i < enemiesPerSpawn; i++)
            {
                if (CountEnemies() >= maxEnemies) break;
                SpawnEnemy();
            }
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(innerRadius, outerRadius);
        Vector3 spawnPos = player.position + new Vector3(offset.x, offset.y, 0f);
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    int CountEnemies()
    {
        return GameObject.FindObjectsOfType<Enemy>().Length;
    }
}
