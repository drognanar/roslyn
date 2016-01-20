// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Editor.Host;
using Microsoft.VisualStudio.LanguageServices.Interactive;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Interactive.Commands
{
    internal class ResetInteractiveTestStub : ResetInteractive
    {
        private IWaitIndicator _waitIndicator;

        private bool _buildSucceeds;

        internal int BuildProjectCount { get; private set; }

        internal int CancelBuildProjectCount { get; private set; }

        internal List<string> References { get; set; }

        internal List<string> ReferenceSearchPaths { get; set; }

        internal List<string> SourceSearchPaths { get; set; }

        internal List<string> NamespacesToImport { get; set; }

        internal string ProjectDirectory { get; set; }

        public ResetInteractiveTestStub(IWaitIndicator waitIndicator, Func<string, string> createReference, Func<string, string> createImport, bool buildSucceeds)
            : base(createReference, createImport)
        {
            _waitIndicator = waitIndicator;
            _buildSucceeds = buildSucceeds;
        }

        protected override void CancelBuildProject()
        {
            CancelBuildProjectCount++;
        }

        protected override Task<bool> BuildProject()
        {
            BuildProjectCount++;
            return Task.FromResult(_buildSucceeds);
        }

        protected override bool GetProjectProperties(
            out List<string> references,
            out List<string> referenceSearchPaths,
            out List<string> sourceSearchPaths,
            out List<string> namespacesToImport,
            out string projectDirectory)
        {
            references = References;
            referenceSearchPaths = ReferenceSearchPaths;
            sourceSearchPaths = SourceSearchPaths;
            namespacesToImport = NamespacesToImport;
            projectDirectory = ProjectDirectory;
            return true;
        }

        protected override IWaitIndicator GetWaitIndicator()
        {
            return _waitIndicator;
        }
    }
}
