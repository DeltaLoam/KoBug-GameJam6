using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPoint : MonoBehaviour
{
private void OnTriggerEnter2D(Collider2D collision)
{
    // Check if the object entering the trigger is the player
    if (collision.CompareTag("Player"))
    {
        Debug.Log("Player reached finish! Loading next scene...");
        
        // Call the method to go to next level
        GoToNextLevel();
    }
}

private void GoToNextLevel()
{
    // Get the current scene index
    int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    
    // Check if there is a next scene
    if (currentSceneIndex < SceneManager.sceneCountInBuildSettings - 1)
    {
        // Load the next scene
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
    else
    {
        // This was the last scene
        Debug.Log("This was the last scene!");
        // You could load a credits scene or return to main menu
    }
}
}
