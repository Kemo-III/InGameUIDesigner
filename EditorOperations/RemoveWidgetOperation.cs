extern alias MountAndBlade;
using System;
using System.Collections.Generic;
using System.Linq;
using MountAndBlade.System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.GauntletUI.BaseTypes;
using System.Reflection;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using HarmonyLib;

namespace InGameUIDesigner.EditorOperations
{
    public class RemoveWidgetOperation : UIEditorOperation
    {
        public RemoveWidgetOperation(Widget removedWidget, Widget removedWidgetParent)
        {
            _removedWidget = removedWidget;
            _removedWidgetParent = removedWidgetParent;
            _removedWidgetSiblingIndex = _removedWidget.GetSiblingIndex();
            var dependentProps = _removedWidget.EditorGetDependentProperties();
            _oldWidgetPropertyDependencies = new Dictionary<Widget, List<PropertyInfo>>();
            foreach (var keyValuePair in dependentProps)
            {
                _oldWidgetPropertyDependencies.Add(keyValuePair.Key, new List<PropertyInfo>());
                foreach (var value in keyValuePair.Value)
                {
                    _oldWidgetPropertyDependencies[keyValuePair.Key].Add(value);
                }
            }
        }

        public Widget RemovedWidget { get => _removedWidget; }
        private Widget _removedWidget;
        private Widget _removedWidgetParent;
        private int _removedWidgetSiblingIndex;
        private Dictionary<Widget, List<PropertyInfo>> _oldWidgetPropertyDependencies;
        public override UIEditorOperationType OperationType => UIEditorOperationType.RemovedWidget;
        public override void Undo()
        {
            _removedWidget.ParentWidget = _removedWidgetParent;
            _removedWidget.SetSiblingIndex(_removedWidgetSiblingIndex, true);
            var editorData = _removedWidget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            editorData.WidgetPropertyDependencies = _oldWidgetPropertyDependencies;
            UIEditorVM.Instance.UpdateWidgetHierarchy();
            UIEditorVM.Instance.SelectWidget(_removedWidget);
            if (_removedWidget is TextureWidget textureWidget)
            {
                AccessTools.Property(typeof(TextureWidget), nameof(textureWidget.TextureProvider))?.SetValue(textureWidget, null);
            }
        }

        public override UIEditorOperation GetRedoOp()
        {
            return new AddWidgetOperation(_removedWidget);
        }
    }
}
