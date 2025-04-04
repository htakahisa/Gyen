using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public static MaterialManager mm;

    // Start is called before the first frame update
    void Awake()
    {
        mm = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Material GetMaterial(int value)
    {
        return GetComponent<MeshRenderer>().materials[value];
    }

}
