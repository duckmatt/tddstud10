using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using MsVsShell = Microsoft.VisualStudio.Shell;

#if DONT_COMPILE

Wave 1 - Wireup infra
v Click

v Workspace 
  v Extract
    v [a] build order graph w/ GUID
    v [b] list of projects, dependencies, files, refs, projfile
  v Make it work for xunit
  v Fire Workspace Created

v SolutionSnapshot 
  v handles Workspace Created
    v Fire Creating snapshot
    v Graph looper
      v Fire snapshoting proj
      v Fire shapshoted proj
    v Fire Created snapshot

v UX
  v Click - changes to Loading
  v Handles Creating snapshot
    v Shows the list of projects, in tree view
  v Handles snapshoting proj 
  v Handles snapshoted proj
  v Handles Created snapshot - change to loaded

- File Copy
  - make UI responsive by bringing in asyncness
  - start stop project level marquee
  - Copy files

Wave 2 - make buildable
- Extract Sequential graph looper
- Trigger MSBuild
  - Show failure in UX  
- Edit proj file to get build outputs
- Edit proj file to replace proj ref with proj output

Wave 3 - keep buildable [p0 cases]
- Parallel graph looper
  - Error handling

Wave 4 - keep buildable [p1 cases]
- Unloaded projects
- Projects that opt out of TddStud10













- Initialize
  v Fix OM - With project build order in mind
  - Solution Event Listener
    - Solution Load
    - Disable/Enable Button
  - Pull Workspace
  - Projec Build Order

- TreeView Style

- Incremental Changes
  - Solution Event Listener
  - Project Event Listener
  - File Event Listener
  - TextView Event Listener

- Remaining Pipeline
  - TB Update should come from pipeline
  - ?Merge Events
  - Copy workspace
  - Reference Files
  - Additional Files
  - Prioritize the Sinks - i.e. next project to build should be the project with largest dependencies

#endif

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    [MsVsShell.ProvideToolWindow(typeof(WorkspacePane), Style = MsVsShell.VsDockStyle.Tabbed, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
    [MsVsShell.ProvideMenuResource(1000, 1)]
    [MsVsShell.PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid("01069CDD-95CE-4620-AC21-DDFF6C57F012")]
    public class PackageToolWindow : MsVsShell.Package
    {
        private MsVsShell.OleMenuCommandService menuService;

        protected override void Initialize()
        {
            base.Initialize();

            var id = new CommandID(GuidsList.guidClientCmdSet, PkgCmdId.cmdidUiLoadWorkspace);
            DefineCommandHandler(new EventHandler(this.LoadWorkspace), id);

            id = new CommandID(GuidsList.guidClientCmdSet, PkgCmdId.cmdidUiEventsWindow);
            DefineCommandHandler(new EventHandler(this.ShowDynamicWindow), id);
        }

        internal MsVsShell.OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
        {
            if (this.Zombied)
                return null;

            if (menuService == null)
            {
                menuService = GetService(typeof(IMenuCommandService)) as MsVsShell.OleMenuCommandService;
            }
            MsVsShell.OleMenuCommand command = null;
            if (null != menuService)
            {
                command = new MsVsShell.OleMenuCommand(handler, id);
                menuService.AddCommand(command);
            }
            return command;
        }

        internal string GetResourceString(string resourceName)
        {
            string resourceValue;
            IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            Guid packageGuid = this.GetType().GUID;
            int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }

        private void ShowDynamicWindow(object sender, EventArgs arguments)
        {
            MsVsShell.ToolWindowPane pane = this.FindToolWindow(typeof(WorkspacePane), 0, true);
            if (pane == null)
            {
                throw new COMException(this.GetResourceString("@101"));
            }
            IVsWindowFrame frame = pane.Frame as IVsWindowFrame;
            if (frame == null)
            {
                throw new COMException(this.GetResourceString("@102"));
            }

            ErrorHandler.ThrowOnFailure(frame.Show());
        }

        private void LoadWorkspace(object sender, EventArgs arguments)
        {
        }
    }
}