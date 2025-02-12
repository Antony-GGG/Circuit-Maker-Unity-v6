using GameAnalyticsSDK;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class Manager : MonoBehaviour
{
    [SerializeField] APIManager _APIManager;

    [SerializeField]private float gridPadding = 1.5f;
    [SerializeField]private PropScript propPrefab;
    [SerializeField]private List<LevelData> levelsData;
    [SerializeField]private LevelData lvlData;
    private PropScript[,] props;
    [SerializeField]private List<PropScript> startPropsList;
    private bool allBulbsFilled;

    [SerializeField]private Sprite lvlLockedIcon;
    [SerializeField]private Sprite lvlIncompleteBtn;
    [SerializeField]private Sprite lvlCompletedIcon;
    [SerializeField]private UnityEngine.UI.Button[] levelButtons;
    private int completedLevels;
    private int currentLevel;

    [SerializeField]private int lvlIndex;

    [SerializeField]private GameObject homeMenuCanvas;
    [SerializeField]private GameObject tutorialCanvas;
    [SerializeField]private GameObject nextLevelCanvas;
    [SerializeField]private GameObject levelCanvas;
    [SerializeField]private GameObject gameOverCanvas;
    [SerializeField]private GameObject replayCanvas;
    [SerializeField]private GameObject gameCompletedCanvas;
    //public GameObject pauseMenuCanvas;

    private int ggScore;

    private GameObject _GGCoinText;

    [SerializeField] private TextMeshProUGUI LevelNumber;

    //Timer & Score Calc
    [SerializeField]private float totalGameTime = 500f;
    private float timeElapsed; // Starting time
    private bool timerIsRunning = false;
    [SerializeField]private TextMeshProUGUI timerText;
    [SerializeField]private TextMeshProUGUI[] scoreText;
    private int playerScore;
    private int[] score = { 100, 200, 300 };
    private float[] timeThresholds = { 50f, 100f };

    private void Start()
    {
        if (!PlayerPrefs.HasKey("PlayerScore"))
        {
            PlayerPrefs.SetInt("PlayerScore", 00);
        }
        if (!PlayerPrefs.HasKey("currentLevel"))
        {
            PlayerPrefs.SetInt("currentLevel", 1);

            lvlIndex = 0;
        }
        if (!PlayerPrefs.HasKey("CompletedLevels"))
        {
            PlayerPrefs.SetInt("CompletedLevels", 0);
        }

        playerScore = PlayerPrefs.GetInt("PlayerScore");
        currentLevel = PlayerPrefs.GetInt("currentLevel");
        completedLevels = PlayerPrefs.GetInt("CompletedLevels");

        Debug.Log("\n" + "Player Score: " + playerScore + "\n" + "Current Level: " + currentLevel + "\n" + "Completed Levels: " + completedLevels + "\n");

        foreach (UnityEngine.UI.Button levelButton in levelButtons)
        {
            SpriteState spriteState = levelButton.spriteState;
            spriteState.disabledSprite = lvlLockedIcon;
            levelButton.spriteState = spriteState;
        }

        for (int i = 0; i < completedLevels; i++)
        {
            levelButtons[i].image.sprite = lvlCompletedIcon;

            if (levelButtons[i + 1] != null)
            {
                levelButtons[i + 1].interactable = true;

                levelButtons[i + 1].GetComponentInChildren<Transform>().GetChild(0).gameObject.SetActive(true);
            }
        }

        _APIManager.StartGame();
        ggScore = 0;
    }

    public void Update()
    {
        if (timerIsRunning)
        {
            if (timeElapsed > 0f)
            {
                // Reduce the timer
                timeElapsed -= Time.deltaTime;
                DisplayTime(timeElapsed);
            }
            else
            {
                GameOver();
            }
        }

        if (!nextLevelCanvas.activeInHierarchy && !tutorialCanvas.activeInHierarchy && !homeMenuCanvas.activeInHierarchy && !levelCanvas.activeInHierarchy && !replayCanvas.activeInHierarchy && !gameOverCanvas.activeInHierarchy && !allBulbsFilled)
        {
            if (!timerIsRunning)
            {
                timerIsRunning = true;
            }
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Calculate the bottom-left position of the grid
            float centerX = (lvlData.col - 1) * gridPadding * 0.5f;
            float centerY = (lvlData.row - 1) * gridPadding * 0.5f;

            // Adjust the mouse position based on the new grid start position
            int col = Mathf.RoundToInt((mousePos.x + centerX) / gridPadding) - lvlData.col / 2;
            int row = Mathf.RoundToInt((mousePos.y + centerY) / gridPadding) - lvlData.row / 2;

            //Debug.Log("Row: "+row+" Column: "+col);

            if (row < 0 || col < 0) return;
            if (row >= lvlData.row) return;
            if (col >= lvlData.col) return;
            if (Input.GetMouseButtonDown(0))
            {
                props[row, col].UpdateInput();
                StartCoroutine(CheckRoutine());
            }
        }
        else if (timerIsRunning)
        {
            timerIsRunning = false;
        }
    }

    private void GameOver()
    {
        gameOverCanvas.SetActive(true);

        if(currentLevel > completedLevels)
        {
            _APIManager.UpdateGameScore(ggScore, _APIManager.ggCoins, "loss", currentLevel);

            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "Level_" + (currentLevel).ToString(), "Score_", 0);
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "Level_" + (currentLevel).ToString(), "Time_", (int)timeElapsed);
        }
    }

    public void DisplayTime(float timeToDisplay)
    {
        int timeToDisp = Mathf.FloorToInt(timeToDisplay);
        timerText.text = timeToDisp.ToString() + "s";
    }

    public void LoadLevel(int _lvlIndex)
    {
        lvlIndex = _lvlIndex;

        timeElapsed = totalGameTime;

        playerScore = PlayerPrefs.GetInt("PlayerScore");
        completedLevels = PlayerPrefs.GetInt("CompletedLevels");

        if(!PlayerPrefs.HasKey("currentLevel"))
        {
            PlayerPrefs.SetInt("currentLevel", 1);
        }
        else
        {
            PlayerPrefs.SetInt("currentLevel", _lvlIndex + 1);
        }

        currentLevel = PlayerPrefs.GetInt("currentLevel");

        if (currentLevel <= completedLevels)
        {
            replayCanvas.SetActive(true);
        }

        ScoreUpdate();

        if (props != null)
        {
            for (int i = 0; i < lvlData.row; i++)
            {
                for (int j = 0; j < lvlData.col; j++)
                {
                    Destroy(props[i, j].gameObject);
                }
            }
            startPropsList.Clear();
        }

        lvlData = levelsData[lvlIndex];

        startPropsList.Clear();
        SpawnCell();

        LevelNumber.text = "Level " + (currentLevel).ToString();

        timerIsRunning = true;

        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "Level_" + (currentLevel).ToString());

        Debug.Log("\n" + "Player Score: " + playerScore + "\n" + "Current Level: " + currentLevel + "\n" + "Completed Levels: " + completedLevels + "\n");
    }

    [DllImport("__Internal")]
    private static extern void GoToURLInSameTab(string url);

    public void QuitGame()
    {
        //webgl pc
        //SceneManager.LoadScene("MainMenu");

        //webgl android ios
        GoToURLInSameTab("https://platform.grandgaming.com/");
    }

    public void RestartLevel()
    {
        for (int i = 0; i < lvlData.row; i++)
        {
            for (int j = 0; j < lvlData.col; j++)
            {
                Destroy(props[i, j].gameObject);
            }
        }
        startPropsList.Clear();

        timeElapsed = totalGameTime;
        LoadLevel(lvlIndex);
        LevelNumber.text = "Level " + (lvlIndex + 1).ToString();
    }

    public void SpawnCell()
    {
        props = new PropScript[lvlData.row, lvlData.col];

        for (int i = 0; i < lvlData.row; i++)
        {
            for (int j = 0; j < lvlData.col; j++)
            {
                PropScript tempProp = Instantiate(propPrefab);
                tempProp.transform.position = new Vector2(j * gridPadding, i * gridPadding);
                tempProp.Initialize(lvlData.propType[i * lvlData.col + j]);
                props[i, j] = tempProp;

                if (tempProp._propType == 1)
                {
                    startPropsList.Add(tempProp);
                }
            }
        }

        // Compute the exact center of the grid
        float centerX = ((lvlData.col - 1) * gridPadding) / 2.0f;
        float centerY = ((lvlData.row - 1) * gridPadding) / 2.0f;

        // Set camera position to center the grid
        Camera.main.transform.position = new Vector3(centerX, centerY, -10f);

        // Compute orthographic size based on the grid dimensions
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = ((lvlData.row) * gridPadding) / 2.0f + 1f;
        float horizontalSize = ((lvlData.col) * gridPadding) / aspectRatio / 2.0f + 1f;

        // Ensure the camera fits both dimensions
        Camera.main.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

        StartCoroutine(CheckRoutine());
    }

    IEnumerator CheckRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        CheckFill();
        CheckWon();
    }

    public void CheckFill()
    {
        for (int i = 0; i < lvlData.row; i++)
        {
            for (int j = 0; j < lvlData.col; j++)
            {
                PropScript prop = props[i, j];
                if (prop._propType != 0)
                {
                    prop.isFilled = false;
                }
            }
        }

        Queue<PropScript> toCheck = new Queue<PropScript>();
        HashSet<PropScript> checkedFilled = new HashSet<PropScript>();
        foreach (var prop in startPropsList)
        {
            toCheck.Enqueue(prop);
        }

        bool bulbReached = false;

        while (toCheck.Count > 0)
        {
            PropScript prop = toCheck.Dequeue();
            checkedFilled.Add(prop);
            List<PropScript> props = prop.connectProps();
            foreach (var _prop in props)
            {
                if (!checkedFilled.Contains(_prop))
                {
                    if (_prop._propType == 6) //propType 6 is bulb
                    {
                        bulbReached = true;
                    }

                    toCheck.Enqueue(_prop);
                }
            }
        }

        if (bulbReached)
        {
            foreach (var prop in checkedFilled)
            {
                prop.isFilled = true;
            }
        }
        else
        {
            foreach (var prop in checkedFilled)
            {
                prop.isFilled = false;
            }
        }

        for (int i = 0; i < lvlData.row; i++)
        {
            for (int j = 0; j < lvlData.col; j++)
            {
                PropScript prop = props[i, j];
                prop.UpdateFilled();
            }
        }
    }

    public void CheckWon()
    {
        allBulbsFilled = true;

        for (int i = 0; i < lvlData.row; i++)
        {
            for (int j = 0; j < lvlData.col; j++)
            {
                PropScript tempProp = props[i, j];

                if (tempProp._propType == 6 && !tempProp.isFilled)
                {
                    allBulbsFilled = false;
                    break;
                }
            }
            if (!allBulbsFilled) break;
        }

        if (allBulbsFilled)
        {
            timerIsRunning = false;

            StartCoroutine(WaitingTime());
        }
    }

    IEnumerator WaitingTime()
    {
        Debug.Log("Waiting");

        yield return new WaitForSeconds(1);

        Debug.Log("Waiting over");

        //Game completed screen
        if (currentLevel == 10)
        {
            Debug.Log("Current level: " + currentLevel + " (inside if)");
            gameCompletedCanvas.SetActive(true);
        }
        else
        {
            Debug.Log("Current level: " + currentLevel + " (inside else)");

            nextLevelCanvas.SetActive(true);

            GGCoinsSetup();
        }

        CompleteLevel();
    }

    private void CompleteLevel()
    {
        float timeUsed = totalGameTime - timeElapsed;
        
        if(currentLevel > completedLevels)
        {
            _APIManager.coinsEarningLevelBased(currentLevel, _GGCoinText);

            PlayerPrefs.SetInt("CompletedLevels", completedLevels + 1);
            Debug.Log("Number of completed level : " + PlayerPrefs.GetInt("CompletedLevels") + " (After update)");

            // Determine score based on time
            if (timeUsed <= timeThresholds[0])
            {
                playerScore += score[2]; // Highest score for quickest completion
                ggScore += score[2];

                PlayerPrefs.SetInt("PlayerScore",playerScore);

                _APIManager.UpdateGameScore(ggScore, _APIManager.ggCoins, "win", currentLevel);

                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Score", score[2]);
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Time", (int)timeElapsed);
            }
            else if (timeUsed <= timeThresholds[1])
            {
                playerScore += score[1]; // Mid score for medium speed completion
                ggScore += score[1];
                PlayerPrefs.SetInt("PlayerScore", playerScore);

                _APIManager.UpdateGameScore(ggScore, _APIManager.ggCoins, "win", currentLevel);

                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Score", score[1]);
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Time", (int)timeElapsed);
            }
            else
            {
                playerScore += score[0]; // Lowest score for slow completion
                ggScore += score[0];
                PlayerPrefs.SetInt("PlayerScore", playerScore);

                _APIManager.UpdateGameScore(ggScore, _APIManager.ggCoins, "win", currentLevel);

                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Score", score[0]);
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + currentLevel.ToString(), "Time", (int)timeElapsed);
            }
            ScoreUpdate();

            levelButtons[lvlIndex].image.sprite = lvlCompletedIcon;
            if (levelButtons[lvlIndex + 1] != null)
            {
                levelButtons[lvlIndex + 1].interactable = true;
                levelButtons[lvlIndex + 1].GetComponentInChildren<Transform>().GetChild(0).gameObject.SetActive(true);
            }
        }

        if (currentLevel % GrandAdManager.instance.adsAfter == 0)
        {
            Debug.Log(currentLevel);
            GrandAdManager.instance.ShowAd("startAd");
        }
    }

    private void ScoreUpdate()
    {
        for(int i = 0; i < scoreText.Length; i++)
        {
            scoreText[i].text = "Score: " + playerScore;
        }
    }

    private void GGCoinsSetup()
    {
        if (_GGCoinText == null)
        {
            _GGCoinText = GameObject.FindGameObjectWithTag("GGCoinText");
        }

        if (_GGCoinText != null)
        {
            if (_GGCoinText.gameObject.activeSelf)
            {
                _GGCoinText.gameObject.SetActive(false);
            }
        }
    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();

        levelButtons[0].image.sprite = lvlIncompleteBtn;
        levelButtons[0].interactable = true;

        for (int i = 1; i < levelButtons.Length; i++)
        {
            levelButtons[i].image.sprite = lvlIncompleteBtn;

            levelButtons[i].interactable = false;

            levelButtons[i].GetComponentInChildren<Transform>().GetChild(0).gameObject.SetActive(false);

            SpriteState spriteState = levelButtons[i].spriteState;
            spriteState.disabledSprite = lvlLockedIcon;
            levelButtons[i].spriteState = spriteState;
        }

        playerScore = 0;
        ggScore = 0;

        ScoreUpdate();

        _APIManager.StartGame();

        homeMenuCanvas.SetActive(true);
    }
}