using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using System.Linq;

public class TuningController : MonoBehaviour
{
    [SerializeField] private bool debuggingMode;
    [SerializeField] private bool useFixationScenes;
    public int seed = -1;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private List<string> scenes = new();
    [SerializeField] private Transform visionCatcherPrefabNone;
    [SerializeField] private List<CatcherRepretitions> visionCatcherRepetitions = new List<CatcherRepretitions>();
    [SerializeField] private List<GameObject> QuestionsPrefabs = new();
    [SerializeField] private GameObject SupportCrossPrefab;
    [SerializeField] private GameObject SupportCheckPrefab;
    [SerializeField] private GameObject EyeTrackingTransformPrefab;
    private GameObject currentQuestionObject;
    private GameObject currentSupportCross;
    private GameObject currentSupportCheck;
    private Transform currentEyeTrackingTransform;
    [HideInInspector] public TuningDataCollector dataCollector;
    [HideInInspector] public FadeController fadeController;
    [HideInInspector] public Utilities utilities;
    private Environments currentEnvScript;
    [HideInInspector]
    public EyeTrackingRaycast currentEyeTrackingScript;
    private Transform searchTransform;
    private List<Combination> combinations = new();
    private List<string> usedLocations = new();
    int currentIndex = -1;
    string tuningScene = "TuningSceneStartup";
    string endScene = "EndScene";
    string fixationScene = "FixationSceneTuning";
    Transform currentVisionCatcherPrefab;
    VisionCatcher currentVisionCatcherScript;
    private float supportObjectWaitTime = 1.5f;
    private bool waiting = false;
    [HideInInspector]
    public Transform environmentPosition;
    bool environmentPositionSet = false;
    int attempts = 0;
    //bool isCSQuestionnaireScene = false;

    private Coroutine sceneTimerCoroutine;
    private Coroutine correctnessCoroutine;

    public InputActionAsset triggerAction;
    public InputActionReference triggerLeft;
    public InputActionReference triggerRight;

    public RippleTuning currentRipple;
    private int currentQuestion = 0;

    private TuneTask currentTuneTask;

