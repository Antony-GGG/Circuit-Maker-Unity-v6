using GameAnalyticsSDK;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class Manager : MonoBehaviour
{
    [SerializeField] APIManager _APIManager;

    public float gridPadding = 1.5f;
    public PropScript propPrefab;
    public List<LevelData> levelsData;
    public LevelData lvlData;
    public PropScript[,] props;
    public List<PropScript> startPropsList;
    public bool allBulbsFilled;

    public Sprite lvlLockedIcon;
    public Sprite lvlIncompleteBtn;
    public Sprite lvlCompletedIcon;
    public bool[] completedLevels;
    public bool allLevelsDone = false;
    public UnityEngine.UI.Button[] levelButtons;
    //public Button[] levelButton;

    public int lvlIndex;

    public GameObject homeMenuCanvas;
    public GameObject nextLevelCanvas;
    public GameObject levelCanvas;
    public GameObject gameOverCanvas;
    public GameObject replayCanvas;
    public GameObject gameCompletedCanvas;
    //public GameObject pauseMenuCanvas;

    public TextMeshProUGUI LevelNumber;

    //Timer & Score Calc
    public float TotalGameTime = 500f;
    public float timeElapsed; // Starting time
    public bool timerIsRunning = false;
    public TextMeshProUGUI timerText;
    public float[] timeThresholds = { 50f, 100f };
    public TextMeshProUGUI[] scoreText;
    public int playerScore;
    public int[] score = { 100, 200, 300 };

    public bool gameOver = false;


    private void Start()
    {
        timeElapsed = TotalGameTime;
        completedLevels = new bool[levelsData.Count];

        for (int i = 0; i < completedLevels.Length; i++)
        {
            completedLevels[i] = false;
        }
        foreach (UnityEngine.UI.Button levelButton in levelButtons)
        {
            SpriteState spriteState = levelButton.spriteState;
            spriteState.disabledSprite = lvlLockedIcon;
            levelButton.spriteState = spriteState;
        }

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
                gameOver = true;
                _APIManager.UpdateGameScore(playerScore, "loss", lvlIndex + 1);

                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "Level_" + (lvlIndex + 1).ToString(), "Score_", 0);
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "Level_" + (lvlIndex + 1).ToString(), "Time_", (int)timeElapsed);
            }
        }

        if (!nextLevelCanvas.activeInHierarchy && !homeMenuCanvas.activeInHierarchy && !levelCanvas.activeInHierarchy && !replayCanvas.activeInHierarchy && !allBulbsFilled && !gameOver)
        {
            if (!timerIsRunning)
            {
                timerIsRunning = true;
            }
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int row = Mathf.FloorToInt(mousePos.y);
            int col = Mathf.FloorToInt(mousePos.x);
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

        if (gameOver)
        {
            gameOverCanvas.SetActive(true);
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

        if (completedLevels[lvlIndex] == true)
        {
            replayCanvas.SetActive(true);
        }

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

        if (lvlIndex >= levelsData.Count)
        {
            return;
        }

        lvlData = levelsData[lvlIndex];
        startPropsList.Clear();
        SpawnCell();
        timeElapsed = TotalGameTime;
        LevelNumber.text = "Level " + (lvlIndex + 1).ToString();

        timerIsRunning = true;

        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "Level_" + (lvlIndex + 1).ToString());

        _APIManager.StartGame();
    }

    public void ResetGameOver()
    {
        if (gameOver)
        {
            gameOver = false;
        }
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

    public void LoadNextLevel()
    {
        if (lvlIndex == (levelsData.Count - 1))
        {
            homeMenuCanvas.SetActive(true);
            return;
        }

        for (int i = 0; i < lvlData.row; i++)
        {
            for (int j = 0; j < lvlData.col; j++)
            {
                Destroy(props[i, j].gameObject);
            }
        }

        lvlIndex++;
        startPropsList.Clear();
        timeElapsed = TotalGameTime;

        LoadLevel(lvlIndex);

        LevelNumber.text = "Level " + (lvlIndex + 1).ToString();
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

        timeElapsed = TotalGameTime;
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
                tempProp.transform.position = new Vector2(j + 0.5f, i + 0.5f);
                tempProp.Initialize(lvlData.propType[i * lvlData.col + j]);
                props[i, j] = tempProp;
                if (tempProp._propType == 1)
                {
                    startPropsList.Add(tempProp);
                }
            }
        }

        float aspectRatio = (float)Screen.width / Screen.height;

        Camera.main.orthographicSize = Mathf.Max(lvlData.row, lvlData.col / aspectRatio) * 0.5f + 1f;

        Vector3 cameraPos = Camera.main.transform.position;
        cameraPos.x = (lvlData.col - 1) * gridPadding * 0.5f;
        cameraPos.y = (lvlData.row - 1) * gridPadding * 0.5f;

        Camera.main.transform.position = cameraPos;

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

        bool bulbReached = false;//new

        while (toCheck.Count > 0)
        {
            PropScript prop = toCheck.Dequeue();
            checkedFilled.Add(prop);
            List<PropScript> props = prop.connectProps();
            foreach (var _prop in props)
            {
                if (!checkedFilled.Contains(_prop))
                {
                    if (_prop._propType == 6) // Assuming propType 6 is bulb
                    {
                        bulbReached = true;
                    }//new

                    toCheck.Enqueue(_prop);
                }
            }
        }

        if (bulbReached)//new
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
            CompleteLevel();

            StartCoroutine(WaitingTime());
            //Debug.Log("Game Won! All bulbs are filled.");
        }
        else
        {
            //Debug.Log("Not all bulbs are filled.");
        }
    }

    private void CompleteLevel()
    {
        float timeUsed = 500f - timeElapsed;
        timerIsRunning = false;

        if (completedLevels[lvlIndex] == false)
        {
            // Determine score based on time
            if (timeUsed <= timeThresholds[0])
            {
                playerScore += score[2]; // Highest score for quickest completion
                _APIManager.coinsEarningLevelBased((lvlIndex + 1));
                _APIManager.UpdateGameScore(score[2], "win", lvlIndex + 1);

                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + (lvlIndex + 1).ToString(), "Score", score[2]);
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + (lvlIndex + 1).ToString(), "Time", (int)timeElapsed);
            }
            else if (timeUsed <= timeThresholds[1])
            {
                playerScore += score[1]; // Mid score for medium speed completion
                _APIManager.coinsEarningLevelBased((lvlIndex + 1));
                _APIManager.UpdateGameScore(score[1], "win", lvlIndex + 1);

                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + (lvlIndex + 1).ToString(), "Score", score[1]);
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + (lvlIndex + 1).ToString(), "Time", (int)timeElapsed);
            }
            else
            {
                playerScore += score[0]; // Lowest score for slow completion
                _APIManager.coinsEarningLevelBased((lvlIndex + 1));
                _APIManager.UpdateGameScore(score[0], "win", lvlIndex + 1);

                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + (lvlIndex + 1).ToString(), "Score", score[0]);
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_" + (lvlIndex + 1).ToString(), "Time", (int)timeElapsed);
            }

            if ((lvlIndex + 1) % GrandAdManager.instance.adsAfter == 0)
            {
                Debug.Log(lvlIndex + 1);
                GrandAdManager.instance.ShowAd("startAd");
            }
            ScoreUpdate();
        }

        if (completedLevels[lvlIndex] == false)
        {
            completedLevels[lvlIndex] = true;
        }

        if (completedLevels[completedLevels.Length - 1] == true)
        {
            allLevelsDone = true;
        }

        if (lvlIndex < (levelsData.Count - 1))
        {
            levelButtons[lvlIndex + 1].interactable = true;
        }

        if (lvlIndex < levelsData.Count)
        {
            levelButtons[lvlIndex].image.sprite = lvlCompletedIcon;
        }

        /*if (lvlIndex < levelsData.Count)
        {
            levelButtons[lvlIndex].interactable = false;

            SpriteState spriteState = levelButtons[lvlIndex].spriteState;
            spriteState.disabledSprite = lvlCompletedIcon;
            levelButtons[lvlIndex].spriteState = spriteState;

            levelButtons[lvlIndex].image.sprite = lvlCompletedIcon;
        }*/
    }

    private void ScoreUpdate()
    {
        scoreText[0].text = "Score: " + playerScore;
        scoreText[1].text = "Score: " + playerScore;
    }

    IEnumerator WaitingTime()
    {
        yield return new WaitForSeconds(2);

        nextLevelCanvas.SetActive(true);

        /*if (!allLevelsDone)
        {

        }
        else
        {
            AllLevelCompleted();
        }*/
    }

    public void AllLevelCompleted()
    {
        gameCompletedCanvas.SetActive(true);
    }

    public void PlayAgain()
    {

        allLevelsDone = false;

        for (int i = 0; i < completedLevels.Length; i++)
        {
            completedLevels[i] = false;
        }

        /*foreach (UnityEngine.UI.Button levelButton in levelButtons)
        {
            SpriteState spriteState = levelButton.spriteState;
            spriteState.disabledSprite = lvlLockedIcon;
            levelButton.spriteState = spriteState;
        }*/

        levelButtons[0].image.sprite = lvlIncompleteBtn;
        levelButtons[0].interactable = true;

        for (int i = 1; i < levelButtons.Length; i++)
        {
            levelButtons[i].image.sprite = lvlIncompleteBtn;

            levelButtons[i].interactable = false;

            SpriteState spriteState = levelButtons[i].spriteState;
            spriteState.disabledSprite = lvlLockedIcon;
            levelButtons[i].spriteState = spriteState;
        }

        playerScore = 0;
        ScoreUpdate();
    }
}