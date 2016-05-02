// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.CodeAnalysis.Editor.Options;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.CodeAnalysis;
using RoslynProject = Microsoft.CodeAnalysis.Project;
using RoslynWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace Roslyn.VisualStudio.Test.Utilities.Remoting
{
    internal class WorkspaceWrapper : MarshalByRefObject
    {
        private readonly VisualStudioWorkspace _workspace;

        public static WorkspaceWrapper Create()
        {
            var visualStudioWorkspace = RemotingHelper.VisualStudioWorkspace;
            return new WorkspaceWrapper(visualStudioWorkspace);
        }

        private WorkspaceWrapper(VisualStudioWorkspace workspace)
        {
            _workspace = workspace;
        }

        public bool UseSuggestionMode
        {
            get
            {
                return Options.GetOption(EditorCompletionOptions.UseSuggestionMode);
            }

            set
            {
                if (value != UseSuggestionMode)
                {
                    RemotingHelper.DTE.ExecuteCommandAsync("Edit.ToggleCompletionMode").GetAwaiter().GetResult();
                }
            }
        }

        private OptionSet Options
        {
            get
            {
                return _workspace.Options;
            }

            set
            {
                _workspace.Options = value;
            }
        }
        
        public void AddReference(string projectName, string referenceAssemblyPath)
        {
            var metadataReference = MetadataReference.CreateFromFile(referenceAssemblyPath);
            RoslynProject newProject = GetProject(projectName).AddMetadataReference(metadataReference);

            if (!_workspace.TryApplyChanges(newProject.Solution))
            {
                throw new Exception($"Reference {referenceAssemblyPath} was not added to {projectName}");
            }
        }

        public void OpenDocument(string projectName, string documentName)
        {
            RoslynProject project = GetProject(projectName);
            DocumentId documentId = project.Documents.Single(doc => doc.Name.Equals(documentName)).Id;

            RemotingHelper.InvokeOnUIThread(() => _workspace.OpenDocument(documentId, activate: true));
        }

        private RoslynProject GetProject(string projectName)
        {
            return _workspace.CurrentSolution.Projects.Single(p => p.Name == projectName);
        }
    }
}
