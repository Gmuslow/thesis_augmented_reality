using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Multilateration;

public class RSSISphere : MonoBehaviour
{
    // Start is called before the first frame update
    public float opacity = 0.3f;
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();

        // Create a new material instance based on the object's material
        Material material = new Material(rend.material);

        // Set the alpha value of the material's color
        Color color = material.color;
        color.a = opacity;
        material.color = color;

        // Assign the modified material to the object
        rend.material = material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
