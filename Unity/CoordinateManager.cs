using MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.OpenXR.Input;
using static Microsoft.MixedReality.GraphicsTools.MeshInstancer;
using MixedReality.Toolkit.Subsystems;


public class CoordinateManager : MonoBehaviour
{
    // Start is called before the first frame update
    public bool debug = true;
    public bool clearRSSIEntries = false;
    public GameObject cam;
    public GameObject anchorPrefab, relativeAnchorPrefab;
    public static Vector3 multilaterationStartPoint;
    public static bool sampling = false;

    public static string filePath;
    private Multilateration mul;

    public Vector3 handOffset = new Vector3(1.5f, -2, 1.5f);
    private void Awake()
    {
        filePath = Application.persistentDataPath + "/current_coord.txt";
        mul = FindObjectOfType<Multilateration>();
    }
    void Start()
    {
        multilaterationStartPoint = new Vector3();
        

    }

    // Update is called once per frame
    void Update()
    {

    }

    //Clears previous measurements and destroys all previous anchor points
    public void StartMultilateration()
    {
        Debug.Log("Starting Multilateration...");
        //Get rid of all sampled points to restart
        ARAnchor[] anchorScripts = FindObjectsOfType<ARAnchor>();
        foreach (ARAnchor anchorScript in anchorScripts)
        {
            Destroy(anchorScript.gameObject);
        }

        //initialize the starting point of multilateration
        multilaterationStartPoint = cam.transform.position;

        if (clearRSSIEntries)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false)) { writer.Write(""); }
        }

        sampling = true;
        CreateAnchor(true);

        
        mul.InstantiateCandidates(multilaterationStartPoint);
        //CreateNewSample(true);
    }



    public void StartStopSampling()
    {
        sampling = !sampling;
    }
    

    //Creates a new anchor point relative to the starting point of multilateration
    //Creates a new entry in the coordinate file.
    //Calls multilateration script 
    public void CreateNewSample(bool relative=false, bool dummyRSSI = false)
    {
        //init
        /*float theta;
        
        Debug.Log(360f - cam.transform.eulerAngles.y);
        theta = (360f - cam.transform.eulerAngles.y) * Mathf.Deg2Rad;
        Debug.Log("Cos tehta: " + Mathf.Cos(theta));
        int xt = 1, yt = 1;
        if (theta > 90f && theta <= 180f)
        {
            xt = -1;
        }
        if (theta > 180f && theta <= 270f)
        {
            xt = -1;
            yt = -1;
        }
        if (theta > 270f && theta < 360f)
        {
            yt = -1;
        }
        Debug.Log(theta);
        Vector3 handAngleRelativeOffset = new Vector3(handOffset.x * xt * Mathf.Pow( Mathf.Cos(theta), 2) + handOffset.z * yt * Mathf.Pow(Mathf.Sin(theta), 2)
            , handOffset.y,
            xt * handOffset.z * Mathf.Pow(Mathf.Cos(theta), 2) + handOffset.x * yt * Mathf.Pow( Mathf.Sin(theta), 2));
        Debug.Log(handAngleRelativeOffset);*/



        //Debug.Log("Current Position: " + currentPos);
        //Debug.Log("Reference Point: " + multilaterationStartPoint);
        //Debug.Log("Relative Position: " + relativePos);
        Vector3 rightWristPos = Vector3.zero;
        var aggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();
        if (aggregator.TryGetJoint(TrackedHandJoint.Wrist, UnityEngine.XR.XRNode.RightHand, out HandJointPose pose))
        {
            rightWristPos = pose.Position;
            Debug.Log("Right Wrist Pos:\t" + rightWristPos);

        }
        else
        {
            Debug.Log("Couldn't get wrist position");
        }

        Vector3 currentPos = cam.transform.position;
        Vector3 relativePos = -(multilaterationStartPoint - rightWristPos);// + handAngleRelativeOffset;
        try
        {
            //coordinate file processing
            string fileContents = File.ReadAllText(filePath);
            string[] entries = fileContents.Split('$');
            string rssi = ConnectBluetooth.HexStringToSignedByte(ConnectBluetooth.rssiValue).ToString();
            //Debug.Log("Received rssi: " + rssi);
            if (dummyRSSI)
            {
                rssi = Random.Range(-60, -81).ToString();
                Debug.Log("Created Dummy RSSI: " + rssi);
            }
            string newEntry = "\n{" + relativePos + ";" + rssi.Trim() + "}$\n";

            string final = "";
            for (int i = 0; i < entries.Length - 1; i++)
            {
                final += entries[i] + "$";
            }
            final += newEntry;

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(final);
            }
        }
        catch (IOException e)
        {
            Debug.LogError("Error reading or writing the file: " + e.Message);
        }

        
        mul.DisplayCandidatePoints();
        CreateAnchor(relative: relative, position: rightWristPos);
    }

    //Creates an anchor object in the worldspace
    public void CreateAnchor(bool relative=false, Vector3 position = new Vector3())
    {
        Debug.Log("Creating Anchor...");
        GameObject anchor;
        if (relative)
        {
            anchor = Instantiate(relativeAnchorPrefab, cam.transform.position, Quaternion.identity);
        }
        else
        {
            anchor = Instantiate(anchorPrefab, position, Quaternion.identity);
        }
        if (anchor.GetComponent<ARAnchor>() == null)
        {
            anchor.AddComponent<ARAnchor>();
        }
    }

    public void CreateDummySample()
    {
        CreateNewSample(dummyRSSI: true);
    }
}
