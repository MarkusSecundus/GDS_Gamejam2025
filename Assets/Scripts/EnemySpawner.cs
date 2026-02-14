using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Randomness;
using NUnit.Framework;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [System.Serializable]
    public struct SpawnCommand
    {
        public GameObject Enemy;
        public float Delay_seconds;
    }

    [SerializeField] SpawnCommand[] spawns;
    [SerializeField] Vector3 randomizeRange = Vector3.zero;
    [SerializeField] int MaxSpawnedObjectsCount = 6;
	[SerializeField] float countCheckInterval_seconds = 1.0f;
    [SerializeField] bool IsPerpetual = true;

	int idx = 0;

    List<GameObject> currentlyAlive = new();

    int _getCurrentlyAliveCount()
    {
        for(int i = currentlyAlive.Count; i-- > 0;)
        {
            if (! currentlyAlive[i]) currentlyAlive.RemoveAt(i);
        }
        return currentlyAlive.Count;
    }

    IEnumerator Start()
    {
        if (!PhotonNetwork.IsMasterClient) yield break;
        if (spawns.Length <= 0) yield break;

        while (true)
        {
            var current = spawns[idx];
			yield return new WaitForSeconds(current.Delay_seconds);

			while (_getCurrentlyAliveCount() >= MaxSpawnedObjectsCount)
            {
                yield return new WaitForSeconds(countCheckInterval_seconds);
            }

            Vector3 spawnPosition = transform.position;

			var newEnemy = PhotonNetwork.Instantiate(current.Enemy.name, spawnPosition, transform.rotation, 0, new object[] {RandomHelpers.Rand.Next()});
            currentlyAlive.Add(newEnemy);

			++idx;
			if (idx >= spawns.Length)
			{
				if (IsPerpetual) idx = idx.Mod(spawns.Length);
				else yield break;
			}
		}
    }

}
