using System;

namespace ModRewriter.Console
{
    public class ActionableReporter<T> : IProgress<T>
    {
        public delegate void DoReport(T value);

        public event DoReport? OnReport;

        public void Report(T value) => OnReport?.Invoke(value);
    }
}