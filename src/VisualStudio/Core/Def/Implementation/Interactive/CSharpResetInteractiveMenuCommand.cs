// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Roslyn.VisualStudio.Services.Interactive;
using System;
using System.ComponentModel.Design;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Interactive
{
    internal sealed class CSharpResetInteractiveMenuCommand
        : AbstractResetInteractiveMenuCommand
    {
        private readonly Lazy<AbstractResetInteractiveService> _resetInteractiveService;

        public CSharpResetInteractiveMenuCommand(
            OleMenuCommandService menuCommandService,
            IVsMonitorSelection monitorSelection,
            IComponentModel componentModel)
            : base(menuCommandService, monitorSelection)
        {
            // TODO: resolve the actual CSharp class.
            _resetInteractiveService = componentModel.DefaultExportProvider.GetExport<AbstractResetInteractiveService>();
        }

        protected override string ProjectKind => VSLangProj.PrjKind.prjKindCSharpProject;

        protected override AbstractResetInteractiveService ResetInteractiveService => _resetInteractiveService.Value;

        protected override CommandID GetResetInteractiveFromProjectCommandID()
        {
            return new CommandID(CSharpInteractiveCommands.InteractiveCommandSetId, CSharpInteractiveCommands.ResetInteractiveFromProject);
        }
    }
}
