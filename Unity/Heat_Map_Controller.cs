using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Heat_Map_Controller : MonoBehaviour
{
    public GameObject[] sensors;
    public float control_value = 100;
    public float control_deviance = 15f;
    Mesh mesh;
    Vector3[] vertices;
    Vector3[] sensor_positions;
    float[] sensorVals;
    // Start is called before the first frame update
    void Start()
    {

        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        //Debug.Log("Number of Vertices: " + vertices.Length);

        sensor_positions = new Vector3[sensors.Length];
        sensorVals = new float[sensors.Length];
        for (int i = 0; i < sensors.Length; i++) //get sensor positions
        {
            Vector3 pos = sensors[i].transform.position;

            

            //Debug.Log("Sensor " + i + " Position: " + pos);
            sensor_positions[i] = pos;
            sensorVals[i] = sensors[i].GetComponent<SensorController>().value;
        }
        
        Color[] colors = new Color[vertices.Length];
        for (int i = 0;i < vertices.Length;i++) //loop through all vertices
        {
            //Debug.Log(vertices[i]);
            Vector3 currentVertex = transform.TransformPoint(vertices[i]);
            //Debug.Log(currentVertex);
            float val = IDW(currentVertex, sensor_positions, sensorVals, power:3);
            //Debug.Log(val);

            colors[i] = GetColor(val);
        }
        SetVertexColors(colors);

    }

    // Update is called once per frame
    float counter = 0;
    float restartTime = 0.05f;
    void Update()
    {
        
        counter += Time.deltaTime;
        if(counter > restartTime)
        {
            for (int i = 0; i < sensors.Length; i++) //get sensor positions and values
            {
                Vector3 pos = sensors[i].transform.position;



                //Debug.Log("Sensor " + i + " Position: " + pos);
                sensor_positions[i] = pos;
                sensorVals[i] = sensors[i].GetComponent<SensorController>().value;
            }


            Color[] colors = new Color[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) //loop through all vertices
            {
                //Debug.Log(vertices[i]);
                Vector3 currentVertex = transform.TransformPoint(vertices[i]);
               // Debug.Log(currentVertex);
                float val = IDW(currentVertex, sensor_positions, sensorVals, power: 3);
               // Debug.Log(val);
                colors[i] = GetColor(val);
            }
            SetVertexColors(colors);
            counter = 0;
        }
        
    }


    void SetVertexColors(Color[] vertexColors)
    {
        // Ensure there is a MeshRenderer component
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer component not found.");
            return;
        }

        // Assign the colors to the existing mesh
        mesh.colors = vertexColors;
        //Debug.Log("Colors set");
    }

    public Gradient colorGradient;
    Color GetColor(float value)
    {
        //Debug.Log(("blah"));
        float max = control_value + control_deviance;
        float min = control_value - control_deviance;
        float diff = (value - min) / (max - min); //linear interpolation
        //Debug.Log("Color value for given value " + value + " : " + diff);
        if (diff > 1f)
        {
            diff = 1f;
        }
        if (diff < 0f)
        {
            diff = 0f;
        }
        if (float.IsNaN(diff))
        {
            diff = 0f;
        }
        Color c;
        try
        {
            //Debug.Log(diff);
            c = colorGradient.Evaluate(diff);
        }
        catch (Exception e)
        {
            Debug.LogError("Color error. Value: " + diff);
            c = Color.black;
        }
        return c;
    }

    float IDW(Vector3 vertexPos, Vector3[] sensorPositions, float[] sensorValues, float bounds = 100f, float power = 2)
    {
        float sumTop = 0;
        float sumBottom = 0;
        for (int i = 0; i < sensorPositions.Length; i++)
        {
            Vector3 diff = sensorPositions[i] - vertexPos;
            if (diff.magnitude < bounds) //if the sensor is within the bounds
            {
                sumTop += (float)(sensorValues[i] / (float)(Math.Pow(diff.magnitude, power)));
                sumBottom += (float)(1f / (Math.Pow(diff.magnitude, power)));
                //Debug.Log("sumTop: " + sumTop);
                //Debug.Log("sumBottom: " + sumBottom);
            }
        }
        if (sumBottom == 0)
            return 0;
        else
            return (float)sumTop / (float)sumBottom;
    }

    void Test_IDW(int numSensors, int numVertices)
    {
        System.Random rnd = new System.Random();

        Vector3[] sensorPos = new Vector3[numSensors];
        float[] sensorVals = new float[numSensors];
        for (int i = 0; i < numSensors; i++)
        {
            Vector3 pos = new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());

            sensorPos[i] = pos;
            sensorVals[i] = (float)rnd.NextDouble();
        }

        var watch = System.Diagnostics.Stopwatch.StartNew();

        for (int j = 0; j < numVertices; j++)
        {
            Vector3 vertexPos = new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
            float val = IDW(vertexPos, sensorPos, sensorVals);
        }

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Debug.Log("Elapsed time for " + numSensors + " sensors and " + numVertices + " vertices:\t" + elapsedMs);
    }

    
}



