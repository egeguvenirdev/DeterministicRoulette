using System.Collections.Generic;
using UnityEngine;

public class BetManager : MonoBehaviour
{
    private readonly List<BetData> activeBets = new List<BetData>();

    public IReadOnlyList<BetData> ActiveBets => activeBets;

    public bool TryAddBet(BetData bet)
    {
        if (bet == null)
        {
            return false;
        }

        if (RouletteRulesDatabase.Instance == null || !RouletteRulesDatabase.Instance.IsValidBet(bet))
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