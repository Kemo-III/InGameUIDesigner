using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using TaleWorlds.MountAndBlade;

namespace InGameUIDesigner
{
    public class UIEditorConsoleCommands
    {

        [CommandLineFunctionality.CommandLineArgumentFunction("open_ui_editor", "ui")]
        public static string OpenUIEditor(List<string> strings)
        {
            GameStateManager.Current.PushState(GameStateManager.Current.CreateState<UIEditorState>());
            return "Opening UI Editor";
        }
    }
}
