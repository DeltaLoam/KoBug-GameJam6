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
    public TextMeshProUGUI fragmentCountText; // Changed to TextMeshProUGUI
    
    [Header("Game State")]
    public bool isGamePaused = false;
    public int score = 0;
    public int totalFragmentsInLevel = 0;
    public int fragmentsCollected = 0;
    
    [Header("Audio")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip gameOverSound;
    public AudioClip victorySound;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Singleton setup
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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void Start()
    {
        // Find Player in scene (if not set in Inspector)
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
        
        // Hide all UI except Main Menu
        ShowMainMenu();
        
        // Count total fragments in level
        CountTotalFragments();
        
        // Initialize fragment display
        UpdateFragmentUI();
    }
    
    private void Update()
    {
        // Check for Escape to pause game
        if (Input.GetKeyDown(KeyCode.Escape) && !IsMainMenuActive())
        {
            TogglePause();
        }
        
        // Update UI
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
    
    // New method to specifically update fragment UI
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
<<<<<<< Updated upstream
        // Start new game
        SceneManager.LoadScene("GameScene"); // Your game scene name
=======
        // เริ่มเกมใหม่
        SceneManager.LoadScene("Part 1 scene 1"); // ชื่อซีนเกมของคุณ
>>>>>>> Stashed changes
        
        // Show game UI
        ShowGameplayUI();
        
        // Play game music
        PlayMusic(gameplayMusic);
        
        // Resume time
        Time.timeScale = 1f;
        isGamePaused = false;
    }
    
    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        
        if (isGamePaused)
        {
            Time.timeScale = 0f; // Pause game time
            pauseMenuUI.SetActive(true);
            gameplayUI.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f; // Resume time
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
        // Add fade out effect (if any)
        PlaySound(gameOverSound);
        
        // Wait for sounds and effects
        yield return new WaitForSeconds(1.5f);
        
        // Show Game Over screen
        gameplayUI.SetActive(false);
        gameOverUI.SetActive(true);
    }
    
    public void Victory()
    {
        StartCoroutine(VictorySequence());
    }
    
    private IEnumerator VictorySequence()
    {
        PlaySound(victorySound);
        
        // Wait for sounds and effects
        yield return new WaitForSeconds(1.5f);
        
        // Show Victory screen
        gameplayUI.SetActive(false);
        victoryUI.SetActive(true);
    }
    
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Your main menu scene name
        ShowMainMenu();
        PlayMusic(menuMusic);
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
        if (gameplayUI != null) gameplayUI.SetActive(false);
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
        AddScore(100); // Add score when collecting Fragment
        
        // Update fragment UI specifically
        UpdateFragmentUI();
        
        // Notify player about ability unlocks based on fragment count
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
        
        // Check if all fragments collected
        if (fragmentsCollected >= totalFragmentsInLevel)
        {
            Victory();
        }
    }
    
    #endregion
    
    #region Audio Management
    
    private void PlayMusic(AudioClip music)
    {
        if (audioSource != null && music != null)
        {
            audioSource.clip = music;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    private void PlaySound(AudioClip sound)
    {
        if (audioSource != null && sound != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }

    public void GoToNextLevel()
    {
        // Get the current scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        // Load the next scene
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
    #endregion
}