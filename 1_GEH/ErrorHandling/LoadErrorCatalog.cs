using System;
using System.Data;
using System.Linq;
using UiPath.CodedWorkflows;

namespace GEHMPRTQUEUECSharp.ErrorHandling
{
    /// <summary>
    /// Betölti a projekt szintű hibakódokat (Config.xlsx / ErrorCodes lap) az ErrorCatalog-ba.
    /// </summary>
    public class LoadErrorCatalog : CodedWorkflow
    {
        private const string CodeColumn    = "Code";
        private const string MessageColumn = "Message";
        private const string RemedyColumn  = "Remedy";

        /// <summary>Validálja és betölti a projekt szintű hibakódokat.</summary>
        /// <param name="errorCodes">Az ErrorCodes lap tartalma (Code, Message, Remedy). Null vagy üres esetén csak az alapkódok élnek.</param>
        /// <returns>A katalógusban lévő kódok száma (alap + projekt).</returns>
        /// <exception cref="Exception">FW-CFG-001 – hiányzó oszlop, üres Message vagy duplikált kód esetén.</exception>
        [Workflow]
        public int Execute(DataTable errorCodes)
        {
            if (errorCodes == null || errorCodes.Rows.Count == 0)
            {
                var baseCount = ErrorCatalog.Load(null);
                Log($"[{nameof(LoadErrorCatalog)}] Nincs projekt szintű hibakód – csak az alapkódok aktívak ({baseCount} db)");
                return baseCount;
            }

            var missingColumns = new[] { CodeColumn, MessageColumn, RemedyColumn }.Where(c => !errorCodes.Columns.Contains(c)).ToList();
            if (missingColumns.Count > 0)
                throw Err.System("FW-CFG-001", $"hiányzó oszlop(ok): {string.Join(", ", missingColumns)}");

            var definitions = errorCodes.AsEnumerable()
                .Where(r => !string.IsNullOrWhiteSpace(r[CodeColumn]?.ToString()))
                .Select(r => new ErrorDefinition(r[CodeColumn].ToString().Trim(), r[MessageColumn]?.ToString().Trim(), r[RemedyColumn]?.ToString().Trim()))
                .ToList();

            var duplicates = definitions.GroupBy(d => d.Code, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Count > 0)
                throw Err.System("FW-CFG-001", $"duplikált kód(ok): {string.Join(", ", duplicates)}");

            var emptyMessages = definitions.Where(d => string.IsNullOrWhiteSpace(d.Message)).Select(d => d.Code).ToList();
            if (emptyMessages.Count > 0)
                throw Err.System("FW-CFG-001", $"üres Message: {string.Join(", ", emptyMessages)}");

            var total = ErrorCatalog.Load(definitions);
            Log($"[{nameof(LoadErrorCatalog)}] Kész — {definitions.Count} projekt kód betöltve, összesen {total} kód");
            return total;
        }
    }
}
