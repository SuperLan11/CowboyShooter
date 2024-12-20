using UnityEngine;

public class EnemyScaler : MonoBehaviour
{
    void Start()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;
            enemyTransform.localScale = enemyTransform.localScale * 1.5f;
        }

        //Debug.Log($"Scaled {enemies.Length} enemies by 50%.");
    }
}
