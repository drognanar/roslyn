// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Editor.Host;
using Microsoft.CodeAnalysis.Editor.Interactive;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Interactive.Commands
{
    internal class TestInteractiveCommandHandler : InteractiveCommandHandler
    {
        private IInteractiveWindow _interactiveWindow;

        public TestInteractiveCommandHandler(
            IInteractiveWindow interactiveWindow,
            IContentTypeRegistryService contentTypeRegistryService,
            IEditorOptionsFactoryService editorOptionsFactoryService,
            IEditorOperationsFactoryService editorOperationsFactoryService,
            IWaitIndicator waitIndicator)
            : base(contentTypeRegistryService, editorOptionsFactoryService, editorOperationsFactoryService, waitIndicator)
        {
            _interactiveWindow = interactiveWindow;
        }

        protected override bool CanParseSubmission(string code)
        {
            return true;
        }

        protected override IEnumerable<TextSpan> GetExecutableSyntaxTreeNodeSelection(TextSpan position, SourceText source, SyntaxNode node, SemanticModel model)
        {
            return null;
        }

        protected override IInteractiveWindow OpenInteractiveWindow(bool focus)
        {
            return _interactiveWindow;
        }
    }
}
