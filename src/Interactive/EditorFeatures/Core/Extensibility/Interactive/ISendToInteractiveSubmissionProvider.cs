using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.CodeAnalysis.Editor.Interactive
{
    internal interface ISendToInteractiveSubmissionProvider
    {
        string GetSelectedText(IEditorOptions editorOptions, CommandArgs args, CancellationToken cancellationToken);
    }
}
