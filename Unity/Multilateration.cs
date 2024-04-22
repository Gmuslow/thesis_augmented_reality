using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Multilateration;

public class Multilateration : MonoBehaviour
{
    // Start is called before the first frame update
    public float maxDist = 5f;
    public int maxPoints = 11;
    public float minY = 0f;
    public float maxY = 2f;
    public static bool showRSSISphere = true;
    public GameObject RSSISphere;

    private List<Vector3> candidatePoints;
    private List<GameObject> candidateObjects;
    static List<GameObject> rssiSpheres;
    void Start()
    {
        rssiSpheres = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //toggle showing RSSI spheres
    public static void toggle()
    {
        
        showRSSISphere = !showRSSISphere;
        
        foreach (GameObject sp in rssiSpheres)
        {
            sp.SetActive(showRSSISphere);
        }

        Debug.Log("Showing RSSI spheres: " + showRSSISphere);
    }

    public class Candidate
    { 
        public Vector3 Position;
        public float Value;
        public Candidate(float v, Vector3 p)
        {
            Position = p;
            Value = v;
        }
    }

    //Algorithm to perform multilateration
    public List<Candidate> PerformMultilateration()
    {
        string coord_filename = CoordinateManager.filePath;
        List<Vector3> current_coordinates = new List<Vector3>();
        List<int> rssi_values = new List<int>();

        using (StreamReader stream = new StreamReader(coord_filename))
        {
            string[] lines = stream.ReadToEnd().Split('$');
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string entry = lines[i].Replace("{", "").Replace("}", "").Trim();
                int RSSI = int.Parse(entry.Split(";")[1]);
                string coord = entry.Split(";")[0].Replace("(", "").Replace(")", "");
                //Debug.Log("coord" + coord);
                float x = float.Parse(coord.Split(",")[0]);
                float y = float.Parse(coord.Split(",")[1]);
                float z = float.Parse(coord.Split(",")[2]);
                if (RSSI > -81)
                {
                    current_coordinates.Add(new Vector3(x, y, z));
                    rssi_values.Add(RSSI);
                }
                
                
            }
        }

        if (rssi_values.Count == 0)
        {
            return null;
        }

        List<float> radii = new List<float>();
        foreach(int  rssi in rssi_values)
        {
            radii.Add(RSSIToMeters(rssi));
        }

        ;
        List<Candidate> candidates = new List<Candidate>();
        foreach (Vector3 candidatePoint in candidatePoints)
        {
            float candidate_value = 0f;
            float combined_distance_score = 0f;
            for (int i = 0; i < current_coordinates.Count; i++)
            {
                Vector3 vector = new Vector3(candidatePoint.x - current_coordinates[i][0], candidatePoint.y - current_coordinates[i][1], candidatePoint.z - current_coordinates[i][2]);
                float mag = vector.magnitude;
                float distance = Mathf.Abs(mag - radii[i]);
                combined_distance_score += distance * PriorityFunction(rssi_values[i]);
            }
            candidate_value = 1.0f / combined_distance_score;
            candidates.Add(new Candidate(candidate_value, candidatePoint));
        }

        //handle RSSI spheres for debugging
        int index = current_coordinates.Count - 1;
        GameObject g = Instantiate(RSSISphere, current_coordinates[index] + CoordinateManager.multilaterationStartPoint, Quaternion.identity);
        Vector3 scale = new Vector3(2f * radii[index], 2f * radii[index], 2f * radii[index]);

        g.transform.localScale = scale;
        g.SetActive(showRSSISphere);
        rssiSpheres.Add(g);
            
        return candidates;
    }

    public float RSSIToMeters(int rssiValue)
    {
        float N = 4f;
        float measuredPower = -83f;
        float exp = (measuredPower - (float)rssiValue) / (10f * N);
        float radius = Mathf.Pow(10, exp);
        return radius;
    }

    public List<Vector3> CreateCandidatePoints()
    {
        List<Vector3> candidatePoints1 = new List<Vector3>();
        float inc = maxDist * 2f / maxPoints;
        for (int i = 0; i < maxPoints; i++)
        {
            for (int j = 0; j < maxPoints; j++)
            {
                for (int k = 0; k < maxPoints; k++)
                {
                    float yValue = j * inc - maxDist;

                    if (yValue > minY &&  yValue < maxY)
                        candidatePoints1.Add(new Vector3(i * inc - maxDist, yValue, k * inc - maxDist));
                }
            }
        }
        return candidatePoints1;
    }

    public float PriorityFunction(int rssi)
    {
        if (rssi < -81)
            return 999;
        else
            return 1;

        //return Mathf.Sqrt(Mathf.Abs(rssi) - 75);
    }


    public GameObject candidateObject;
    

    //performs multilateration based upon coordinate file and then displays candidate matrix
    public void DisplayCandidatePoints()
    {

        List<Candidate> candidates = PerformMultilateration();
        if (candidates == null)
        {
            return;
        }
        float max = 0f;
        foreach(Candidate candidate in candidates) //computing max real quick
        {
            if (candidate.Value > max)
            {
                max = candidate.Value;
            }
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            float alpha = candidates[i].Value / max;
            if (alpha < 0.4)
            {
                candidateObjects[i].SetActive(false);
            }
            else
            {
                candidateObjects[i].SetActive(true);
            }
            


            //Renderer rend = g.GetComponent<Renderer>();

            // Create a new material instance based on the object's material
            //Material material = new Material(rend.material);

            // Set the alpha value of the material's color
            //Color color = material.color;
            //new Color(1 - alpha, 1 - alpha, 1 - alpha);
            
            //color.a = alpha; //set the alpha to some function of the candidate value (TODO<------------)
            //material.color = color;


            // Assign the modified material to the object
            //rend.material = material;
            
        }
    }

    //should only be called on init or restart
    public void InstantiateCandidates(Vector3 relativeTo)
    {
        candidatePoints = CreateCandidatePoints();
        if (candidateObjects != null)
        {
            foreach (GameObject candidateController in candidateObjects)
            {
                Destroy(candidateController);
            }
        }
        candidateObjects = new List<GameObject>();
        foreach (Vector3 candidate in candidatePoints)
        {
            GameObject g = Instantiate(candidateObject, candidate + relativeTo, Quaternion.identity);
            g.SetActive(true);
            candidateObjects.Add(g);
        }
        
    }

}
