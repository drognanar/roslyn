' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.ComponentModel.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Editor
Imports Microsoft.CodeAnalysis.Editor.Host
Imports Microsoft.CodeAnalysis.Editor.Interactive
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.VisualStudio.InteractiveWindow
Imports Microsoft.VisualStudio.Text.Editor
Imports Microsoft.VisualStudio.Text.Operations
Imports Microsoft.VisualStudio.Utilities

Namespace Microsoft.VisualStudio.LanguageServices.VisualBasic.Interactive

    <ExportCommandHandler("Interactive Command Handler")>
    Friend NotInheritable Class VisualBasicInteractiveCommandHandler
        Inherits InteractiveCommandHandler

        Private ReadOnly _interactiveWindowProvider As VisualBasicVsInteractiveWindowProvider

        <ImportingConstructor>
        Public Sub New(
            interactiveWindowProvider As VisualBasicVsInteractiveWindowProvider,
            contentTypeRegistryService As IContentTypeRegistryService,
            editorOptionsFactoryService As IEditorOptionsFactoryService,
            editorOperationsFactoryService As IEditorOperationsFactoryService,
            waitIndicator As IWaitIndicator)

            MyBase.New(contentTypeRegistryService, editorOptionsFactoryService, editorOperationsFactoryService, waitIndicator)
            _interactiveWindowProvider = interactiveWindowProvider
        End Sub

        Protected Overrides Function CanParseSubmission(code As String) As Boolean
            ' Return True to send the direct selection.
            Return True
        End Function

        Protected Overrides Function GetExecutableSyntaxTreeNodeSelection(position As TextSpan, source As SourceText, node As SyntaxNode, model As SemanticModel) As IEnumerable(Of TextSpan)
            Return Nothing
        End Function

        Protected Overrides Function OpenInteractiveWindow(focus As Boolean) As IInteractiveWindow
            Return _interactiveWindowProvider.Open(instanceId:=0, focus:=focus).InteractiveWindow
        End Function
    End Class
End Namespace

