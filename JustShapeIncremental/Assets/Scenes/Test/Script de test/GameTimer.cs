using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text timerText;

    private float elapsedTime;

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerDisplay();
    }

    public void StopTimer()
    {
        enabled = false;
    }

    public void StartTimer()
    {
        enabled = true;
    }
}