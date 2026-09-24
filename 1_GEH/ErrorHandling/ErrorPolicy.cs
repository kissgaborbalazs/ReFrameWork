using System;

namespace GEHMPRTQUEUECSharp.ErrorHandling
{
    /// <summary>
    /// Hibakezelési döntések egy helyen – hogy a XAML kifejezésekben ne legyenek beégetett számok.
    /// </summary>
    public static class ErrorPolicy
    {
        /// <summary>A Config kulcs, amely a queue Max # Retries értékét tartalmazza.</summary>
        public const string QueueMaxRetryNumberKey = "QueueMaxRetryNumber";

        /// <summary>
        /// Igaz, ha a tétel az utolsó feldolgozási kísérletnél tart (utána az Orchestrator már nem hozza létre újra).
        /// A RetryNo 0-tól számol: Max # Retries = 2 esetén a kísérletek RetryNo értéke 0, 1, 2 → a 2 az utolsó.
        /// </summary>
        /// <param name="retryNo">A queue item RetryNo értéke.</param>
        /// <param name="maxRetryConfigValue">A Config["QueueMaxRetryNumber"] értéke (lehet null vagy üres).</param>
        /// <returns>
        /// Igaz, ha retryNo &gt;= max retry. Hiányzó / érvénytelen config érték esetén a max retry 0,
        /// vagyis minden kísérlet utolsónak számít – inkább menjen ki egy értesítés feleslegesen, mint hogy elmaradjon.
        /// </returns>
        public static bool IsLastAttempt(int retryNo, object maxRetryConfigValue) =>
            retryNo >= (int.TryParse(Convert.ToString(maxRetryConfigValue)?.Trim(), out var maxRetry) && maxRetry > 0 ? maxRetry : 0);
    }
}
