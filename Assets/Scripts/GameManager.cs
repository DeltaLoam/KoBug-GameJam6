using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public Text fragmentCountText;
    
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
        // หา Player ในซีน (ถ้าไม่ได้กำหนดไว้ใน Inspector)
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
        
        // ซ่อน UI ทั้งหมดยกเว้น Main Menu
        ShowMainMenu();
        
        // นับจำนวน Fragment ทั้งหมดในด่าน
        CountTotalFragments();
    }
    
    private void Update()
    {
        // ตรวจสอบการกด Escape เพื่อหยุดเกม
        if (Input.GetKeyDown(KeyCode.Escape) && !IsMainMenuActive())
        {
            TogglePause();
        }
        
        // อัปเดต UI
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
        
        if (fragmentCountText != null && player != null)
        {
            fragmentCountText.text = "Fragments: " + player.collectedFragments + " / " + totalFragmentsInLevel;
        }
    }
    
    #region Game State Management
    
    public void StartGame()
    {
        // เริ่มเกมใหม่
        SceneManager.LoadScene("GameScene"); // ชื่อซีนเกมของคุณ
        
        // แสดง UI เกม
        ShowGameplayUI();
        
        // เล่นเพลงระหว่างเล่นเกม
        PlayMusic(gameplayMusic);
        
        // เวลาดำเนินต่อ
        Time.timeScale = 1f;
        isGamePaused = false;
    }
    
    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        
        if (isGamePaused)
        {
            Time.timeScale = 0f; // หยุดเวลาเกม
            pauseMenuUI.SetActive(true);
            gameplayUI.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f; // เวลาดำเนินต่อ
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
        // เพิ่มเอฟเฟกต์ fade out (ถ้ามี)
        PlaySound(gameOverSound);
        
        // รอสักครู่ให้เสียงและเอฟเฟกต์ทำงาน
        yield return new WaitForSeconds(1.5f);
        
        // แสดงหน้า Game Over
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
        
        // รอสักครู่ให้เสียงและเอฟเฟกต์ทำงาน
        yield return new WaitForSeconds(1.5f);
        
        // แสดงหน้า Victory
        gameplayUI.SetActive(false);
        victoryUI.SetActive(true);
    }
    
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // ชื่อซีนเมนูหลักของคุณ
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
        AddScore(100); // เพิ่มคะแนนเมื่อเก็บ Fragment
        
        // ตรวจสอบว่าเก็บ Fragment ครบหรือไม่
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