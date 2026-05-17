using System.Collections.Generic;
using UnityEngine;

public class BetManager : MonoBehaviour, IBetService
{
    private readonly List<BetData> activeBets = new List<BetData>();
    private RouletteRulesDatabase rulesDatabase;

    public IReadOnlyList<BetData> ActiveBets => activeBets;

    public void SetRulesDatabase(RouletteRulesDatabase db)
    {
        if (db == null)
        {
            Debug.LogWarning("[BetManager] Attempted to set null rules database.", this);
            return;
        }
        rulesDatabase = db;
    }

    public bool TryAddBet(BetData bet)
    {
        if (bet == null)
        {
            return false;
        }

        if (rulesDatabase == null)
        {
            Debug.LogError("[BetManager] Rules database not initialized. Call SetRulesDatabase first.", this);
            return false;
        }

        if (!rulesDatabase.IsValidBet(bet))
        {
            return false;
        }

        activeBets.Add(bet);
        return true;
    }

    public bool RemoveBet(BetData bet)
    {
        return activeBets.Remove(bet);
    }

    public void ClearBets()
    {
        activeBets.Clear();
    }

    public int GetTotalStake()
    {
        int totalStake = 0;

        for (int i = 0; i < activeBets.Count; i++)
        {
            totalStake += activeBets[i].amount;
        }

        return totalStake;
    }

    public List<BetData> GetBetSnapshot()
    {
        return new List<BetData>(activeBets);
    }
}