extern alias MountAndBlade;
using System;
using System.Collections.Generic;
using System.Linq;
using MountAndBlade.System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.GauntletUI.BaseTypes;
using System.Reflection;
using static TaleWorlds.Core.ItemCategory;

namespace InGameUIDesigner.EditorOperations
{
    public class ChangePropertyOperation : UIEditorOperation
    {
        public ChangePropertyOperation(WidgetPropertyVM propertyVM, object oldValue)
        {
            _property = propertyVM;
            _oldValue = oldValue;
        }

        private WidgetPropertyVM _property;
        private object _oldValue;

        public override UIEditorOperationType OperationType => UIEditorOperationType.PropertyChanged;

        public override void Undo()
        {
            _property.UpdateActualPropertyByUndo(_oldValue);
        }

        public override UIEditorOperation GetRedoOp()
        {
            return new ChangePropertyOperation(_property, _property.PropertyValue);
        }
    }
}
