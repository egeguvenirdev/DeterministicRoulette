using System.Collections.Generic;

public interface IOutcomeService
{
    void SetSelectedNumber(int number);
    void ClearSelection();
    int GetOutcome();
    int GetSelectedNumber();
    bool HasSelection();
}

public interface IBetService
{
    IReadOnlyList<BetData> ActiveBets { get; }
    bool TryAddBet(BetData bet);
    bool RemoveBet(BetData bet);
    void ClearBets();
    int GetTotalStake();
    List<BetData> GetBetSnapshot();
}

public interface IPayoutService
{
    int CalculateWinnings(int resultNumber, List<BetData> bets);
    List<BetData> GetWinningBets(int resultNumber, List<BetData> bets);
}

public interface IStatisticsService
{
    GameStateData CurrentState { get; }
    bool CanAfford(int amount);
    void ApplyRound(RoundResultData roundResult, int totalStake);
}
