using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] TMP_InputField playerNameInp;
    [SerializeField] Button submitPlayerNameBtn;

    [SerializeField] Button startTheGameSngBtn;
    [SerializeField] Button startTheGameMltBtn;
    [SerializeField] Button viewHighScoresBtn;
    [SerializeField] Button deleteUserBtn;
    [SerializeField] Button quitGameBtn;

    [SerializeField] Button leftArrowBtn;
    [SerializeField] Button rightArrowBtn;
    [SerializeField] Button closeHighScores;

    [SerializeField] GameObject playerNameBox;
    [SerializeField] GameObject mainMenuButtons;
    [SerializeField] GameObject highScoreBox;

    [SerializeField] Text bossNameTxt;
    [SerializeField] Text playerNamesTxt;
    [SerializeField] Text fastestTimesTxt;

    private readonly List<string> bossNamesList = new();
    private int highScoreBoss;
    private bool startingTheGame;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IncreaseMusicVolume());
        startingTheGame = false;

        bossNamesList.InsertRange(bossNamesList.Count, new string[] 
        {
            "FernBehemoth", 
            "PsychicPsycho", 
            "ChainedUndead",
            "SidusIstar", 
            "GlacialOverlord"
        });

        highScoreBoss = 0;       
        playerNameInp.text = PlayerPrefs.GetString("playerName");

        if (playerNameInp.text.Length != 0)
        {
            ClosePlayerNameBox();
        }
    }

    /// <summary>
    /// Saves player's name in PlayerPrefs
    /// </summary>
    public void SubmitPlayerName()
    {
        if (playerNameInp.text.Length == 0)
        {
            playerNameInp.text = "You must enter your name";
        }
        else
        {
            PlayerPrefs.SetInt("numberOfDefeatedBosses", 0);
            ClosePlayerNameBox();
        }     
    }

    /// <summary>
    /// Closes box for filling player's name
    /// </summary>
    private void ClosePlayerNameBox()
    {
        bossNameTxt.text = "Fastest players who have beaten the boss: Fern Behemoth";
        playerNameBox.SetActive(false);
        mainMenuButtons.SetActive(true);
    }

    /// <summary>
    /// Start the game with only one player
    /// </summary>
    public void StartTheGameSingleplayer()
    {
        if (!startingTheGame)
        {
            startingTheGame = true;
            PlayerPrefs.SetInt("noPlayers", 1);

            PrepareTheGame();
        }
    }

    /// <summary>
    /// Start the game with two players
    /// </summary>
    public void StartTheGameMultiplayer()
    {
        if (!startingTheGame)
        {
            startingTheGame = true;
            PlayerPrefs.SetInt("noPlayers", 2);

            PrepareTheGame();
        }
    }

    /// <summary>
    /// Start the game by making black screen appear, lower the general audio volume and call 
    /// the method to load the hub world scene
    /// </summary>
    private void PrepareTheGame()
    {
        GameObject.Find("BlackScreenSprite").GetComponent<Animator>().SetTrigger("Appear");

        SingletonSFX.Instance.PlaySFX("SFX6_big_thing_fly_sky");

        StartCoroutine(LowerMusicVolume());
        StartCoroutine(LoadHubWorld());
    }

    /// <summary>
    /// Load hub world scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadHubWorld()
    {
        yield return new WaitForSecondsRealtime(2f);

        SceneManager.LoadScene("HubWorld");
    }

    /// <summary>
    /// Shows high score board
    /// </summary>
    public void ShowHighScoreBoard()
    {
        highScoreBoss = -1;
        mainMenuButtons.SetActive(false);
        highScoreBox.SetActive(true);
        ShowRightHighScores();
    }

    /// <summary>
    /// Shows high scores of certain boss after clicking left arrow
    /// </summary>
    public void ShowLeftHighScores()
    {
        playerNamesTxt.text = "";
        fastestTimesTxt.text = "";

        EmptyFillHighScoreBoard(0);

        ChangeBossOrder(false);

        // Get only alphabet characters
        string bossName = Regex.Replace(bossNamesList[highScoreBoss], "([a-z])([A-Z])", "$1 $2");
        bossNameTxt.text = $"Fastest players who have beaten the boss: {bossName}";
        PlayerData.GetDataFromDatabase(bossNamesList[highScoreBoss], this);
    }

    /// <summary>
    /// Shows high scores of certain boss after clicking right arrow
    /// </summary>
    public void ShowRightHighScores()
    {
        playerNamesTxt.text = "";
        fastestTimesTxt.text = "";

        EmptyFillHighScoreBoard(0);

        ChangeBossOrder(true);
        string bossName = Regex.Replace(bossNamesList[highScoreBoss], "([a-z])([A-Z])", "$1 $2");
        bossNameTxt.text = $"Fastest players who have beaten the boss: {bossName}";
        PlayerData.GetDataFromDatabase(bossNamesList[highScoreBoss], this);
    }

    /// <summary>
    /// Changes boss order in high score board
    /// </summary>
    /// <param name="right">True if right arrow was clicked</param>
    private void ChangeBossOrder(bool right)
    {
        if (right)
        {
            highScoreBoss = highScoreBoss == 4 ? 0 : highScoreBoss + 1;
        }
        else
        {
            highScoreBoss = highScoreBoss == 0 ? 4 : highScoreBoss - 1;
        }
    }

    /// <summary>
    /// Fills high score boards with player names and fastest times
    /// </summary>
    /// <param name="playerList">List of players and fastest times</param>
    public void FillHighScoreBoard(IOrderedEnumerable<Player> playerList)
    {
        int noPlayersInTop = playerList.Count() >= 10 ? 10 : playerList.Count();

        playerNamesTxt.text = "";
        fastestTimesTxt.text = "";

        for (int i = 0; i < noPlayersInTop; i++)
        {
            playerNamesTxt.text += $"{i + 1}. " + playerList.ToArray()[i].playerName + "\n";
            fastestTimesTxt.text += TimeCountDown.GetTimeFormated(playerList.ToArray()[i].finishTime) + "\n";
        }

        EmptyFillHighScoreBoard(noPlayersInTop);
    }

    /// <summary>
    /// Fills boards with zeros and hyphens if there are less than 10 players saved in DB
    /// </summary>
    /// <param name="noPlayersInTop"></param>
    private void EmptyFillHighScoreBoard(int noPlayersInTop)
    {
        for (int i = noPlayersInTop; i < 10; i++)
        {
            playerNamesTxt.text += $"{i + 1}. ---\n";
            fastestTimesTxt.text += "00:00.000\n";
        }
    }

    /// <summary>
    /// Close high score board
    /// </summary>
    public void CloseHighScores()
    {
        mainMenuButtons.SetActive(true);
        highScoreBox.SetActive(false);
    }

    /// <summary>
    /// Deletes user data on local computer
    /// </summary>
    public void DeleteUser()
    {
        playerNameInp.text = "";
        DeletePlayerPrefsData();
        mainMenuButtons.SetActive(false);
        playerNameBox.SetActive(true);    
    }

    /// <summary>
    /// Deletes PlayerPrefs data
    /// </summary>
    private void DeletePlayerPrefsData()
    {
        PlayerPrefs.DeleteAll();
    }

    /// <summary>
    /// Quits game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Saves player's name as string in PlayerPrefs
    /// </summary>
    private void OnDisable()
    {
        PlayerPrefs.SetString("playerName", playerNameInp.text);
    }

    /// <summary>
    /// Increase the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator IncreaseMusicVolume()
    {
        while (AudioListener.volume < 1)
        {
            AudioListener.volume += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    /// <summary>
    /// Lower the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator LowerMusicVolume()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        while (AudioListener.volume > 0)
        {
            AudioListener.volume -= 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
