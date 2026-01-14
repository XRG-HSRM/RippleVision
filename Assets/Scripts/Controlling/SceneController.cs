using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Linq;

public class SceneController : MonoBehaviour
{
    [Header("Enable debugging, no data will be collected")]
    [SerializeField] private bool debuggingMode;
    [Header("Use the fixation scene between every other scene")]
    [SerializeField] private bool useFixationScenes;
    [Header("Randomize trials")]
    [SerializeField] private bool shuffleSetup;
    [Header("Repetitions of each technique")]
    [SerializeField] private int repetitions;
    [Header("Maximum time until a scene autonomously finishes")]
    [SerializeField] private float maximumSceneDuration;
    [Header("Rng Seed: -1 for random")]
    public int seed = -1;
    [Header("VR Rig and Main camera (Must have the VRCamera Script)")]
    [SerializeField] private Transform VRRig;
    [SerializeField] private Camera mainCamera;
    [Header("Single scenes before starting the first run")]
    [SerializeField] private List<string> startupScenes = new();
    [Header("Scene names (must contain 'TestDemo' to be treated as such)")]
    [SerializeField] private List<string> scenes = new();
    [Header("Single scenes after finishing all runs")]
    [SerializeField] private List<string> endScenes = new();
    [Header("'None'-Technique")]
    [SerializeField] private Transform visionCatcherPrefabNone;
    [Header("Techniques")]
    [SerializeField] private List<Transform> visionCatcherPrefabs = new List<Transform>();
    [Header("Techniques to view in TestDemo Scene")]
    [SerializeField] private List<Transform> demoVisionCatchers = new List<Transform>();
    [Header("Question Items")]
    [SerializeField] private GameObject QuestionCanvas;
    [SerializeField] private GameObject QuestionsPrefab;
    [SerializeField] private GameObject ConfirmPrefab;
    [SerializeField] private List<Question> questions;
    [Header("Confirmation Items")]
    [SerializeField] private GameObject SupportCrossPrefab;
    [SerializeField] private GameObject SupportCheckPrefab;
    [Header("EyeTrack Transform")]
    [SerializeField] private GameObject EyeTrackingTransformPrefab;

    private GameObject currentQuestionObject;
    private List<Question> currentQuestions;

    private GameObject currentSupportCross;
    private GameObject currentSupportCheck;
    private Transform currentEyeTrackingTransform;
    [HideInInspector] public DataCollector dataCollector;
    [HideInInspector] public FadeController fadeController;
    [HideInInspector] public Utilities utilities;
    private Environments currentEnvScript;
    [HideInInspector]
    public EyeTrackingRaycast currentEyeTrackingScript;
    private Transform searchTransform;
    private List<Combination> combinations = new();
    private List<string> usedLocations = new();
    int currentIndex = -1;
    string[] startScenes = new string[] { "MS_StartupScene", "LabyrinthStartupScene", "DynamicsStartupScene", "TuningScene", "MS_DynamicsStartupScene" };
    string csQuestionnaireScene = "ArtificialEndScene";
    string endScene = "EndScene";
    string fixationScene = "FixationScene";
    Transform currentVisionCatcherPrefab;
    VisionCatcher currentVisionCatcherScript;
    private float supportObjectWaitTime = 1.5f;
    private bool waiting = false;
    [HideInInspector]
    public Transform environmentPosition;
    bool objectFoundSuccessfully = false;
    int attempts = 0;
    //bool isCSQuestionnaireScene = false;

    private List<VRButtonGroup> buttonGroups = new();
    private ConfirmButton currentConfirmButton;

    private Coroutine sceneTimerCoroutine;
    private Coroutine correctnessCoroutine;

    [Header("VR Hand Controller Items")]
    public InputActionAsset triggerAction;
    public InputActionReference triggerLeft;
    public InputActionReference triggerRight;

    private LaserPointer laserPointer;

