using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SensorCreator : MonoBehaviour
{
    public GameObject testingModel;
    public GameObject sensorPrefab;
    public float delay = 0.5f;
    private List<GameObject> sensors = new List<GameObject>();
    Heat_Map_Controller h;
    string[][] grid;
    int numSensors;

    //PrintRow(grid, 4);
    string[] headerRow; 
    void Awake()
    {
        grid = ReadCSVFile("scripts/sensor_values.csv");
        headerRow = GetRow(grid, 4);

        numSensors = 0;
        bool readingSensorIDs = true;
        List<int> sensorIDs = new List<int>();
        List<Vector3> sensorTransforms = new List<Vector3>();
        string[][] sensorPositionGrid = ReadCSVFile("scripts/sensor_positions.csv");
        string[] xPositions = GetRow(sensorPositionGrid, 1);
        string[] yPositions = GetRow(sensorPositionGrid, 2);
        string[] zPositions = GetRow(sensorPositionGrid, 3);
        string[] firstValues = GetRow(grid, 7);
        Vector3 modelPosition = testingModel.transform.position;
        h = testingModel.GetComponent<Heat_Map_Controller>();
        List<float> firstSensorValues = new List<float>();
        for (int i = 1; i < headerRow.Length; i++)
        {
            if (headerRow[i] != "Control Temperature" && readingSensorIDs)
            {
                numSensors++;
                try
                {
                    
                    sensorIDs.Add(Int32.Parse(headerRow[i]));

                    // Debug.Log(zPositions[i]);
                    Vector3 sensorPos = new Vector3(modelPosition.x + (float)Double.Parse(xPositions[i]), modelPosition.y + (float)Double.Parse(yPositions[i]), modelPosition.z + (float)Double.Parse(zPositions[i]));
                    sensorTransforms.Add(sensorPos);

                    //Debug.Log(firstValues[i]);
                    firstSensorValues.Add((float)Double.Parse(firstValues[i]));
                }
                catch (Exception e)
                {
                    Debug.LogError("Error: " + e.Message);
                    //Debug.Log("Got: " + headerRow[i]);
                }
            }
            
            else
            {
                readingSensorIDs = false;
            }

            if (headerRow[i] == "Part Temperature Setpoint")
            {

                h.control_value = (float)Double.Parse(firstValues[i]);
                //Debug.Log(temperature_setpoint);
            }
        }

        for (int i = 0; i < sensorIDs.Count; i++)
        {
            GameObject sensor = Instantiate(sensorPrefab, sensorTransforms[i], Quaternion.identity);
            sensor.GetComponent<SensorController>().ID = sensorIDs[i];
            sensor.GetComponent<SensorController>().value = firstSensorValues[i];
            sensors.Add(sensor);
        }
        h.sensors = sensors.ToArray();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    float counter = 0f;
    int rowCounter = 8;
    void Update()
    {
        
        bool readingSensorIDs = true;
        List<float> sensorValues = new List<float>();
        string[] rowVals = GetRow(grid, rowCounter);
        counter += Time.deltaTime;
        if (counter > delay)
        {
            Debug.Log("Showing values for row " + rowCounter + " of excel sheet.");
            counter = 0f;
            try
            {
                for (int i = 1; i < headerRow.Length; i++)
                {
                    if (headerRow[i] != "Control Temperature" && readingSensorIDs)
                    {
                        try
                        {

                            sensorValues.Add((float)Double.Parse(rowVals[i]));
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Error: " + e.Message);
                            //Debug.Log("Got: " + headerRow[i]);
                        }
                    }

                    else
                    {
                        readingSensorIDs = false;
                    }

                    if (headerRow[i] == "Part Temperature Setpoint")
                    {

                        h.control_value = (float)Double.Parse(rowVals[i]);
                        //Debug.Log(temperature_setpoint);
                    }
                }

                for (int i = 0; i < numSensors; i++)
                {
                    sensors[i].GetComponent<SensorController>().value = sensorValues[i];
                }
                    rowCounter += 1;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    string[][] ReadCSVFile(string filename)
    {
        string filePath = Path.Combine(Application.dataPath, filename);

        if (File.Exists(filePath))
        {
            try
            {
                List<string[]> csvData = new List<string[]>();

                using (StreamReader sr = new StreamReader(filePath))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string[] values = line.Split(',');

                        csvData.Add(values);
                    }
                }

                return csvData.ToArray();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error reading the CSV file: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("CSV file not found at path: " + filePath);
        }

        return null;
    }

    string[] GetRow(string[][] grid, int row) {
        List<string> rowValues = new List<string>();
        for (int i = 0; i < grid.Length; i++)
        {
            if (i == row)
            {
                for (int j = 0; j < grid[i].Length; j++)
                {
                    //Debug.Log(grid[i][j]);
                    rowValues.Add(grid[i][j]);
                }
                break;
            }
        }
        return rowValues.ToArray();
    }
}
