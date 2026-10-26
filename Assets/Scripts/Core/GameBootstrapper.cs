using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private RouletteRulesDatabase rulesDatabase; // Rulet sayılarını, renklerini, bahis geçerliliğini ve ödeme oranlarını tutar.
    [SerializeField] private OutcomeSelector outcomeSelector; //Spin sonucunu seçer. Rastgele, belirli bir sayı veya bir preset.
    [SerializeField] private PayoutCalculator payoutCalculator; // Sonuca göre kazanan bahisleri ve toplam ödemeyi hesaplar.
    [SerializeField] private BetManager betManager; // Aktif bahisleri doğrular, tutar ve yönetir.
    [SerializeField] private StatisticsManager statisticsManager; // Bakiye, spin sayısı, kazanma/kaybetme toplamları ve tur geçmişini günceller.
    [SerializeField] private RouletteGameManager gameManager; // Tur servislerini bir araya getirir; spin’i çalıştırıp RoundCompleted olayını yayınlar.

    [Header("UI")]
    [SerializeField] private GameUIController gameUIController; //Arayüz bileşenlerini ve kullanıcı etkileşimlerini koordine eder.

    private void Start()
    {
        if (!HasRequiredReferences())
        {
            Debug.LogWarning($"[Bootstrapper] Missing references. Bootstrap skipped (manual inspector wiring mode). Missing: {GetMissingReferences()}", this);
            return;
        }

        if (!WireDependencies())
        {
            Debug.LogWarning("[Bootstrapper] Failed to wire dependencies. Bootstrap skipped (manual inspector wiring mode).", this);
            return;
        }

        gameUIController.Initialize();
    }

    private bool HasRequiredReferences()
    {
        return rulesDatabase != null &&
               outcomeSelector != null &&
               payoutCalculator != null &&
               betManager != null &&
               statisticsManager != null &&
               gameManager != null &&
               gameUIController != null;
    }

    private bool WireDependencies()
    {
        outcomeSelector.SetRulesDatabase(rulesDatabase);
        payoutCalculator.SetRulesDatabase(rulesDatabase);
        betManager.SetRulesDatabase(rulesDatabase);

        IOutcomeService outcomeService = outcomeSelector;
        IPayoutService payoutService = payoutCalculator;
        IBetService betService = betManager;
        IStatisticsService statsService = statisticsManager;

        if (outcomeService == null || payoutService == null || betService == null || statsService == null)
        {
            return false;
        }

        gameManager.Configure(outcomeService, payoutService, betService, statsService);
        return true;
    }

    private string GetMissingReferences()
    {
        System.Collections.Generic.List<string> missing = new System.Collections.Generic.List<string>();

        if (rulesDatabase == null)
        {
            missing.Add(nameof(rulesDatabase));
        }

        if (outcomeSelector == null)
        {
            missing.Add(nameof(outcomeSelector));
        }

        if (payoutCalculator == null)
        {
            missing.Add(nameof(payoutCalculator));
        }

        if (betManager == null)
        {
            missing.Add(nameof(betManager));
        }

        if (statisticsManager == null)
        {
            missing.Add(nameof(statisticsManager));
        }

        if (gameManager == null)
        {
            missing.Add(nameof(gameManager));
        }

        if (gameUIController == null)
        {
            missing.Add(nameof(gameUIController));
        }

        return string.Join(", ", missing);
    }
}