    private int run = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetupController();
        SetupStudy();
        SetupDataCollection();
    }

    private void SetupStudy()
    {
        Random.InitState(seed);
        List<Combination> tmpCombinations = new();
        Application.targetFrameRate = 90;
        for (int i = 0; i < scenes.Count; i++)
        {
            for (int j = 0; j < visionCatcherRepetitions.Count; j++)
            {
                tmpCombinations.Add(new Combination(fixationScene, visionCatcherPrefabNone));
                for (int k = 0; k < visionCatcherRepetitions[j].repetitions; k++)
                {
                    tmpCombinations.Add(new Combination(scenes[i], visionCatcherRepetitions[j].visionCatcherPrefab));
                }
                if (useFixationScenes)
                {
                    tmpCombinations.Add(new Combination(fixationScene, visionCatcherPrefabNone));
                }
            }
            combinations.AddRange(tmpCombinations);
            tmpCombinations = new();
        }
        currentTuneTask = TuneTask.visibleButAcceptable;
        Debug.Log("Total of " + combinations.Count + " experiments...");
    }

    private void SetupController()
    {
        dataCollector = GetComponent<TuningDataCollector>();
        fadeController = GetComponent<FadeController>();
        utilities = GetComponent<Utilities>();
        currentEyeTrackingScript = GetComponent<EyeTrackingRaycast>();
        currentEyeTrackingScript.mainCamera = mainCamera.transform;
        if (seed < 0)
        {
            seed = Random.Range(0, 30000);
        }
    }

    void OnEnable()
    {
        triggerAction.Enable();
    }

    void OnDisable()
    {
        triggerAction.Disable();
    }

    private void Update()
    {
        if (debuggingMode && Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
            return;
        }

        if (Input.GetKeyUp(KeyCode.O))
        {
            InitialSetEnvironmentPosAndRot();
        }
    }

    private void FixedUpdate()
    {
        // object seen
        if (!waiting && (Input.GetKeyUp(KeyCode.Space) 
            ||
            (Input.GetKeyUp(KeyCode.P)) 
            //||
            //(triggerLeft.action.ReadValue<float>() > 0.1f) 
            //||
            //(triggerRight.action.ReadValue<float>() > 0.1f)
            ))
        {
            waiting = true;
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == tuningScene)
            {
                LoadNextScene();
                return;
            }
            correctnessCoroutine = StartCoroutine(CheckCorrectness());
        }
    }

    private IEnumerator CheckCorrectness()
    {
        bool correct = currentEyeTrackingScript.lookingAtSearchObject;
        //waiting = true;
        Transform gameObjectTransform;
        if (correct)
        {
            if (sceneTimerCoroutine != null)
            {
                StopCoroutine(sceneTimerCoroutine);
            }
            gameObjectTransform = currentSupportCheck.transform;
            gameObjectTransform.transform.position = mainCamera.transform.position + ((searchTransform.position - mainCamera.transform.position) / 1.1f);
        }
        else
        {
            attempts += 1;
            gameObjectTransform = currentSupportCross.transform;
            gameObjectTransform.transform.position = mainCamera.transform.position + (currentEyeTrackingScript.gazeRay.direction * currentEnvScript.maxSearchDistance);
        }
        gameObjectTransform.transform.LookAt(mainCamera.transform);
        float distance = Vector3.Distance(gameObjectTransform.position, mainCamera.transform.position);
        float scaleFactor = distance / mainCamera.orthographicSize;
        float fixedScale = 1f;
        gameObjectTransform.localScale = Vector3.one * scaleFactor * fixedScale;
        gameObjectTransform.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        gameObjectTransform.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        gameObjectTransform.gameObject.SetActive(true);
        yield return new WaitForSeconds(supportObjectWaitTime);
        gameObjectTransform.gameObject.SetActive(false);
        if (correct)
        {
            SearchObjectFound();
        }
        else
        {
            waiting = false;
        }
    }

    private void SearchObjectFound()
    {
        currentVisionCatcherScript.StopVisionCatcher();
        NotifyDataCollector();
        ResumeScene();
    }

    private void DisplayQuestions()
    {
        Transform questionTransform = currentEnvScript.GetQuestionTransform();
        currentQuestionObject = Instantiate(QuestionsPrefabs[currentQuestion], questionTransform);
    }

    private void NotifyDataCollector()
    {
        WriteData();
    }

    public void ResumeScene()
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (currentVisionCatcherScript)
        {
            //dataCollector.SetDataCollectingState(false);
            currentVisionCatcherScript.StopVisionCatcher();
            currentEyeTrackingScript.eyeTrackingActive = false;
            currentEyeTrackingTransform = null;
        }
        if (currentEyeTrackingTransform)
        {
            currentEyeTrackingTransform = null;
        }
        currentIndex++;
        if (SceneManager.GetActiveScene().name == endScene) return;
        // TODO remove?:
        if (currentIndex > combinations.Count - 1)
        {
            // TODO remove?: stop data collector 
            Debug.Log("Done.");
            LoadScene(endScene);
            return;
        }
        Debug.Log(currentIndex.ToString() + ": " + combinations[currentIndex].ToString());
        currentVisionCatcherPrefab = combinations[currentIndex].visionCatcherPrefab;
        LoadScene(currentIndex);
    }

    public void LoadScene(string name)
    {
        ClearEnvironment();
        StartCoroutine(LoadSceneWithFade(name));
    }

    public void LoadScene(int index)
    {
        ClearEnvironment();
        if (index >= 0 && index < combinations.Count)
        {
            StartCoroutine(LoadSceneWithFade(index));
        }
        else
        {
            Debug.LogError("index error");
        }
    }

    private IEnumerator LoadSceneWithFade(string name)
    {
        fadeController.FadeOut();
        yield return new WaitForSeconds(fadeController.fadeDuration);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(name);
        waiting = false;
    }

    private IEnumerator LoadSceneWithFade(int index)
    {
        fadeController.FadeOut();
        yield return new WaitForSeconds(fadeController.fadeDuration);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(combinations[index].sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // fixationcross between scenes to adjust alignment
        if (scene.name == fixationScene)
        {
            FindSceneEnvironment();
            SetupEnvironment();
            fadeController.FadeInNoDelay();
            return;
        }
        // last scene reached
        if (scene.name == endScene)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }
        FindSceneEnvironment();
        SetupEnvironment();
        SetupVisionCatcher();
        DisplayQuestions();
        fadeController.FadeIn();
        attempts = 0;
        waiting = false;
    }

    private void SetupVisionCatcher()
    {
        var newVisionCatcher = Instantiate(currentVisionCatcherPrefab);
        currentVisionCatcherScript = newVisionCatcher.GetComponent<VisionCatcher>();
        currentVisionCatcherScript.SetupVisionCatcher(searchTransform, mainCamera, this);
        // to set our new ripple values after the intensity / freq+speed were set
        currentRipple = newVisionCatcher.GetComponent<RippleTuning>();
        SetCurrentRippleValues();
        currentVisionCatcherScript.StartVisionCatcher();
    }

    private void SetCurrentRippleValues()
    {
        // to switch the questions to "visible tuning"
        Debug.Log("---");
        Debug.Log(currentTuneTask);
        if (currentTuneTask != currentRipple.tuneTask)
        {
            currentTuneTask = currentRipple.tuneTask;
            dataCollector.intensities = new();
            dataCollector.frequencies = new();
            dataCollector.speeds = new();
            dataCollector.angles = new();
            dataCollector.SetupNewRun("barelyVisibleTuning");
            run = 3;
        }
        // randomize intensity
        currentRipple.intensity = Random.Range(0f, 0.5f);
        currentQuestion = 0 + run;
        if (currentRipple.tunable.Contains(TunableRipple.intensity)) return;
        // ^ intensity tuned

        // randomize frequency
        currentRipple.frequency = Random.Range(0.0005f, 0.0045f);
        currentRipple.intensity = dataCollector.intensities.Average();
        currentQuestion = 1 + run;
        if (currentRipple.tunable.Contains(TunableRipple.frequency)) return;
        // ^ frequency tuned

        // randomize speed
        currentRipple.rippleSpeed = Random.Range(0.01f, 0.05f);
        currentRipple.frequency = dataCollector.frequencies.Average();
        currentQuestion = 2 + run;
        if (currentRipple.tunable.Contains(TunableRipple.speed)) return;
        // ^ speed tuned

        // randomize angle
        currentRipple.angle = Random.Range(25f, 90f);
        currentRipple.rippleSpeed = dataCollector.speeds.Average();
        currentQuestion = 2 + run;
    }

    private void FindSceneEnvironment()
    {
        currentEnvScript = FindFirstObjectByType<Environments>();
        if (currentEnvScript == null)
        {
            Debug.LogError("current environment script not found");
        }
    }

    private void SetupEnvironment()
    {
        currentSupportCross = Instantiate(SupportCrossPrefab);
        currentSupportCheck = Instantiate(SupportCheckPrefab);
        currentSupportCheck.SetActive(false);
        currentSupportCross.SetActive(false);
        currentEyeTrackingTransform = Instantiate(EyeTrackingTransformPrefab).transform;
        currentEyeTrackingScript.eyetrackingTransform = currentEyeTrackingTransform;
        currentEyeTrackingScript.eyeTrackingActive = true;
        currentEnvScript.tuningController = this;
        currentEnvScript.SetupEnvironment();
        SetEnvironmentPosAndRot();
        searchTransform = currentEnvScript.GetSearchObject();
    }

    private void InitialSetEnvironmentPosAndRot()
    {
        environmentPosition = transform.GetChild(0);
        currentEnvScript.environmentTransform.position = mainCamera.transform.position + new Vector3(0, -2, 0);
        Vector3 targetDirection = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        currentEnvScript.environmentTransform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        environmentPosition.rotation = currentEnvScript.environmentTransform.rotation;
        environmentPosition.position = currentEnvScript.environmentTransform.position;
        environmentPositionSet = true;
    }

    private void SetEnvironmentPosAndRot()
    {
        if (!environmentPositionSet)
        {
            InitialSetEnvironmentPosAndRot();
        }
        currentEnvScript.environmentTransform.rotation = environmentPosition.rotation;
        currentEnvScript.environmentTransform.position = environmentPosition.position;
    }

    private void ClearEnvironment()
    {
        currentEnvScript = null;
        searchTransform = null;
    }

    public Transform GetCamera()
    {
        return mainCamera.transform;
    }

    private void SetupDataCollection()
    {
        dataCollector.controller = this;
        dataCollector.SetupNewRun("clearlyVisibleTuning");
    }

    public void WriteData()
    {
        dataCollector.ApplyVisionCatcher(currentVisionCatcherScript);
        dataCollector.SaveToFile();
    }

    public void SearchLocationUsed(string location)
    {
        usedLocations.Add(location);
    }

    public List<string> GetAllUsedSearchLocations()
    {
        return usedLocations;
    }
}

[System.Serializable]
public class CatcherRepretitions
{
    public Transform visionCatcherPrefab;
    public int repetitions;
}