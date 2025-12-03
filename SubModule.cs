using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace InGameUIDesigner
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("InGameUIDesigner").PatchAll();
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("OpenUIEditor",
                new TextObject("Open UI Editor"), 5, OpenEditor, () => (false, TextObject.GetEmpty())));
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        private void OpenEditor()
        {
            UIEditorConsoleCommands.OpenUIEditor(null);
        }
    }
}