// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Editor.Commands;
using System;
using System.ComponentModel.Composition;

namespace Microsoft.CodeAnalysis.Editor.CSharp.Interactive
{
    /// <summary>
    /// Lazy implementation of the <see cref="IExecuteInInteractiveCommandHandler"/>.
    /// Contains a lazy reference to the actual implementation to avoid loading the dll
    /// that contains the actual implementation of the handler.
    /// </summary>
    [ExportCommandHandler("Interactive Command Handler", ContentTypeNames.CSharpContentType)]
    internal class CSharpLazyExecuteInInteractiveCommandHandler
        : ICommandHandler<ExecuteInInteractiveCommandArgs>
    {
        private readonly Lazy<IExecuteInInteractiveCommandHandler> _interactiveExecuteSelection;

        [ImportingConstructor]
        public CSharpLazyExecuteInInteractiveCommandHandler(
            [Import]Lazy<IExecuteInInteractiveCommandHandler> interactiveExecuteSelection)
        {
            _interactiveExecuteSelection = interactiveExecuteSelection;
        }

        void ICommandHandler<ExecuteInInteractiveCommandArgs>.ExecuteCommand(ExecuteInInteractiveCommandArgs args, Action nextHandler)
        {
            _interactiveExecuteSelection.Value.ExecuteCommand(args, nextHandler);
        }

        CommandState ICommandHandler<ExecuteInInteractiveCommandArgs>.GetCommandState(ExecuteInInteractiveCommandArgs args, Func<CommandState> nextHandler)
        {
            return CommandState.Available;
        }
    }
}
