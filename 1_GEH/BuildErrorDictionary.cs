using System;
using System.Collections.Generic;
using System.Data;
using UiPath.CodedWorkflows;

namespace GEHMPRTQUEUECSharp._1_GEH
{
    public class BuildErrorDictionary : CodedWorkflow
    {
        [Workflow]
        public Dictionary<string, string> Execute(Exception exception, string exceptionType, string robotName = "N/A", string transactionIdentifier = "N/A", string automationName = "N/A")
        {
            if(string.IsNullOrWhiteSpace(robotName))
                robotName = "N/A";
            if(string.IsNullOrWhiteSpace(transactionIdentifier))
                transactionIdentifier = "N/A";
            if(string.IsNullOrWhiteSpace(automationName))
                automationName = "N/A";

            // Biztonságos adatkinyerés null-ellenőrzésekkel
            System.Collections.IDictionary exceptionData = exception?.Data;
            var faultedDetails = exceptionData?.Contains("FaultedDetails") == true
                ? exceptionData["FaultedDetails"]
                : null;

            var workflowFile = faultedDetails?.GetType().GetProperty("WorkflowFile")?.GetValue(faultedDetails)?.ToString();

            // A Dictionary összeállítása
            var resultDictionary = new Dictionary<string, string>()
            {
                {"{automationName}",    automationName ?? "N/A"},
                {"{result}",            exceptionType + " exception" ?? "N/A"},
                {"{reference}",         transactionIdentifier ?? "N/A"},
                {"{userName}",          Environment.UserName ?? "N/A"},
                {"{environment}",       Environment.UserDomainName ?? "N/A"},
                {"{timestamp}",         DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                {"{exceptionType}",     exception?.GetType().Name ?? "N/A"},
                {"{exceptionMessage}",  exception?.Message ?? "N/A"},
                {"{activityName}",      exceptionData?.Contains("Name") == true ? exceptionData["Name"]?.ToString() ?? "N/A" : "N/A"},
                {"{activityType}",      exceptionData?.Contains("TypeName") == true ? exceptionData["TypeName"]?.ToString() ?? "N/A" : "N/A"},
                {"{stackTrace}",        exceptionType == "Business" ? "N/A" : exception?.StackTrace ?? "N/A"},
                {"{robotName}",         robotName},
                {"{xamlName}",          workflowFile?.Replace(Environment.CurrentDirectory + "\\", "") ?? "N/A"}
            };

            return  resultDictionary;
        }
    }
}