    [HideInInspector]
    public static SceneController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetupController();
        SetupStudy();
        if (!debuggingMode)
        {
            SetupDataCollection();
        }
    }

    private void SetupStudy()
    {
        Random.InitState(seed);
        List<Combination> tmpCombinations = new();
        //visionCatcherPrefabs.Insert(0, visionCatcherPrefabNone);
        Application.targetFrameRate = 90;
        for (int i = 0; i < scenes.Count; i++)
        {
            if (!scenes[i].Contains("TestDemo") && !scenes[i].Contains("PauseScene"))
            {
                for (int j = 0; j < visionCatcherPrefabs.Count; j++)
                {
                    tmpCombinations.Add(new Combination(scenes[i], visionCatcherPrefabs[j]));
                }
                List<Combination> tmp = new List<Combination>(tmpCombinations);
                for (int k = 0; k < repetitions; k++)
                {
                    tmpCombinations.AddRange(tmp);
                }
                if (shuffleSetup)
                {
                    // need different shuffles (seeds)
                    tmpCombinations = utilities.ShuffleList(tmpCombinations, seed + i);
                }
                combinations.AddRange(tmpCombinations);
                tmpCombinations = new();
            }
            else if (scenes[i].Contains("TestDemoCatchers"))
            {
                // all vision catchers in catcher test scene
                for (int j = 0; j < demoVisionCatchers.Count; j++)
                {
                    combinations.Add(new Combination(scenes[i], demoVisionCatchers[j]));
                }
                if (useFixationScenes)
                {
                    combinations.Add(new Combination(fixationScene, visionCatcherPrefabNone));
                }
            }
            else
            {
                // no vision catcher in demo scene
                combinations.Add(new Combination(scenes[i], visionCatcherPrefabNone));
                if (useFixationScenes)
                {
                    combinations.Add(new Combination(fixationScene, visionCatcherPrefabNone));
                }
            }
            if (scenes[i].Contains("PauseScene"))
            {
                if (useFixationScenes)
                {
                    combinations.Add(new Combination(fixationScene, visionCatcherPrefabNone));
                }
            }

        }
        Debug.Log("Total of " + combinations.Count + " experiments...");
        //if (useFixationScenes)
        //{
        //    for (int i = 0; i < combinations.Count; i += 2)
        //    {
        //        combinations.Insert(i, new Combination(fixationScene, visionCatcherPrefabNone));
        //    }
        //}
        if (debuggingMode) return;
        for (int h = 0; h < startupScenes.Count; h++)
        {
            combinations.Insert(h, new Combination(startupScenes[h], visionCatcherPrefabNone));
        }
        for (int h = 0; h < endScenes.Count; h++)
        {
            combinations.Add(new Combination(endScenes[h], visionCatcherPrefabNone));
        }
        if (useFixationScenes)
        {
            combinations.Insert(0, new Combination(fixationScene, visionCatcherPrefabNone));
        }
    }

    private void SetupController()
    {
        dataCollector = GetComponent<DataCollector>();
        fadeController = GetComponent<FadeController>();
        utilities = GetComponent<Utilities>();
        currentEyeTrackingScript = GetComponent<EyeTrackingRaycast>();
        laserPointer = GetComponent<LaserPointer>();
        currentEyeTrackingScript.mainCamera = mainCamera.transform;
        if (seed < 0)
        {
            seed = Random.Range(0, 30000);
        }
    }

    private void SetupQuestions()
    {
        if (currentQuestionObject) Destroy(currentQuestionObject);
        currentQuestionObject = Instantiate(QuestionCanvas);
        currentQuestionObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        GameObject newQuestion;
        float location = 0f;
        for (int i = 0; i < currentQuestions.Count; i++)
        {
            newQuestion = Instantiate(QuestionsPrefab, currentQuestionObject.transform);
            newQuestion.transform.SetPositionAndRotation(new Vector3(0f, -location, 0f), Quaternion.identity);
            Question currentQuestion = currentQuestions[i];
            currentQuestion.SetAnswer("error");
            VRButtonGroup btn = newQuestion.GetComponent<VRButtonGroup>();
            btn.question = currentQuestion.question;
            btn.answerOptions = currentQuestion.answers.Count;
            btn.questionHeight = currentQuestion.questionHeight;
            btn.CreateButtons(this, i, currentQuestion.answerWidth, currentQuestions[i].answers, currentQuestion.isEditable);
            location += currentQuestions[i].questionHeight;
            buttonGroups.Add(btn);
        }
        newQuestion = Instantiate(ConfirmPrefab, currentQuestionObject.transform);
        newQuestion.transform.SetPositionAndRotation(new Vector3(0f, -location + 0.2f, 0f), Quaternion.identity);
        currentConfirmButton = newQuestion.GetComponent<ConfirmButton>();
        currentConfirmButton.SetupConfirmButton(this);

        DontDestroyOnLoad(currentQuestionObject);

        currentQuestionObject.SetActive(false);
    }

    public void SetAnswer(int i, string answer)
    {
        currentQuestions[i].SetAnswer(answer);
        for (int j = 0; j < currentQuestions.Count; j++)
        {
            if (currentQuestions[j].GetAnswer() == "error")
            {
                return;
            }
        }
        EnableConfirmButton();
    }

    private void EnableConfirmButton()
    {
        currentConfirmButton.EnableButton();
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
        if (debuggingMode && !waiting && Input.GetKeyUp(KeyCode.L))
        {
            waiting = true;
            LoadNextScene();
            return;
        }

        if (Input.GetKeyUp(KeyCode.O))
        {
            SetPlayerPosAndRot();
        }
        if (!debuggingMode && !waiting && Input.GetKeyUp(KeyCode.P))
        {
            CheckInput();
        }
    }

    private void FixedUpdate()
    {
        //if (!waiting && (debuggingMode) && ((triggerLeft.action.ReadValue<float>() > 0.9f) ||
        //    (triggerRight.action.ReadValue<float>() > 0.9f)))
        //{
        //    waiting = true;
        //    LoadNextScene();
        //    return;
        //}
        if (!debuggingMode && !waiting && ((triggerLeft.action.ReadValue<float>() > 0.9f) ||
            (triggerRight.action.ReadValue<float>() > 0.9f))
            )
        {
            CheckInput();
        }
    }

    private void CheckInput()
    {
        // object seen
        if (currentEnvScript != null && !currentEnvScript.enableGuidance) return;
        waiting = true;
        string currentScene = SceneManager.GetActiveScene().name;
        if (startScenes.Contains(currentScene))
        {
            LoadNextScene();
            return;
        }
        correctnessCoroutine = StartCoroutine(CheckCorrectness());
    }

    private IEnumerator CheckCorrectness()
    {
        bool correct = currentEyeTrackingScript.lookingAtSearchObject;
        //waiting = true;
        Transform gameObjectTransform;
        if (correct)
        {
            currentVisionCatcherScript.StopVisionCatcher();
            if (sceneTimerCoroutine != null)
            {
                StopCoroutine(sceneTimerCoroutine);
            }
            if (!debuggingMode)
            {
                dataCollector.SetDataCollectingState(false);
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
            objectFoundSuccessfully = true;
            SearchObjectFound();
        }
        else
        {
            waiting = false;
        }
    }

    private void SearchObjectFound()
    {
        if (debuggingMode)
        {
            Debug.LogError("In debugging mode. NOT collecting data.");
            ResumeScene();
        }
        DisplayQuestions();
    }

    private void DisplayQuestions()
    {
        SetupQuestions();
        Vector3 location = searchTransform.position;
        Vector3 direction = location - mainCamera.transform.position;
        currentQuestionObject.transform.position = mainCamera.transform.position + (direction).normalized * 3.5f;
        currentQuestionObject.transform.position = new Vector3(currentQuestionObject.transform.position.x, mainCamera.transform.position.y + 0.75f, currentQuestionObject.transform.position.z);
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            currentQuestionObject.transform.rotation = targetRotation;
        }
        currentQuestionObject.SetActive(true);
        laserPointer.ActivateLaserPointer(true);
    }

    public void ConfirmAnswers()
    {
        List<string> answers = new();
        for (int i = 0; i < currentQuestions.Count; i++)
        {
            answers.Add(currentQuestions[i].GetAnswer().ToString());
        }
        if (currentEnvScript.isQuestionScene)
        {
            dataCollector.CollectCustomData(answers);
        }
        else if (currentEnvScript.enableDataCollector)
        {
            dataCollector.SetSetupData(answers, objectFoundSuccessfully, attempts);
        }
    }

    public void ResumeScene()
    {
        if (currentQuestionObject) Destroy(currentQuestionObject);
        laserPointer.ActivateLaserPointer(false);
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        laserPointer.ActivateLaserPointer(false);
        if (currentVisionCatcherScript)
        {
            currentVisionCatcherScript.StopVisionCatcher();
            currentEyeTrackingScript.eyeTrackingActive = false;
            currentEyeTrackingTransform = null;
        }
        if (currentEyeTrackingTransform)
        {
            currentEyeTrackingTransform = null;
        }
        currentIndex++;
        if (SceneManager.GetActiveScene().name == endScene)
        {
            Application.Quit();
            return;
        }
        // TODO remove?:
        if (currentIndex > combinations.Count - 1)
        {
            // TODO remove?: stop data collector 
            Debug.Log("Done.");
            LoadScene(endScene);
            return;
        }
        //Debug.Log(currentIndex.ToString() + ": " + combinations[currentIndex].ToString());
        currentVisionCatcherPrefab = combinations[currentIndex].visionCatcherPrefab;
        Debug.Log(currentVisionCatcherPrefab.name);
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
        // last scene reached
        if (scene.name == endScene)
        {
            Application.Quit();
            return;
        }
        FindSceneEnvironment();
        SetupEnvironment();
        if (currentEnvScript.enableGuidance)
        {
            SetupVisionCatcher();
            if (!debuggingMode && currentEnvScript.enableDataCollector)
            {
                UpdateDataCollector();
            }
            if (currentEnvScript.enableDataCollector)
            {
                sceneTimerCoroutine = StartCoroutine(SceneTimer());
                attempts = 0;
                waiting = false;
            }
        }
        if (currentEnvScript.isQuestionScene)
        {
            currentQuestions = currentEnvScript.transform.GetComponent<QuestionScene>().GetQuestions();
            List<string> r_questions = new();
            for (int i = 0; i < currentQuestions.Count; i++)
            {
                //Debug.Log(currentQuestions[i].CSVName);
                r_questions.Add(currentQuestions[i].CSVName);
            }
            dataCollector.SetupCustomCSV(currentEnvScript.environmentName, r_questions);
            DisplayQuestions();
        }
        else
        {
            currentQuestions = questions;
        }
        fadeController.FadeIn();
    }

    IEnumerator SceneTimer()
    {
        yield return new WaitForSeconds(maximumSceneDuration);
        if (correctnessCoroutine != null)
        {
            StopCoroutine(correctnessCoroutine);
        }
        if (!debuggingMode)
        {
            dataCollector.SetDataCollectingState(false);
        }
        waiting = true;
        objectFoundSuccessfully = false;
        Transform gameObjectTransform;
        gameObjectTransform = currentSupportCross.transform;
        gameObjectTransform.transform.position = mainCamera.transform.position + (currentEyeTrackingScript.gazeRay.direction * currentEnvScript.maxSearchDistance);
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
        SearchObjectFound();
    }

    private void UpdateDataCollector()
    {
        dataCollector.eyeTrackingTransform = currentEyeTrackingTransform;
        SetEnvAndCatcherData();
        dataCollector.SetDataCollectingState(true);
    }

    private void SetupVisionCatcher()
    {
        var newVisionCatcher = Instantiate(currentVisionCatcherPrefab);
        currentVisionCatcherScript = newVisionCatcher.GetComponent<VisionCatcher>();
        currentVisionCatcherScript.SetupVisionCatcher(searchTransform, mainCamera, this);
        currentVisionCatcherScript.StartVisionCatcher();
    }

    public void FindSceneEnvironment()
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
        currentEnvScript.controller = this;
        currentEnvScript.SetupEnvironment();
        environmentPosition = currentEnvScript.environmentTransform;
        SetPlayerPosAndRot();
        searchTransform = currentEnvScript.GetSearchObject();
    }

    private void SetPlayerPosAndRot()
    {
        //VRRig.rotation = currentEnvScript.playerLocation.rotation;
        VRRig.position = currentEnvScript.playerLocation.position;
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
        Debug.Log(seed);
        List<string> r_questions = new();
        for (int i = 0; i < questions.Count; i++)
        {
            r_questions.Add(questions[i].CSVName);
        }
        dataCollector.Setup("Setup", "Data", r_questions, seed);
    }

    public void SetEnvAndCatcherData()
    {
        dataCollector.SetEnvAndCatcher(combinations[currentIndex].sceneName, combinations[currentIndex].visionCatcherPrefab.GetComponent<VisionCatcher>().visionCatcherName, searchTransform);
    }

    public void SearchLocationUsed(string location)
    {
        usedLocations.Add(location);
    }

    public List<string> GetAllUsedSearchLocations()
    {
        return usedLocations;
    }

    public LaserPointer GetLaserPointer()
    {
        return laserPointer;
    }
}

[System.Serializable]
public class Question
{
    public string CSVName;
    public string question;
    public List<string> answers;
    public float questionHeight;
    public float answerWidth;
    public bool isEditable;

    private string answer = "error";

    public void SetAnswer(string answer)
    {
        this.answer = answer;
    }

    public string GetAnswer()
    {
        return answer;
    }
}