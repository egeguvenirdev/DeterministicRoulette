using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple PlayerPrefs-based save system for roulette game state.
/// </summary>
public static class GameSaveManager
{
    [System.Serializable]
    private class SavedRoundData
    {
        public int selectedNumber;
        public string selectedOutcomeLabel;
        public int resultNumber;
        public int totalWinnings;
        public int totalLosses;
    }

    [System.Serializable]
    private class SavedGameData
    {
        public int totalChips;
        public int spinsPlayed;
        public int totalWins;
        public int totalLosses;
        public int netProfit;
        public int rouletteType;
        public List<SavedRoundData> roundHistory = new List<SavedRoundData>();
    }

    // Keys
    private const string GameStateJsonKey = "GameStateJson";
    private const string ChipsKey = "Chips";
    private const string SpinsKey = "Spins";
    private const string WinsKey = "Wins";
    private const string LossesKey = "Losses";

    public static void SaveGame(GameStateData state)
    {
        if (state == null)
        {
            return;
        }

        SavedGameData payload = new SavedGameData
        {
            totalChips = state.totalChips,
            spinsPlayed = state.spinsPlayed,
            totalWins = state.totalWins,
            totalLosses = state.totalLosses,
            netProfit = state.netProfit,
            rouletteType = (int)state.rouletteType,
            roundHistory = new List<SavedRoundData>()
        };

        if (state.roundHistory != null)
        {
            for (int i = 0; i < state.roundHistory.Count; i++)
            {
                RoundResultData round = state.roundHistory[i];
                if (round == null)
                {
                    continue;
                }

                payload.roundHistory.Add(new SavedRoundData
                {
                    selectedNumber = round.selectedNumber,
                    selectedOutcomeLabel = round.selectedOutcomeLabel,
                    resultNumber = round.resultNumber,
                    totalWinnings = round.totalWinnings,
                    totalLosses = round.totalLosses
                });
            }
        }

        string json = JsonUtility.ToJson(payload);
        PlayerPrefs.SetString(GameStateJsonKey, json);
        PlayerPrefs.Save();
    }

    public static bool TryLoadGame(out GameStateData state)
    {
        state = null;

        if (PlayerPrefs.HasKey(GameStateJsonKey))
        {
            string json = PlayerPrefs.GetString(GameStateJsonKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                SavedGameData payload = JsonUtility.FromJson<SavedGameData>(json);
                if (payload != null)
                {
                    state = new GameStateData
                    {
                        totalChips = payload.totalChips,
                        spinsPlayed = payload.spinsPlayed,
                        totalWins = payload.totalWins,
                        totalLosses = payload.totalLosses,
                        netProfit = payload.netProfit,
                        rouletteType = (RouletteType)payload.rouletteType,
                        roundHistory = new List<RoundResultData>()
                    };

                    if (payload.roundHistory != null)
                    {
                        for (int i = 0; i < payload.roundHistory.Count; i++)
                        {
                            SavedRoundData savedRound = payload.roundHistory[i];
                            if (savedRound == null)
                            {
                                continue;
                            }

                            RoundResultData restoredRound = new RoundResultData
                            {
                                selectedNumber = savedRound.selectedNumber,
                                selectedOutcomeLabel = savedRound.selectedOutcomeLabel,
                                resultNumber = savedRound.resultNumber,
                                totalWinnings = savedRound.totalWinnings,
                                totalLosses = savedRound.totalLosses
                            };

                            state.roundHistory.Add(restoredRound);
                        }
                    }

                    return true;
                }
            }
        }

        if (!PlayerPrefs.HasKey(ChipsKey))
        {
            return false;
        }

        state = new GameStateData
        {
            totalChips = PlayerPrefs.GetInt(ChipsKey, 1000),
            spinsPlayed = PlayerPrefs.GetInt(SpinsKey, 0),
            totalWins = PlayerPrefs.GetInt(WinsKey, 0),
            totalLosses = PlayerPrefs.GetInt(LossesKey, 0),
            roundHistory = new List<RoundResultData>()
        };

        return true;
    }

    public static void ResetGame()
    {
        PlayerPrefs.DeleteKey(GameStateJsonKey);
        PlayerPrefs.DeleteKey(ChipsKey);
        PlayerPrefs.DeleteKey(SpinsKey);
        PlayerPrefs.DeleteKey(WinsKey);
        PlayerPrefs.DeleteKey(LossesKey);
        PlayerPrefs.Save();
    }
}
