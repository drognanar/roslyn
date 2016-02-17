// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Roslyn.VisualStudio.Services.Interactive;
using System;
using System.ComponentModel.Design;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Interactive
{
    internal sealed class VisualBasicResetInteractiveMenuCommand
        : AbstractResetInteractiveMenuCommand
    {
        private readonly Lazy<AbstractResetInteractiveService> _resetInteractiveService;

        public VisualBasicResetInteractiveMenuCommand(
            OleMenuCommandService menuCommandService,
            IVsMonitorSelection monitorSelection,
            IComponentModel componentModel)
            : base(menuCommandService, monitorSelection)
        {
            // TODO: finish implementing the menu command.
            _resetInteractiveService = componentModel.DefaultExportProvider.GetExport<AbstractResetInteractiveService>();
        }

        protected override string ProjectKind
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override AbstractResetInteractiveService ResetInteractiveService => _resetInteractiveService.Value;

        protected override CommandID GetResetInteractiveFromProjectCommandID()
        {
            throw new NotImplementedException();
        }
    }
}
