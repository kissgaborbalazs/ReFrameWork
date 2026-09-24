using System;
using UiPath.Core;

namespace GEHMPRTQUEUECSharp.ErrorHandling
{
    /// <summary>
    /// Hibakód alapú kivétel-factory. Az üzenet és a javítási javaslat a katalógusból (ErrorCatalog) jön,
    /// a hibakód az Exception.Data["ErrorCode"] kulcson utazik tovább a GEH és a SetTransactionStatus felé.
    /// XAML Throw activity-ben: Err.Business("PRJ-BE-001", in_TransactionItem.Reference)
    /// </summary>
    public static class Err
    {
        /// <summary>Üzleti kivétel (BusinessRuleException) létrehozása hibakód alapján.</summary>
        /// <param name="code">Katalógusban szereplő hibakód.</param>
        /// <param name="args">Az üzenetsablon paraméterei.</param>
        /// <returns>A dobandó kivétel – az eredeti BusinessRuleException típussal.</returns>
        public static BusinessRuleException Business(string code, params object[] args) =>
            Tag(new BusinessRuleException(ErrorCatalog.Format(code, args)), code);

        /// <summary>Rendszerkivétel létrehozása hibakód alapján.</summary>
        /// <param name="code">Katalógusban szereplő hibakód.</param>
        /// <param name="args">Az üzenetsablon paraméterei.</param>
        /// <returns>A dobandó kivétel.</returns>
        public static Exception System(string code, params object[] args) =>
            Tag(new Exception(ErrorCatalog.Format(code, args)), code);

        /// <summary>Rendszerkivétel létrehozása hibakód alapján, az eredeti kivételt InnerException-ként megtartva.</summary>
        /// <param name="code">Katalógusban szereplő hibakód.</param>
        /// <param name="inner">Az eredeti (becsomagolt) kivétel.</param>
        /// <param name="args">Az üzenetsablon paraméterei.</param>
        /// <returns>A dobandó kivétel.</returns>
        public static Exception System(string code, Exception inner, params object[] args) =>
            Tag(new Exception(ErrorCatalog.Format(code, args), inner), code);

        private static T Tag<T>(T exception, string code) where T : Exception
        {
            exception.Data[ErrorCatalog.ErrorCodeKey] = code;
            return exception;
        }
    }
}
