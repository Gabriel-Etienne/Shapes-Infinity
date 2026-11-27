using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerTemporaire : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float restartDelay = 2f;

    private bool isRestarting;

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (isRestarting)
            return;

        isRestarting = true;
        StartCoroutine(RestartSceneCoroutine());
    }

    private IEnumerator RestartSceneCoroutine()
    {
        yield return new WaitForSeconds(restartDelay);

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}