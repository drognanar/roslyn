// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Roslyn.Test.Utilities;
using Xunit;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Editor.Host;
using System.Collections.Generic;
using System;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Interactive.Commands
{
    public class ResetInteractiveTests
    {
        private string WorkspaceXmlStr =>
@"<Workspace>
    <Project Language=""C#"" CommonReferences=""true"">
        <Document FilePath=""CSharpDocument""></Document>
    </Project>
</Workspace>";

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.Interactive)]
        public async void TestResetREPLWithProjectContext()
        {
            using (var workspace = await TestWorkspace.CreateAsync(WorkspaceXmlStr))
            {
                var expectedSubmissions = new List<string> {
                    "r:r1\r\nr:r2\r\n",
                    "i:ns1\r\ni:ns2\r\n"};
                AssertResetInteractive(workspace, buildSucceeds: true, expectedSubmissions: expectedSubmissions);

                // Test that no submissions are executed if the build fails.
                AssertResetInteractive(workspace, buildSucceeds: false, expectedSubmissions: new List<string>());
            }
        }

        private async void AssertResetInteractive(TestWorkspace workspace, bool buildSucceeds, List<string> expectedSubmissions)
        {
            InteractiveWindowTestHost testHost = new InteractiveWindowTestHost();
            List<string> executedSubmissionCalls = new List<string>();
            EventHandler<string> ExecuteSubmission = (_, code) => { executedSubmissionCalls.Add(code); };

            testHost.Evaluator.OnExecute += ExecuteSubmission;

            IWaitIndicator waitIndicator = workspace.GetService<IWaitIndicator>();

            ResetInteractiveTestStub resetInteractive = new ResetInteractiveTestStub(
                waitIndicator,
                CreateReference,
                CreateImport,
                buildSucceeds: buildSucceeds)
            {
                References = new List<string> { "r1", "r2" },
                ReferenceSearchPaths = new List<string> { "rsp1", "rsp2" },
                SourceSearchPaths = new List<string> { "ssp1", "ssp2" },
                NamespacesToImport = new List<string> { "ns1", "ns2" },
                ProjectDirectory = "pj",
            };

            await resetInteractive.Execute(testHost.Window, "Interactive C#");

            // Validate that the project was rebuilt.
            Assert.Equal(1, resetInteractive.BuildProjectCount);
            Assert.Equal(0, resetInteractive.CancelBuildProjectCount);

            AssertEx.Equal(expectedSubmissions, executedSubmissionCalls);

            testHost.Evaluator.OnExecute -= ExecuteSubmission;
        }

        private string CreateReference(string referenceName)
        {
            return $"r:{referenceName}";
        }

        private string CreateImport(string importName)
        {
            return $"i:{importName}";
        }
    }
}
