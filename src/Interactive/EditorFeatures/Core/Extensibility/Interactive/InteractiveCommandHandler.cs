// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Editor.Commands;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.Editor.Host;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Editor.Interactive
{
    internal abstract class InteractiveCommandHandler :
        ICommandHandler<ExecuteInInteractiveCommandArgs>,
        ICommandHandler<CopyToInteractiveCommandArgs>
    {
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private readonly IEditorOptionsFactoryService _editorOptionsFactoryService;
        private readonly IEditorOperationsFactoryService _editorOperationsFactoryService;
        private readonly IWaitIndicator _waitIndicator;

        protected InteractiveCommandHandler(
            IContentTypeRegistryService contentTypeRegistryService,
            IEditorOptionsFactoryService editorOptionsFactoryService,
            IEditorOperationsFactoryService editorOperationsFactoryService,
            IWaitIndicator waitIndicator)
        {
            _contentTypeRegistryService = contentTypeRegistryService;
            _editorOptionsFactoryService = editorOptionsFactoryService;
            _editorOperationsFactoryService = editorOperationsFactoryService;
            _waitIndicator = waitIndicator;
        }

        protected IContentTypeRegistryService ContentTypeRegistryService { get { return _contentTypeRegistryService; } }

        protected abstract IInteractiveWindow OpenInteractiveWindow(bool focus);

        protected abstract IEnumerable<TextSpan> GetExecutableSyntaxTreeNodeSelection(TextSpan position, SourceText source, SyntaxNode node, SemanticModel model);

        /// <summary>Returns whether the submission can be parsed in interactive.</summary>
        protected abstract bool CanParseSubmission(string code);

        /// <summary>Returns the span for the currently selected line.</summary>
        private static IEnumerable<SnapshotSpan> GetSelectedLine(CommandArgs args)
        {
            SnapshotPoint? caret = args.TextView.GetCaretPoint(args.SubjectBuffer);
            int caretPosition = args.TextView.Caret.Position.BufferPosition.Position;
            ITextSnapshotLine containingLine = caret.Value.GetContainingLine();
            return new SnapshotSpan[] {
                new SnapshotSpan(containingLine.Start, containingLine.End)
            };
        }

        private async Task<IEnumerable<SnapshotSpan>> GetExecutableSyntaxTreeNodeSelection(
            TextSpan selectionSpan,
            CommandArgs args,
            ITextSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            Document doc = args.SubjectBuffer.GetRelatedDocuments().FirstOrDefault();
            var semanticDocument = await SemanticDocument.CreateAsync(doc, cancellationToken).ConfigureAwait(false);
            var text = semanticDocument.Text;
            var root = semanticDocument.Root;
            var model = semanticDocument.SemanticModel;

            return GetExecutableSyntaxTreeNodeSelection(selectionSpan, text, root, model)
                .Select(span => new SnapshotSpan(snapshot, span.Start, span.Length));
        }

        private IEnumerable<SnapshotSpan> ExpandSelection(IEnumerable<SnapshotSpan> selectedSpans, CommandArgs args, CancellationToken cancellationToken)
        {
            var selectedSpansStart = selectedSpans.Min(span => span.Start);
            var selectedSpansEnd = selectedSpans.Max(span => span.End);
            ITextSnapshot snapshot = args.TextView.TextSnapshot;

            IEnumerable<SnapshotSpan> newSpans = GetExecutableSyntaxTreeNodeSelection(
                TextSpan.FromBounds(selectedSpansStart, selectedSpansEnd),
                args,
                snapshot,
                cancellationToken).WaitAndGetResult(cancellationToken);

            return newSpans.Any()
                ? newSpans.Select(n => new SnapshotSpan(snapshot, n.Span.Start, n.Span.Length))
                : selectedSpans;
        }

        private string GetSelectedText(CommandArgs args, CancellationToken cancellationToken)
        {
            var editorOptions = _editorOptionsFactoryService.GetOptions(args.SubjectBuffer);
            IEnumerable<SnapshotSpan> selectedSpans = args.TextView.Selection.GetSnapshotSpansOnBuffer(args.SubjectBuffer).Where(ss => ss.Length > 0);

            // If there is no selection select the current line.
            if (!selectedSpans.Any())
            {
                selectedSpans = GetSelectedLine(args);
            }

            // Send the selection as is if it does not contain any parsing errors.
            var candidateSubmission = GetSubmissionFromSelectedSpans(editorOptions, selectedSpans);
            if (CanParseSubmission(candidateSubmission))
            {
                return candidateSubmission;
            }

            // Otherwise heuristically try to expand it.
            return GetSubmissionFromSelectedSpans(editorOptions, ExpandSelection(selectedSpans, args, cancellationToken));
        }

        private static string GetSubmissionFromSelectedSpans(IEditorOptions editorOptions, IEnumerable<SnapshotSpan> selectedSpans)
        {
            return string.Join(editorOptions.GetNewLineCharacter(), selectedSpans.Select(ss => ss.GetText()));
        }

        CommandState ICommandHandler<ExecuteInInteractiveCommandArgs>.GetCommandState(ExecuteInInteractiveCommandArgs args, Func<CommandState> nextHandler)
        {
            return CommandState.Available;
        }

        void ICommandHandler<ExecuteInInteractiveCommandArgs>.ExecuteCommand(ExecuteInInteractiveCommandArgs args, Action nextHandler)
        {
            var window = OpenInteractiveWindow(focus: false);
            _waitIndicator.Wait("execute executein interactive", true, (context) =>
            {
                window.SubmitAsync(new[] { GetSelectedText(args, context.CancellationToken) });
            });
        }

        CommandState ICommandHandler<CopyToInteractiveCommandArgs>.GetCommandState(CopyToInteractiveCommandArgs args, Func<CommandState> nextHandler)
        {
            return CommandState.Available;
        }

        void ICommandHandler<CopyToInteractiveCommandArgs>.ExecuteCommand(CopyToInteractiveCommandArgs args, Action nextHandler)
        {
            var window = OpenInteractiveWindow(focus: true);
            var buffer = window.CurrentLanguageBuffer;

            if (buffer != null)
            {
                CopyToWindow(window, args);
            }
            else
            {
                Action action = null;
                action = new Action(() =>
                {
                    window.ReadyForInput -= action;
                    CopyToWindow(window, args);
                });

                window.ReadyForInput += action;
            }
        }

        private void CopyToWindow(IInteractiveWindow window, CopyToInteractiveCommandArgs args)
        {
            var buffer = window.CurrentLanguageBuffer;
            Debug.Assert(buffer != null);

            using (var edit = buffer.CreateEdit())
            {
                _waitIndicator.Wait("execute copy to window", true, (context) => {
                    var text = GetSelectedText(args, context.CancellationToken);

                    // If the last line isn't empty in the existing submission buffer, we will prepend a
                    // newline
                    var lastLine = buffer.CurrentSnapshot.GetLineFromLineNumber(buffer.CurrentSnapshot.LineCount - 1);
                    if (lastLine.Extent.Length > 0)
                    {
                        var editorOptions = _editorOptionsFactoryService.GetOptions(args.SubjectBuffer);
                        text = editorOptions.GetNewLineCharacter() + text;
                    }

                    edit.Insert(buffer.CurrentSnapshot.Length, text);
                    edit.Apply();
                });
            }

            // Move the caret to the end
            var editorOperations = _editorOperationsFactoryService.GetEditorOperations(window.TextView);
            var endPoint = new VirtualSnapshotPoint(window.TextView.TextBuffer.CurrentSnapshot, window.TextView.TextBuffer.CurrentSnapshot.Length);
            editorOperations.SelectAndMoveCaret(endPoint, endPoint);
        }
    }
}
