using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }
    
    [Header("Player Reference")]
    public PlayerController player;
    
    [Header("UI References")]
    public GameObject mainMenuUI;
    public GameObject gameplayUI;
    public GameObject pauseMenuUI;
    public GameObject gameOverUI;
    public GameObject victoryUI;
    public Text scoreText;
    public TextMeshProUGUI fragmentCountText;
    
    [Header("Game State")]
    public bool isGamePaused = false;
    public int score = 0;
    public int totalFragmentsInLevel = 0;
    public int fragmentsCollected = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
        
        ShowMainMenu();
        CountTotalFragments();
        UpdateFragmentUI();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsMainMenuActive())
        {
            TogglePause();
        }
        
        UpdateUI();
    }
    
    private void CountTotalFragments()
    {
        totalFragmentsInLevel = FindObjectsOfType<EdgeFragment>().Length;
    }
    
    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        
        UpdateFragmentUI();
    }
    
    public void UpdateFragmentUI()
    {
        if (fragmentCountText != null && player != null)
        {
            fragmentCountText.text = "Fragments: " + player.collectedFragments + " / " + totalFragmentsInLevel;
        }
    }
    
    #region Game State Management
    
    public void StartGame()
    {
        SceneManager.LoadScene("Part 1 scene 1");
        ShowGameplayUI();
        Time.timeScale = 1f;
        isGamePaused = false;
    }
    
    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        
        if (isGamePaused)
        {
            Time.timeScale = 0f;
            pauseMenuUI.SetActive(true);
            gameplayUI.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenuUI.SetActive(false);
            gameplayUI.SetActive(true);
        }
    }
    
    public void GameOver()
    {
        StartCoroutine(GameOverSequence());
    }
    
    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(1.5f);
        gameplayUI.SetActive(false);
        gameOverUI.SetActive(true);
    }
    
    public void Victory()
    {
        StartCoroutine(VictorySequence());
    }
    
    private IEnumerator VictorySequence()
    {
        yield return new WaitForSeconds(1f);
        gameplayUI.SetActive(false);
        victoryUI.SetActive(true);
    }
    
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        ShowMainMenu();
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    #endregion
    
    #region UI Management
    
    public void ShowMainMenu()
    {
        HideAllUI();
        mainMenuUI.SetActive(true);
    }
    
    public void ShowGameplayUI()
    {
        HideAllUI();
        gameplayUI.SetActive(true);
    }
    
    public void ShowPauseMenu()
    {
        HideAllUI();
        pauseMenuUI.SetActive(true);
    }
    
    private void HideAllUI()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (gameplayUI != null) mainMenuUI.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (victoryUI != null) victoryUI.SetActive(false);
    }
    
    private bool IsMainMenuActive()
    {
        return mainMenuUI != null && mainMenuUI.activeSelf;
    }
    
    #endregion
    
    #region Score and Progress
    
    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }
    
    public void CollectFragment()
    {
        fragmentsCollected++;
        AddScore(100);
        UpdateFragmentUI();
        
        if (player != null)
        {
            if (player.collectedFragments == 1)
            {
                Debug.Log("Triangle form unlocked! Press 2 to transform.");
            }
            
            if (player.collectedFragments == 3)
            {
                Debug.Log("Double dash unlocked!");
            }
            
            if (player.collectedFragments == 5)
            {
                Debug.Log("Rectangle form unlocked! Press 3 to transform.");
            }
        }
        
        if (fragmentsCollected >= totalFragmentsInLevel)
        {
            Victory();
        }
    }
    
    #endregion
}
