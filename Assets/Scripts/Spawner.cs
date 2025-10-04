using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefab;       
    public int count = 10;            
    public Vector3 spawnArea = new Vector3(10, 5, 10); // Size of spawn box
    public bool randomRotation = true;
    public bool randomScale = false;
    public Vector2 scaleRange = new Vector2(1f, 2f);
    
    public void Spawn()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                Random.Range(-spawnArea.y / 2, spawnArea.y / 2),
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );

            Quaternion rotation = randomRotation ? Random.rotation : Quaternion.identity;

            GameObject obj = Instantiate(prefab, randomPos, rotation);

            if (randomScale)
            {
                float scale = Random.Range(scaleRange.x, scaleRange.y);
                obj.transform.localScale = Vector3.one * scale;

                // Optional: if prefab has a "size" property
                var creature = obj.GetComponent<EatableEntity>();
                if (creature != null)
                {
                    creature.SetSize(scale); // sync visual scale and size
                }
            }
        }
    }

    // Optional: spawn automatically at start
    void Start()
    {
        Spawn();
    }
}
