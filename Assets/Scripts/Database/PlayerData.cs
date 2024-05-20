using Proyecto26;
using System.Collections.Generic;
using FullSerializer;
using System.Linq;

public static class PlayerData
{
    public static fsSerializer serializer = new();

    /// <summary>
    /// Prepare data for database by checking if the data doesn't exist or if player has a 
    /// better time stored in the database than the current one
    /// </summary>
    /// <param name="boss">Name of the boss</param>
    /// <param name="playerName">Name of the player</param>
    /// <param name="fastestTime">Time needed to beat the boss</param>
    public static void PrepareForDatabase(string boss, string playerName, float fastestTime)
    {
        GetDataFromDatabase(boss, null, playerName, fastestTime, false);
    }

    /// <summary>
    /// Get the data stored in database, calls the method to see if there is a need to store the new data
    /// </summary>
    /// <param name="boss">Name of the boss</param>
    /// <param name="playerName">Name of the player</param>
    /// <param name="fastestTime">Time needed to beat the boss</param>
    /// <param name="justFetch">True if data is needed just to show the highscore, 
    /// false if it needs to compare new data with the old</param>
    public static void GetDataFromDatabase(string boss, TitleScreen titleScreen, 
        string playerName = "", float fastestTime = 0, bool justFetch = true)
    {
        RestClient.Get("https://ruined-essence-of-the-divine-default-rtdb.europe-west1.firebasedatabase.app/" + boss + ".json")
        .Then(x =>
        {
            fsData playersData = fsJsonParser.Parse(x.Text);
            Dictionary<string, Player> playerDataDictionary = new Dictionary<string, Player>();
            serializer.TryDeserialize(playersData, ref playerDataDictionary);
            if (justFetch)
            {
                SortList(playerDataDictionary, titleScreen);
            }
            else
            {
                if (playersData.IsNull)
                {
                    PostToDatabase(boss, playerName, fastestTime);
                }
                else
                {
                    bool postToDatabase = CheckIfBetterTimeExists(boss, playerName, fastestTime, playerDataDictionary);
                    if (postToDatabase)
                    {
                        PostToDatabase(boss, playerName, fastestTime);
                    }
                }
            }
        });
    }

    /// <summary>
    /// Sort list by fastest time descending
    /// </summary>
    /// <param name="playerDataDictionary"></param>
    private static void SortList(Dictionary<string, Player> playerDataDictionary, TitleScreen titleScreen)
    {
        var sortedPlayerDataDictionary = playerDataDictionary.Values.OrderBy(v => v.finishTime);
        titleScreen.FillHighScoreBoard(sortedPlayerDataDictionary);
    }

    /// <summary>
    /// Checks if the data doesn't exist or if player's current finish time is better than the one stored in database
    /// </summary>
    /// <param name="boss">Name of the boss</param>
    /// <param name="playerName">Name of the player</param>
    /// <param name="finishTime">Time needed to beat the boss</param>
    /// <param name="playerData">Data about all players and their time needed to defeat the boss</param>
    /// <returns>True if player data doesn't exist or the current time is the fastest, false if the mentioned isn't the case</returns>
    private static bool CheckIfBetterTimeExists(string boss, string playerName, float finishTime, 
        Dictionary<string, Player> playerDataDictionary)
    {
        foreach (var data in playerDataDictionary.Values)
        {
            if (data.playerName.Equals(playerName))
            {
                if (data.finishTime > finishTime 
                    || data.finishTime == 0)
                {
                    PostToDatabase(boss, playerName, finishTime);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        PostToDatabase(boss, playerName, finishTime);
        return true;
    }

    /// <summary>
    /// Save the data to Firebase
    /// </summary>
    /// <param name="boss">Name of the boss</param>
    /// <param name="playerName">Name of the player</param>
    /// <param name="fastestTime">Fastest time of the player to beat the boss</param>
    public static void PostToDatabase(string boss, string playerName, float fastestTime)
    {
        Player player = new(playerName, fastestTime);

        RestClient.Put("https://ruined-essence-of-the-divine-default-rtdb.europe-west1.firebasedatabase.app/"
            + boss + "/" + playerName + ".json", player);
    }
}
