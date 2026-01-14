using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    [HideInInspector]
    public SceneController controller;

    private StreamWriter streamWriter1;
    private StreamWriter streamWriter2;
    private StreamWriter customStreamWriter;
    private string fileName1;
    private string fileName2;
    private string customName;
    private string directory = "Data";
    private string directoryPath;
    private List<string> buffer1;
    private List<string> buffer2;
    private List<string> customBuffer;
    private float writeInterval = 1.0f;
    private float timeSinceLastWrite = 0f;
    private bool collectData = false;
    private Transform mainCamera;

    // Setup file vars
    [HideInInspector]
    public int currentStudyID = 0;
    private int run = 0;
    private string age;
    private string gender;
    private string experiencedInVR;
    private string visualImpairments;
    private int seed;
    //private Combination combination;
    //private string setupLogLatest;
    private string easeToSee;
    private string immersionAffected;
    private string currentEnv = "StartupScene";
    private string currentCatcher = "None";
    private Transform searchLocation;
    bool objectFoundSuccessfully;
    private long frameCounterOnStart = -1;
    private long frameCounterOnEnd = -1;
    private long timeOnStart = -1;
    private long timeOnEnd = -1;
    private int attempts;

    // Data file vars
    //private GazeIndex gazeIndex = GazeIndex.COMBINE;
    private Stopwatch stopwatch;
    [HideInInspector]
    public Transform eyeTrackingTransform;
    private long frameCounter = -1;



    /* Setup file functions */

    public void SetStudyStart(string age, string gender, string experiencedInVR, string visualImpairments)
    {
        this.age = age;
        this.gender = gender;
        this.experiencedInVR = experiencedInVR;
        this.visualImpairments = visualImpairments;
    }

    public void SetEnvAndCatcher(string currentEnv, string currentCatcher, Transform searchLocation)
    {
        this.currentEnv = currentEnv;
        this.currentCatcher = currentCatcher;
        this.searchLocation = searchLocation;
    }

    public void SetSetupData(List<string> answers, bool objectFoundSuccessfully, int attempts)
    {
        string dataToWrite;
        this.objectFoundSuccessfully = objectFoundSuccessfully;
        this.attempts = attempts;
        float timeDif = timeOnEnd - timeOnStart;
        string r_answers = string.Join(";", answers);
        dataToWrite = $"{currentStudyID};{run};{seed};{r_answers};{this.currentEnv};{controller.environmentPosition.position};{controller.environmentPosition.forward};{this.currentCatcher};{this.searchLocation.position};{this.objectFoundSuccessfully};{this.attempts};{frameCounterOnStart};{frameCounterOnEnd};{timeOnStart};{timeOnEnd};{timeDif};";
        WriteToBufferSetup(dataToWrite);
        WriteBufferToCSVSetup();
        frameCounterOnStart = -1;
        frameCounterOnEnd = -1;
        timeOnStart = -1;
        timeOnEnd = -1;
        run += 1;
        controller.ResumeScene();
    }


    /* Data file functions */

    void LateUpdate()
    {
        if (!collectData) return;
        timeSinceLastWrite += Time.deltaTime;

        if (timeSinceLastWrite >= writeInterval && (buffer1.Count > 0 || buffer2.Count > 0))
        {
            WriteBufferToCSVData();
            timeSinceLastWrite = 0f;
        }

        CollectData();
    }

    private void CollectData()
    {
        //XR_HTC_eye_tracker.Interop.GetEyePupilData(out XrSingleEyePupilDataHTC[] out_pupils);
        Vector3 gazeOrigin = controller.currentEyeTrackingScript.gazeRay.origin;
        Vector3 gazeDirection = controller.currentEyeTrackingScript.gazeRay.direction;
        //XrSingleEyePupilDataHTC leftPupil = out_pupils[(int)XrEyePositionHTC.XR_EYE_POSITION_LEFT_HTC];
        //XrSingleEyePupilDataHTC rightPupil = out_pupils[(int)XrEyePositionHTC.XR_EYE_POSITION_RIGHT_HTC];
    //    float leftEyeOpenness = leftPupil.pupilDiameter; // eye openness (0 closed, 1 open)
    //    float rightEyeOpenness = verboseData.right.eye_openness;
    //    float leftEyePupilDiameter = -1;
    //    float rightEyePupilDiameter = -1;

    //    if (verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
    //    {
    //        rightEyePupilDiameter = verboseData.right.pupil_diameter_mm;
    //        rightEyePupilRelativeToLens = verboseData.right.pupil_position_in_sensor_area;

    //    }
    //    Vector2 leftEyeGazeOrigin = new Vector2(0.0f, 0.0f); // millimeter position of cornea center relative to each lens center (~ 30,-1,-25/-30,1,-25)
    //    Vector2 rightEyeGazeOrigin = new Vector2(0.0f, 0.0f);
    //    Vector3 leftEyeGazeDirection = new Vector2(0.0f, 0.0f); // gaze direction normal vector
    //    Vector3 rightEyeGazeDirection = new Vector2(0.0f, 0.0f);
    //    if (verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) &&
    //verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
    //    {
    //        leftEyeGazeDirection = verboseData.left.gaze_direction_normalized;
    //        leftEyeGazeOrigin = verboseData.left.gaze_origin_mm;
    //    }
    //    if (verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) &&
    //verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
    //    {
    //        rightEyeGazeOrigin = verboseData.right.gaze_origin_mm;
    //        rightEyeGazeDirection = verboseData.right.gaze_direction_normalized;
    //    }

        string timestampTotal = System.DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
        long elapsedTime = stopwatch.ElapsedMilliseconds;
        float angleToSearchObject = GetAngleToSearchObject(gazeOrigin, gazeDirection);
        string frame = frameCounter++.ToString();
        //string frameData = $"{currentStudyID};{run};{currentEnv};{currentCatcher};{timestampTotal};{frame};{elapsedTime};{controller.currentEyeTrackingScript.lookingAtSearchObject};{controller.environmentPosition.position};{controller.environmentPosition.forward};{mainCamera.position};{mainCamera.forward};{mainCamera.up};{eyeTrackingTransform.position};{searchLocation.position};{angleToSearchObject};{leftEyeOpenness};{rightEyeOpenness};{leftEyePupilDiameter};{rightEyePupilDiameter};{"todo"};{"todo"};{leftEyeGazeOrigin};{rightEyeGazeOrigin};{leftEyeGazeDirection};{rightEyeGazeDirection};{gazeOrigin};{gazeDirection};";
        string frameData = $"{currentStudyID};{run};{currentEnv};{currentCatcher};{timestampTotal};{frame};{elapsedTime};{controller.currentEyeTrackingScript.lookingAtSearchObject};{controller.environmentPosition.position};{controller.environmentPosition.forward};{mainCamera.position};{mainCamera.forward};{mainCamera.up};{eyeTrackingTransform.position};{searchLocation.position};{angleToSearchObject};{gazeOrigin};{gazeDirection};";
        //string frameData = $"{Time.time},{Time.frameCount},5";
        WriteToBufferData(frameData);
    }

    private float GetAngleToSearchObject(Vector3 gazeOrigin, Vector3 gazeDirection)
    {
        Vector3 toObject = (searchLocation.position - gazeOrigin).normalized;
        float dotProduct = Vector3.Dot(gazeDirection.normalized, toObject);
        return Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
    }


    /* General functions */

    public void SetDataCollectingState(bool active)
    {
        if (active)
        {
            frameCounterOnStart = frameCounter;
            timeOnStart = stopwatch.ElapsedMilliseconds;
        }
        else
        {
            frameCounterOnEnd = frameCounter;
            timeOnEnd = stopwatch.ElapsedMilliseconds;
        }
        collectData = active;
    }

    public void SetupCustomCSV(string customFileName, List<string> questions)
    {
        // Setup first CSV file
        customName = GetFileNameBasedOnID(customFileName);
        customStreamWriter = new StreamWriter(customName, false, System.Text.Encoding.UTF8);
        customBuffer = new List<string>();
        string r_questions = string.Join(";", questions);
        WriteToBufferCustom($"StudyID;{r_questions}");
        WriteBufferToCustomCSV();
    }

    public void CollectCustomData(List<string> answers)
    {
        string r_answers = string.Join(";", answers);
        WriteToBufferCustom($"{currentStudyID};{r_answers}");
        frameCounterOnStart = -1;
        frameCounterOnEnd = -1;
        timeOnStart = -1;
        timeOnEnd = -1;
        controller.ResumeScene();
    }

    public void Setup(string baseFileName1, string baseFileName2, List<string> questions, int seed)
    {
        mainCamera = controller.GetCamera();
        stopwatch = new Stopwatch();
        stopwatch.Start();

        this.seed = seed;

        //directoryPath = Path.Combine(Application.dataPath, directory);
        directoryPath = Path.Combine(Application.persistentDataPath, directory);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Setup first CSV file
        fileName1 = GetUniqueFileName(baseFileName1);
        streamWriter1 = new StreamWriter(fileName1, false, System.Text.Encoding.UTF8);
        buffer1 = new List<string>();
        string r_questions = string.Join(";", questions);
        WriteToBufferSetup($"StudyID;run;seed;{r_questions};currentEnv;envPosition;envRotation;currentCatcher;searchObjectPosition;objectFound;wrongAttempts;firstFrame;lastFrame;firstFrameTime;lastFrameTime;totalRunTime;");
        WriteBufferToCSVSetup();

        // Setup second CSV file
        fileName2 = GetUniqueFileName(baseFileName2);
        streamWriter2 = new StreamWriter(fileName2, false, System.Text.Encoding.UTF8);
        buffer2 = new List<string>();
        //WriteToBufferData("StudyID;run;currentEnv;catcherID;time;frame;elapsedTime;lookingAtSearchObject;cameraEnvCenterPos;cameraEnvCenterForward;cameraPos;cameraForward;cameraUp;posEyeTracking;searchObjectPos;angleToSearchObject;leftEyeOpenness;rightEyeOpenness;leftEyePupilDiameter;rightEyePupilDiameter;leftEyePupilRelativeToLens;rightEyePupilRelativeToLens;leftEyeGazeOrigin;rightEyeGazeOrigin;leftEyeGazeDirection;rightEyeGazeDirection;gazeOrigin;gazeDirection;");
        WriteToBufferData("StudyID;run;currentEnv;catcherID;time;frame;elapsedTime;lookingAtSearchObject;cameraEnvCenterPos;cameraEnvCenterForward;cameraPos;cameraForward;cameraUp;posEyeTracking;searchObjectPos;angleToSearchObject;gazeOrigin;gazeDirection;");
        WriteBufferToCSVData();

        UnityEngine.Debug.Log("Data collection setup complete.");
    }


    public void WriteToBufferSetup(string data)
    {
        buffer1?.Add(data);
    }

    public void WriteToBufferData(string data)
    {
        buffer2?.Add(data);
    }

    public void WriteToBufferCustom(string data)
    {
        customBuffer?.Add(data);
    }

    private void WriteBufferToCSVSetup()
    {
        if (streamWriter1 != null)
        {
            foreach (var line in buffer1)
            {
                streamWriter1.WriteLine(line);
            }
            streamWriter1.Flush();
            buffer1.Clear();
        }
    }

    private void WriteBufferToCSVData()
    {
        if (streamWriter2 != null)
        {
            foreach (var line in buffer2)
            {
                streamWriter2.WriteLine(line);
            }
            streamWriter2.Flush();
            buffer2.Clear();
        }
    }

    private void WriteBufferToCustomCSV()
    {
        if (customStreamWriter != null)
        {
            foreach (var line in customBuffer)
            {
                customStreamWriter.WriteLine(line);
            }
            customStreamWriter.Flush();
            customBuffer.Clear();
        }
    }
    
    private string GetUniqueFileName(string baseFileName)
    {
        string fullPath = Path.Combine(directoryPath, baseFileName + ".csv");
        int fileCount = 1;

        while (File.Exists(fullPath))
        {
            fullPath = Path.Combine(directoryPath, $"{baseFileName}_{fileCount}.csv");
            currentStudyID = fileCount;
            fileCount++;
        }

        return fullPath;
    }

    private string GetFileNameBasedOnID(string baseFileName)
    {
        string fullPath = Path.Combine(directoryPath, baseFileName + ".csv");
        if(currentStudyID!= 0)
        {
            fullPath = Path.Combine(directoryPath, $"{baseFileName}_{currentStudyID}.csv");
        }
        return fullPath;
    }

    private void EndDataCollection()
    {
        if (buffer1 != null && buffer1.Count > 0)
        {
            WriteBufferToCSVSetup();
        }
        streamWriter1?.Close();

        if (buffer2 != null && buffer2.Count > 0)
        {
            WriteBufferToCSVData();
        }
        streamWriter2?.Close();

        if (customBuffer != null && customBuffer.Count > 0)
        {
            WriteBufferToCustomCSV();
        }
        customStreamWriter?.Close();
    }

    void OnApplicationQuit()
    {
        EndDataCollection();
    }
}
