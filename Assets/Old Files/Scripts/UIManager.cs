using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI levelText;

    [SerializeField]private TextMeshProUGUI levelWonText;

    [SerializeField]private GameObject nextLevelUI;

    private void Awake()
    {
        nextLevelUI.SetActive(false);
        levelWonText.gameObject.SetActive(false);
        levelText.text = levelText.text + SceneManager.GetActiveScene().name;
    }

    public void LevelCompleted()
    {
        levelWonText.gameObject.SetActive(true);

        StartCoroutine(FadeInText());
    }

    IEnumerator WaitingTime()
    {
        yield return new WaitForSeconds(2);
        nextLevelUI.SetActive(true);
    }

    private IEnumerator FadeInText()
    {
        float duration = 2f; // Duration for the fade
        float t = 0f; // Time variable

        // Set initial color with alpha = 0
        Color startColor = new Color(levelWonText.color.r, levelWonText.color.g, levelWonText.color.b, 0);
        Color endColor = new Color(levelWonText.color.r, levelWonText.color.g, levelWonText.color.b, 1);

        levelWonText.color = startColor; // Set initial color

        while (t < duration)
        {
            t += Time.deltaTime; // Increment time
            levelWonText.color = Color.Lerp(startColor, endColor, t / duration); // Lerp color
            yield return null; // Wait for next frame
        }

        levelWonText.color = endColor; // Ensure final color is set
        StartCoroutine(WaitingTime());
    }

    public void LoadNextLevel()
    {
        
        int currentLevel = int.Parse(SceneManager.GetActiveScene().name);

        int nextLevel = currentLevel + 1;

        if (nextLevel < 4)
        {
            SceneManager.LoadScene(nextLevel);
        }
        else
        {
            SceneManager.LoadScene("GameStartScreen");
        }
    }
}

