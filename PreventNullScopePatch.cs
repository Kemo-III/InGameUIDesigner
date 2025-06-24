using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.GauntletUI.GamepadNavigation;

namespace InGameUIDesigner
{
    [HarmonyPatch(typeof(GauntletGamepadNavigationManager), "OnWidgetDisconnectedFromRoot")]
    public class PreventNullScopePatch 
    {
        // When changing a widget's parent, it fires an event DisconnectFromRoot that sets the scope's parent widget to null
        // However when the ConnectToRoot event is fired next, the scope's parent remains null leading to a crash
        // This patch bypasses the DisconnectFromRoot event. Hopefully that doesn't lead to any weird behaviour
        public static bool Prefix()
        {
            if (UIEditorVM.DontDisconnectScopeFromRoot) return false;
            return true;
        }
    }
}
