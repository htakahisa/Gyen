using UnityEngine;

public class BotManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetPos()
    {
        transform.position = new Vector3(Random.Range(-8.53f, 2.77f), 0.01000023f, Random.Range(1.20f, 3.27f));
    }
}
