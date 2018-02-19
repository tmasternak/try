﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using static Pocket.Logger<WorkspaceServer.Servers.Dotnet.DotnetWorkspaceServer>;
using Workspace = MLS.Agent.Tools.Workspace;
using OmnisharpEmitResponse = OmniSharp.Client.Commands.OmniSharpResponseMessage<OmniSharp.Client.Commands.EmitResponse>;

namespace WorkspaceServer.Servers.Dotnet
{
    public class DotnetWorkspaceServer : IWorkspaceServer, IDisposable
    {
        private readonly Workspace _workspace;
        private readonly OmniSharpServer _omniSharpServer;
        private readonly AsyncLazy<bool> _initialized;
        private bool _disposed;
        private readonly Budget _initializationBudget = new Budget();
        private readonly TimeSpan _defaultTimeoutInSeconds;

        public DotnetWorkspaceServer(
            Workspace workspace,
            int? defaultTimeoutInSeconds = 30)
        {
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

            _defaultTimeoutInSeconds = TimeSpan.FromSeconds(
                defaultTimeoutInSeconds ?? 30);

            // FIX: (DotnetWorkspaceServer) lower the verbosity for release builds
#if DEBUG
            var logToPocketLogger = true;
#else
            var logToPocketLogger = true;
#endif

            _omniSharpServer = new OmniSharpServer(
                _workspace.Directory,
                Paths.EmitPlugin,
                logToPocketLogger: logToPocketLogger);

            _initialized = new AsyncLazy<bool>(async () =>
            {
                await _workspace.EnsureBuilt(_initializationBudget);
                await _omniSharpServer.WorkspaceReady(_initializationBudget);
                return true;
            });
        }

        public async Task EnsureInitializedAndNotDisposed(Budget budget = null)
        {
            budget?.RecordEntryAndThrowIfBudgetExceeded();

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DotnetWorkspaceServer));
            }

            await _initialized.ValueAsync()
                              .CancelIfExceeds(budget ?? new Budget());
        }

        public async Task<RunResult> Run(WorkspaceRunRequest request, Budget budget = null)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                budget = budget ?? new TimeBudget(_defaultTimeoutInSeconds);

                CommandLineResult commandLineResult = null;
                Exception exception = null;
                string exceptionMessage = null;
                OmnisharpEmitResponse emitResponse = null;

                try
                {
                    emitResponse = await Emit(request, budget);

                    if (emitResponse.Body.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        return new RunResult(
                            false,
                            emitResponse.Body
                                        .Diagnostics
                                        .Where(d => d.Severity == DiagnosticSeverity.Error)
                                        .Select(e => e.ToString())
                                        .ToArray(),
                            diagnostics: emitResponse.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray());
                    }

                    var dotnet = new MLS.Agent.Tools.Dotnet(_workspace.Directory);

                    commandLineResult = await dotnet.Execute(emitResponse.Body.OutputAssemblyPath, budget);

                    if (commandLineResult.Exception != null)
                    {
                        exceptionMessage = commandLineResult.Exception.ToString();
                    }
                    else if (commandLineResult.Error.Count > 0)
                    {
                        exceptionMessage = string.Join(Environment.NewLine, commandLineResult.Error);
                    }
                }
                catch (TimeoutException timeoutException)
                {
                    exception = timeoutException;
                }
                catch (BudgetExceededException budgetExceededException)
                {
                    exception = budgetExceededException; 
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    exception = taskCanceledException;
                }

                var runResult = new RunResult(
                    succeeded: !exception.IsConsideredRunFailure(),
                    output: commandLineResult?.Output,
                    exception: exceptionMessage ?? exception.ToDisplayString(),
                    diagnostics: emitResponse?.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray());

                operation.Complete(runResult, budget);

                return runResult;
            }
        }

        private async Task<OmnisharpEmitResponse> Emit(WorkspaceRunRequest request, Budget budget)
        {
            await EnsureInitializedAndNotDisposed(budget);

            foreach (var sourceFile in request.SourceFiles)
            {
                var file = new FileInfo(Path.Combine(_workspace.Directory.FullName, sourceFile.Name));

                var text = sourceFile.Text.ToString();

                if (!file.Exists)
                {
                    File.WriteAllText(file.FullName, text);
                }

                await _omniSharpServer.UpdateBuffer(file, text);
            }

            var emitResponse = await _omniSharpServer.Emit(budget);

            budget.RecordEntryAndThrowIfBudgetExceeded();

            return emitResponse;
        }

        public Task<CompletionResult> GetCompletionList(CompletionRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<DiagnosticResult> GetDiagnostics(WorkspaceRunRequest request)
        {
            var emitResult = await Emit(request, new Budget());
            var diagnostics = emitResult.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray();
            return new DiagnosticResult(diagnostics);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _initializationBudget.Cancel();
                _omniSharpServer?.Dispose();
            }
        }
    }
}
