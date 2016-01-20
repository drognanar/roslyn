// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;
using Roslyn.Test.Utilities;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Interactive.Commands
{
    internal class InteractiveCommandHandlerTests
    {
        private string ExampleCode1 =
@"var x = 1;
Task.Run(() => { return 1; });";

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.Interactive)]
        public void TestExecuteInInteractive()
        {
            // Tests that the command is unavailable without a selection.
            AssertUnavailableExecuteInInteractive("$$");
            AssertUnavailableExecuteInInteractive($"{ExampleCode1}$$");

            // Tests cases when code is selected and submission buffer is empty.
            AssertExecuteInInteractive(@"{|Selection:var x = 1;$$|}", "var x = 1;");
            AssertExecuteInInteractive($@"{{|Selection:{ExampleCode1}$$|}}", ExampleCode1);
            AssertExecuteInInteractive(
$@"var o = new object[] {{ 1, 2, 3 }};
Console.WriteLine(o);
{{|Selection:{ExampleCode1}$$|}}

Console.WriteLine(x);", ExampleCode1);

            // Tests that submission works with box selection.
            AssertExecuteInInteractive(
$@"some text {{|Selection:$$int x;|}} also here
text some {{|Selection:int y;|}} here also",
@"int x;
int y;");

            // Tests cases when interactive buffer was not empty before calling the command.
            // Execute in interactive clears the existing current buffer before execution.
            AssertExecuteInInteractive(
                @"{|Selection:var y = 2;$$|}",
                "var y = 2;",
                submissionBuffer: "var x = 1;");
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.Interactive)]
        public void TestCopyToInteractive()
        {
            // Tests that the command is unavailable without a selection.
            AssertUnavailableCopyToInteractive("$$");
            AssertUnavailableCopyToInteractive($"{ExampleCode1}$$");
            AssertUnavailableCopyToInteractive($"{ExampleCode1}$$", submissionBuffer: "var x = 1;");
            AssertUnavailableCopyToInteractive($"{ExampleCode1}$$", submissionBuffer: "x = 2;");

            // Tests a regular copy command.
            AssertCopyToInteractive($"{{|Selection:{ExampleCode1}$$|}}", ExampleCode1);

            // Tests cases when interactive buffer was not empty before calling the command.
            AssertCopyToInteractive(
                $"{{|Selection:{ExampleCode1}$$|}}",
                $"var x = 1;\r\n{ExampleCode1}",
                submissionBuffer: "var x = 1;");
        }

        private static void AssertUnavailableCopyToInteractive(string code, string submissionBuffer = null)
        {
            using (var workspace = InteractiveWindowCommandHandlerTestState.CreateTestState(code))
            {
                PrepareSubmissionBuffer(submissionBuffer, workspace);
                Assert.Equal(CommandState.Unavailable, workspace.GetStateForCopyToInteractive());
            }
        }

        private static void AssertCopyToInteractive(string code, string expectedBufferText, string submissionBuffer = null)
        {
            using (var workspace = InteractiveWindowCommandHandlerTestState.CreateTestState(code))
            {
                PrepareSubmissionBuffer(submissionBuffer, workspace);
                workspace.SendCopyToInteractive();
                Assert.Equal(expectedBufferText, workspace.WindowCurrentLanguageBuffer.CurrentSnapshot.GetText());
            }
        }

        private static void AssertUnavailableExecuteInInteractive(string code)
        {
            using (var workspace = InteractiveWindowCommandHandlerTestState.CreateTestState(code))
            {
                Assert.Equal(CommandState.Unavailable, workspace.GetStateForExecuteInInteractive());
            }
        }

        private static void AssertExecuteInInteractive(string code, string expectedSubmission, string submissionBuffer = null)
        {
            List<string> submissions = new List<string>();
            EventHandler<string> appendSubmission = (_, item) => { submissions.Add(item.TrimEnd()); };

            using (var workspace = InteractiveWindowCommandHandlerTestState.CreateTestState(code))
            {
                PrepareSubmissionBuffer(submissionBuffer, workspace);
                Assert.Equal(CommandState.Available, workspace.GetStateForExecuteInInteractive());

                workspace.Evaluator.OnExecute += appendSubmission;
                workspace.ExecuteInInteractive();
                AssertEx.Equal(new string[] { expectedSubmission }, submissions);
            }
        }

        private static void PrepareSubmissionBuffer(string submissionBuffer, InteractiveWindowCommandHandlerTestState workspace)
        {
            if (string.IsNullOrEmpty(submissionBuffer))
            {
                return;
            }

            workspace.WindowCurrentLanguageBuffer.Insert(0, submissionBuffer);
        }
    }
}
