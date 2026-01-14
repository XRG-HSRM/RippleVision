using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TuningDataCollector : MonoBehaviour
{
    [HideInInspector]
    public TuningController controller;
    [HideInInspector]
    public int currentStudyID = 0;

    private RippleTuning rV;
    private TunableRipple[] tunable;
    private bool onlyInOneEye;
    private bool stereoInverse;
    private bool useDirection;
    private float directionRange;
    private float minDistance;
    private float foveaSize;
    private float angle;
    private float fallOff;
    private float rippleSpeed;
    private float brightness;
    private float intensity;
    private float frequency;

    public List<float> intensities = new List<float>();
    public List<float> frequencies = new List<float>();
    public List<float> speeds = new List<float>();
    public List<float> angles = new List<float>();

    private string filePath;

    public void SetupNewRun(string studyType)
    {
        filePath = Path.Combine(Application.dataPath, "data", studyType + ".csv");
        if (!File.Exists(filePath))
        {
            string header = "currentStudyID;task;tunables;onlyInOneEye;stereoInverse;useDirection;directionRange;minDistance;foveaSize;angle;fallOff;rippleSpeed;brightness;intensity;frequency";
            File.WriteAllText(filePath, header + "\n");
            currentStudyID = 0;
        }
        else
        {
            var lines = File.ReadAllLines(filePath).Skip(1);

            if (lines.Any())
            {
                currentStudyID = lines
                    .Select(line => int.TryParse(line.Split(';')[0], out var id) ? id : -1)
                    .Where(id => id >= 0)
                    .DefaultIfEmpty(-1)
                    .Max() + 1;
            }
            else
            {
                currentStudyID = 0;
            }
        }
    }

    public void ApplyVisionCatcher(VisionCatcher visionCatcher)
    {
        ApplyRippleCatcher(visionCatcher);
    }

    private void ApplyRippleCatcher(VisionCatcher visionCatcher)
    {
        rV = visionCatcher.transform.GetComponent<RippleTuning>();
        tunable = rV.tunable;
        onlyInOneEye = rV.onlyInOneEye;
        stereoInverse = rV.stereoInverse;
        directionRange = rV.directionRange;
        minDistance = rV.minDistance;
        foveaSize = rV.foveaSize;
        angle = rV.angle;
        fallOff = rV.fallOff;
        rippleSpeed = rV.rippleSpeed;
        brightness = rV.brightness;
        intensity = rV.intensity;
        frequency = rV.frequency;
    }

    // TODO: add studyID / current task
    public void SaveToFile()
    {
        foreach (TunableRipple t in tunable)
        {
            switch (t)
            {
                case TunableRipple.intensity:
                    intensities.Add(intensity);
                    break;
                case TunableRipple.frequency:
                    frequencies.Add(frequency);
                    break;
                case TunableRipple.speed:
                    speeds.Add(rippleSpeed);
                    break;
                case TunableRipple.angle:
                    angles.Add(angle);
                    break;
                default:
                    break;
            }
        }
        string tunables = "";
        for (int i = 0; i < tunable.Length; i++)
        {
            tunables += tunable[i].ToString();
            if (i < tunable.Length - 1)
            {
                tunables += " + ";
            }
        }
        string line = $"{currentStudyID};{controller.currentRipple.tuneTask};{tunables};{onlyInOneEye};{stereoInverse};{useDirection};{directionRange};{minDistance};{foveaSize};{angle};{fallOff};{rippleSpeed};{brightness};{intensity};{frequency}";
        File.AppendAllText(filePath, line + "\n");
    }
}
