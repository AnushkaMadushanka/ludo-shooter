using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner instance;

    public List<Enemy> enemies = new List<Enemy>();
    public int currWave;
    private int waveValue;
    public List<GameObject> enemiesToSpawn = new List<GameObject>();

    public Transform[] spawnLocations;
    public float spawnInterval;
    private float spawnTimer;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (spawnTimer <= 0)
        {
            //spawn an enemy
            if (enemiesToSpawn.Count > 0)
            {
                Instantiate(enemiesToSpawn[0], spawnLocations[Random.Range(0, spawnLocations.Length)].position, Quaternion.identity); // spawn first enemy in our list
                enemiesToSpawn.RemoveAt(0); // and remove it
                spawnTimer = spawnInterval;
            }
            
        }
        else
        {
            spawnTimer -= Time.fixedDeltaTime;
        }
    }

    public int GenerateWave()
    {
        waveValue = currWave * 10;
        return GenerateEnemies();
    }

    public int GenerateEnemies()
    {
        // Create a temporary list of enemies to generate
        // 
        // in a loop grab a random enemy 
        // see if we can afford it
        // if we can, add it to our list, and deduct the cost.

        // repeat... 

        //  -> if we have no points left, leave the loop

        List<GameObject> generatedEnemies = new List<GameObject>();
        while (waveValue > 0)
        {
            var available = enemies.FindAll(enemy => enemy.cost < waveValue);
            if (available.Count == 0) break;
            int randEnemyId = Random.Range(0, available.Count);
            int randEnemyCost = enemies[randEnemyId].cost;

            if (waveValue - randEnemyCost >= 0)
            {
                generatedEnemies.Add(enemies[randEnemyId].enemyPrefab);
                waveValue -= randEnemyCost;
            }
            else if (waveValue <= 0)
            {
                break;
            }
        }
        enemiesToSpawn.Clear();
        enemiesToSpawn = generatedEnemies;
        return generatedEnemies.Count;
    }

    private void OnDestroy()
    {
        var cWave = currWave - 1;
        var hWave = PlayerPrefs.GetInt("highestWave");
        PlayerPrefs.SetInt("currentWave", cWave);
        PlayerPrefs.SetInt("highestWave", cWave > hWave ? cWave : hWave);
    }

}

[System.Serializable]
public class Enemy
{
    public GameObject enemyPrefab;
    public int cost;
}
