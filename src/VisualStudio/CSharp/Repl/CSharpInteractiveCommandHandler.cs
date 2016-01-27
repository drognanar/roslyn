// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Editor.Interactive;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.Editor.Host;

namespace Microsoft.VisualStudio.LanguageServices.CSharp.Interactive
{
    [ExportCommandHandler("Interactive Command Handler", ContentTypeNames.CSharpContentType)]
    internal sealed class CSharpInteractiveCommandHandler : InteractiveCommandHandler
    {
        private readonly CSharpVsInteractiveWindowProvider _interactiveWindowProvider;

        [ImportingConstructor]
        public CSharpInteractiveCommandHandler(
            CSharpVsInteractiveWindowProvider interactiveWindowProvider,
            IContentTypeRegistryService contentTypeRegistryService,
            IEditorOptionsFactoryService editorOptionsFactoryService,
            IEditorOperationsFactoryService editorOperationsFactoryService,
            IWaitIndicator waitIndicator)
            : base(contentTypeRegistryService, editorOptionsFactoryService, editorOperationsFactoryService, waitIndicator)
        {
            _interactiveWindowProvider = interactiveWindowProvider;
        }

        protected override IInteractiveWindow OpenInteractiveWindow(bool focus)
        {
            return _interactiveWindowProvider.Open(instanceId: 0, focus: focus).InteractiveWindow;
        }

        protected override SyntaxNode GetSelectedNode(CommandArgs args)
        {
            Document doc = args.SubjectBuffer.GetRelatedDocuments().FirstOrDefault();
            int caretPosition = args.TextView.Caret.Position.BufferPosition.Position;
            var cancellationToken = CancellationToken.None;
            SyntaxTree tree = doc.GetSyntaxTreeAsync(cancellationToken).WaitAndGetResult(cancellationToken);
            var token = tree.GetRoot(cancellationToken).FindToken(caretPosition);

            var node = tree.GetRoot(cancellationToken).FindNode(TextSpan.FromBounds(caretPosition, caretPosition));
            // TODO: find the top-level? statement that encapsulates the token?

            return null;
        }
    }
}
