using UnityEngine;

public class RandomPropSpawn : MonoBehaviour
{

    public float spawnChance = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float randomVar = Random.value;

        if (randomVar > spawnChance)
        {
            this.gameObject.SetActive(false);
            
        }
    }


}
