using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieSpawnController : MonoBehaviour
{
    public int initialZombiesPerWave = 5;
    public int currentZombiesPerWave;

    public float spawnDelay = 0.5f;

    public int currentWave = 0;
    public float waveCooldown = 10.0f;

    public bool inCooldown;
    public float cooldownCounter = 0;

    public List<Zombie> currentZombiesAlive = new List<Zombie>();

    public GameObject zombiePrefab;

    private void Start()
    {
        currentZombiesPerWave = initialZombiesPerWave;
        StartNextWave();
    }

    private void StartNextWave()
    {
        currentZombiesAlive.Clear();
        currentWave++;
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < currentZombiesPerWave; i++)
        {
            Vector3 spawnOffset = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            Vector3 spawnPosition = transform.position + spawnOffset;

            var zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);

            Zombie enemyScript = zombie.GetComponent<Zombie>();

            if (enemyScript != null)
            {
                currentZombiesAlive.Add(enemyScript);
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void Update()
    {
        List<Zombie> zombiesToRemove = new List<Zombie>();
        foreach (Zombie zombie in currentZombiesAlive)
        {
            if (zombie != null && zombie.isDead)
            {
                zombiesToRemove.Add(zombie);
            }
        }

        foreach (Zombie zombie in zombiesToRemove)
        {
            currentZombiesAlive.Remove(zombie);
        }

        zombiesToRemove.Clear();

        if (currentZombiesAlive.Count == 0 && !inCooldown)
        {
            StartCoroutine(WaveCooldown());
        }

        if (inCooldown)
        {
            cooldownCounter -= Time.deltaTime;
        }
        else
        {
            cooldownCounter = waveCooldown;
        }
    }

    private IEnumerator WaveCooldown()
    {
        inCooldown = true;
        yield return new WaitForSeconds(waveCooldown);
        inCooldown = false;
        currentZombiesPerWave *= 2;
        StartNextWave();
    }
}
