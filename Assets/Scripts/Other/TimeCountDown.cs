using UnityEngine;

public class TimeCountDown : MonoBehaviour
{
    public bool countTime = false;
    private float timeValue = 0;

    // Update is called once per frame
    void Update()
    {
        if (countTime)
        {
            timeValue += Time.deltaTime;
        }
    }

    /// <summary>
    /// Get measured time from the boss battle which starts when the boss starts attacking
    /// and ends when player beats the boss
    /// </summary>
    /// <returns>Measured time in seconds</returns>
    public float GetTime()
    {
        return timeValue;
    }

    /// <summary>
    /// Get time formated as strings in minutes and seconds
    /// </summary>
    /// <returns>Time in minutes and seconds</returns>
    public static string GetTimeFormated(float timeForFormating)
    {
        float minutes = Mathf.FloorToInt(timeForFormating / 60);
        float seconds = Mathf.FloorToInt(timeForFormating % 60);
        float fraction = Mathf.FloorToInt(timeForFormating * 1000);
        fraction %= 1000;

        string formatedTime = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, fraction);
        return formatedTime;
    }
}
