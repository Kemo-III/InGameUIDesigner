using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

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
