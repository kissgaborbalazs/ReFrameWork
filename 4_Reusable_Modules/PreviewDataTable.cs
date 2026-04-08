using System;
using System.Data;
using System.Linq;
using UiPath.CodedWorkflows;

namespace GEHMPRTQUEUECSharp._4_Reusable_Modules
{
    /// <summary>
    /// Diagnosztikai célra logolja a DataTable első N sorát fix-szélességű oszlopokkal.
    /// </summary>
    public class LogDataTablePreview : CodedWorkflow
    {
        private const int MaxColWidth        = 30;
        private const string NullPlaceholder = "<null>";
        private const string Separator       = " | ";

        /// <summary>Logolja a DataTable első <paramref name="rowCount"/> sorát igazított oszlopokkal.</summary>
        /// <param name="dt">A megjelenítendő adattábla.</param>
        /// <param name="label">Azonosító felirat a log sorokban.</param>
        /// <param name="rowCount">Megjelenítendő sorok száma (default: 5).</param>
        /// <exception cref="ArgumentNullException">Ha dt null.</exception>
        [Workflow]
        public void Execute(DataTable dt, string label = "Preview", int rowCount = 5)
        {
            ArgumentNullException.ThrowIfNull(dt);

            rowCount = rowCount < 1 ? 5 : rowCount;
            var effectiveCount = Math.Min(rowCount, dt.Rows.Count);
            var columns        = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            var previewRows    = dt.AsEnumerable().Take(effectiveCount).ToList();

            // Oszloponként a maximális szélesség (fejléc + adatok), MaxColWidth-re levágva
            var widths = columns.Select(col =>
                Math.Min(MaxColWidth,
                    previewRows
                        .Select(r => (r[col]?.ToString() ?? NullPlaceholder).Length)
                        .Append(col.Length)
                        .Max())
            ).ToArray();

            var header    = string.Join(Separator, columns.Select((c, i) => Truncate(c, widths[i]).PadRight(widths[i])));
            var divider   = string.Join(Separator, widths.Select(w => new string('-', w)));

            Log($"[{label}] {dt.Rows.Count} sor / {columns.Length} oszlop — első {effectiveCount} sor:");
            Log($"[{label}] {header}");
            Log($"[{label}] {divider}");

            foreach (var row in previewRows)
            {
                var line = string.Join(Separator,
                    columns.Select((c, i) => Truncate(row[c]?.ToString() ?? NullPlaceholder, widths[i]).PadRight(widths[i])));
                Log($"[{label}] {line}");
            }

            Log($"[{label}] {divider}");
        }

        private static string Truncate(string value, int maxWidth) =>
            value.Length <= maxWidth ? value : value[..(maxWidth - 1)] + "…";
    }
}