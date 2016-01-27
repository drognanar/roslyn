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

        /// <summary>
        /// Returns spans selected to send to interactive.
        /// </summary>
        /// <returns>If the selection is non empty returns the selected spans.
        /// Otherwise returns the currently selected line.</returns>
        private IEnumerable<SnapshotSpan> GetSelectedSpans(CommandArgs args)
        {
            IEnumerable<SnapshotSpan> selectedSpans = args.TextView.Selection.GetSnapshotSpansOnBuffer(args.SubjectBuffer).Where(ss => ss.Length > 0);
            return selectedSpans.Any()
                ? selectedSpans
                : GetSnapshotSpanForCurrentLine(args);
        }

        protected abstract SyntaxNode GetSelectedNode(CommandArgs args);

        private async Task<IEnumerable<SyntaxNode>> GetMyNode(CommandArgs args, ITextSnapshotLine containingLine, CancellationToken cancellationToken)
        {
            Document doc = args.SubjectBuffer.GetRelatedDocuments().FirstOrDefault();
            int caretPosition = args.TextView.Caret.Position.BufferPosition.Position;
            var semanticDocument = await SemanticDocument.CreateAsync(doc, cancellationToken).ConfigureAwait(false);
            var text = semanticDocument.Text;
            var root = semanticDocument.Root;
            var model = semanticDocument.SemanticModel;


            SyntaxTree tree = await doc.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            return tree.GetRoot(cancellationToken).ChildNodes()
                .Where(n => n.Span.OverlapsWith(TextSpan.FromBounds(containingLine.Start.Position, containingLine.End.Position)));
        }

        private async Task<IEnumerable<SnapshotSpan>> GetSnapshotSpanForCurrentLineAsync()
        {
            return null;
        }

        private IEnumerable<SnapshotSpan> GetSnapshotSpanForCurrentLine(CommandArgs args)
        {
            SnapshotPoint? caret = args.TextView.GetCaretPoint(args.SubjectBuffer);
            ITextSnapshotLine containingLine = caret.Value.GetContainingLine();
            var cancellationToken = CancellationToken.None;
            IEnumerable<SyntaxNode> node = GetMyNode(args, containingLine, cancellationToken).WaitAndGetResult(cancellationToken);
            
            if (node.Any())
            {
                return node.Select(n => new SnapshotSpan(containingLine.Snapshot, n.SpanStart, n.Span.Length));
            }

            return new SnapshotSpan[] {
                new SnapshotSpan(containingLine.Start, containingLine.End)
            };
        }

        private string GetSelectedText(CommandArgs args)
        {
            var editorOptions = _editorOptionsFactoryService.GetOptions(args.SubjectBuffer);

            // If we have multiple selections, that's probably a box-selection scenario.
            // So let's join the text together with newlines
            return string.Join(editorOptions.GetNewLineCharacter(), GetSelectedSpans(args).Select(ss => ss.GetText()));
        }

        CommandState ICommandHandler<ExecuteInInteractiveCommandArgs>.GetCommandState(ExecuteInInteractiveCommandArgs args, Func<CommandState> nextHandler)
        {
            return CommandState.Available;
        }

        void ICommandHandler<ExecuteInInteractiveCommandArgs>.ExecuteCommand(ExecuteInInteractiveCommandArgs args, Action nextHandler)
        {
            var window = OpenInteractiveWindow(focus: false);
            window.SubmitAsync(new[] { GetSelectedText(args) });
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
                var text = GetSelectedText(args);

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
            }

            // Move the caret to the end
            var editorOperations = _editorOperationsFactoryService.GetEditorOperations(window.TextView);
            var endPoint = new VirtualSnapshotPoint(window.TextView.TextBuffer.CurrentSnapshot, window.TextView.TextBuffer.CurrentSnapshot.Length);
            editorOperations.SelectAndMoveCaret(endPoint, endPoint);
        }
    }
}
