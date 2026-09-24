using System;
using System.Collections.Generic;
using System.Linq;
using UiPath.Core;

namespace GEHMPRTQUEUECSharp.ErrorHandling
{
    /// <summary>
    /// Egy hibakód definíciója: a kódhoz tartozó üzenetsablon és javítási javaslat.
    /// </summary>
    /// <param name="Code">Egyedi hibakód (pl. FW-SYS-001, PRJ-BE-001).</param>
    /// <param name="Message">Üzenetsablon, string.Format helyőrzőkkel ({0}, {1} ...).</param>
    /// <param name="Remedy">Javítási javaslat – ez kerül a queue item Details mezőjébe és az e-mailbe.</param>
    public sealed record ErrorDefinition(string Code, string Message, string Remedy);

    /// <summary>
    /// Központi hibakód-katalógus. Két rétegből áll:
    /// 1) framework alapkódok (FW-*) – kódba égetve, Config nélkül is elérhetők;
    /// 2) projekt kódok (PRJ-*) – a Config.xlsx ErrorCodes lapjáról töltődnek (LoadErrorCatalog.cs).
    /// Azonos kód esetén a projekt szintű definíció felülírja az alapot.
    /// </summary>
    public static class ErrorCatalog
    {
        /// <summary>Az Exception.Data kulcsa, amely a hibakódot hordozza.</summary>
        public const string ErrorCodeKey = "ErrorCode";

        /// <summary>Kód nélküli (nem katalogizált) üzleti kivétel.</summary>
        public const string UnknownBusiness = "FW-BE-000";

        /// <summary>Kód nélküli (nem katalogizált) rendszerkivétel.</summary>
        public const string UnknownSystem = "FW-SYS-000";

        private static readonly ErrorDefinition[] BaseDefinitions =
        {
            new(UnknownBusiness, "{0}",
                "Az üzenet alapján ellenőrizd a bemeneti adatot. Javasolt a hibához projekt szintű hibakódot felvenni (Config.xlsx / ErrorCodes)."),
            new(UnknownSystem, "{0}",
                "Fejlesztői vizsgálat szükséges. A hibariport (Excel) és a képernyőkép a mellékletben található."),
            new("FW-SYS-001", "Elérte az egymást követő rendszerkivételek maximális számát: {0}",
                "Ellenőrizd a célalkalmazások elérhetőségét, majd a korábbi rendszerhibák riportjait. A folyamat leállt."),
            new("FW-CFG-001", "Hibás hibakód-katalógus a Config.xlsx ErrorCodes lapján: {0}",
                "Javítsd az ErrorCodes lapot: a Code oszlop kötelező és egyedi, a Message oszlop kötelező."),
        };

        /// <summary>Az aktuálisan érvényes (alap + projekt) katalógus.</summary>
        public static IReadOnlyDictionary<string, ErrorDefinition> Current { get; private set; } =
            BaseDefinitions.ToDictionary(d => d.Code, StringComparer.OrdinalIgnoreCase);

        /// <summary>Betölti a projekt szintű kódokat; azonos kódnál a projekt definíció nyer.</summary>
        /// <param name="projectDefinitions">A projekt szintű hibakódok.</param>
        /// <returns>A katalógusban lévő kódok száma a betöltés után.</returns>
        public static int Load(IEnumerable<ErrorDefinition> projectDefinitions)
        {
            Current = BaseDefinitions.Concat(projectDefinitions ?? Enumerable.Empty<ErrorDefinition>())
                .GroupBy(d => d.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Last(), StringComparer.OrdinalIgnoreCase);
            return Current.Count;
        }

        /// <summary>Igaz, ha a kód szerepel a katalógusban.</summary>
        /// <param name="code">A keresett hibakód.</param>
        public static bool Contains(string code) => !string.IsNullOrWhiteSpace(code) && Current.ContainsKey(code);

        /// <summary>Visszaadja a kód definícióját; ismeretlen kód esetén a nem katalogizált rendszerhiba definícióját.</summary>
        /// <param name="code">A keresett hibakód.</param>
        public static ErrorDefinition Get(string code) =>
            Contains(code) ? Current[code] : Current[UnknownSystem];

        /// <summary>A kódhoz tartozó javítási javaslat (Details mezőhöz, e-mailhez).</summary>
        /// <param name="code">A hibakód.</param>
        public static string Remedy(string code) => Get(code).Remedy;

        /// <summary>
        /// Elkészíti a "[KÓD] üzenet" formátumú szöveget. Hibás sablon vagy ismeretlen kód esetén
        /// sem dob kivételt – a hibakezelés nem hibázhat a katalógus miatt.
        /// </summary>
        /// <param name="code">A hibakód.</param>
        /// <param name="args">Az üzenetsablon paraméterei.</param>
        /// <returns>A formázott üzenet a kóddal előtagolva.</returns>
        public static string Format(string code, params object[] args)
        {
            args ??= Array.Empty<object>();
            if (!Contains(code))
                return $"[{code}] (nem katalogizált kód) {string.Join(", ", args)}".TrimEnd();

            try
            {
                return $"[{code}] {string.Format(Current[code].Message, args)}";
            }
            catch (FormatException)
            {
                return $"[{code}] {Current[code].Message}" + (args.Length > 0 ? $" | {string.Join(", ", args)}" : string.Empty);
            }
        }

        /// <summary>
        /// Meghatározza a kivétel hibakódját: az Exception.Data["ErrorCode"] értéke, ennek hiányában
        /// a kivétel típusa alapján a nem katalogizált üzleti / rendszer kód.
        /// </summary>
        /// <param name="exception">A vizsgált kivétel.</param>
        /// <returns>A hibakód; null kivételre a nem katalogizált rendszerhiba kódja.</returns>
        public static string Resolve(Exception exception) =>
            exception?.Data[ErrorCodeKey] as string
            ?? (exception is BusinessRuleException ? UnknownBusiness : UnknownSystem);

        /// <summary>
        /// Queue item Reason mezőhöz: "[KÓD] üzenet". Ha az üzenet már kóddal kezdődik, nem duplázza.
        /// </summary>
        /// <param name="code">A hibakód.</param>
        /// <param name="message">A kivétel üzenete.</param>
        public static string ToReason(string code, string message) =>
            string.IsNullOrWhiteSpace(code) || (message ?? string.Empty).StartsWith($"[{code}]")
                ? message
                : $"[{code}] {message}";
    }
}
