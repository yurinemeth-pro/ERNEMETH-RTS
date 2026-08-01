using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform stimulus;
    public int spawnCount = 20;
    public float spawnRadius = 5f;

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = transform.position + (Vector3)randomOffset;

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            newEnemy.GetComponent<EnemyUnit>().stimulus = stimulus;
        }
    }
}