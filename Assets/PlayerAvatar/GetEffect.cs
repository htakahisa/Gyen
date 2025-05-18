using UnityEngine;
using UnityEngine.Events;

public class GetEffect : MonoBehaviour
{

    public UnityEvent effect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Active()
    {
        effect?.Invoke();
    }

    public void DebugCall()
    {
        Debug.Log("It called");
    }

}
