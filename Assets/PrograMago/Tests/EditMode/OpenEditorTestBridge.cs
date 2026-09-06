using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace PrograMago.Tests
{
    [InitializeOnLoad]
    public static class OpenEditorTestBridge
    {
        private const string RequestFileName = "run-editmode.request";
        private const string ResultFileName = "OpenEditorEditMode.txt";

        private static readonly string ResultDirectory = Path.Combine(
            Directory.GetParent(UnityEngine.Application.dataPath).FullName,
            "TestResults");

        private static bool isRunning;
        private static TestRunnerApi runner;
        private static ResultCallbacks callbacks;

        static OpenEditorTestBridge()
        {
            EditorApplication.update += CheckForRequest;
        }

        private static void CheckForRequest()
        {
            string requestPath = Path.Combine(ResultDirectory, RequestFileName);
            if (isRunning || !File.Exists(requestPath))
            {
                return;
            }

            Directory.CreateDirectory(ResultDirectory);
            File.Delete(requestPath);
            string resultPath = Path.Combine(ResultDirectory, ResultFileName);
            if (File.Exists(resultPath))
            {
                File.Delete(resultPath);
            }

            isRunning = true;
            runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            callbacks = new ResultCallbacks(resultPath, HandleFinished);
            runner.RegisterCallbacks(callbacks);
            runner.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode
            }));
        }

        private static void HandleFinished()
        {
            isRunning = false;
        }

        private sealed class ResultCallbacks : ICallbacks
        {
            private readonly string resultPath;
            private readonly Action finished;

            public ResultCallbacks(string resultPath, Action finished)
            {
                this.resultPath = resultPath;
                this.finished = finished;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                var lines = new List<string>
                {
                    $"Result: {result.ResultState}",
                    $"Passed: {result.PassCount}",
                    $"Failed: {result.FailCount}",
                    $"Skipped: {result.SkipCount}"
                };

                lines.AddRange(Flatten(result)
                    .Where(test => test.FailCount > 0 && !test.HasChildren)
                    .Select(test => $"FAIL {test.FullName}: {test.Message}"));
                File.WriteAllLines(resultPath, lines);
                finished();
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }

            private static IEnumerable<ITestResultAdaptor> Flatten(ITestResultAdaptor result)
            {
                yield return result;
                foreach (ITestResultAdaptor child in result.Children)
                {
                    foreach (ITestResultAdaptor descendant in Flatten(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }
}
