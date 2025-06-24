extern alias MountAndBlade;
using System;
using System.Collections.Generic;
using System.Linq;
using MountAndBlade.System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.GauntletUI.BaseTypes;

namespace InGameUIDesigner.EditorOperations
{
    public class MoveWidgetOperation : UIEditorOperation
    {
        public MoveWidgetOperation(Widget target, Vector2 oldOffset)
        {
            _target = target;
            _oldOffset = oldOffset;
        }

        private Widget _target;
        private Vector2 _oldOffset;

        public override UIEditorOperationType OperationType => UIEditorOperationType.Move;

        public override void Undo()
        {
            _target.PosOffset = _oldOffset;
            UIEditorVM.Instance.SelectWidget(_target);
        }

        public override UIEditorOperation GetRedoOp()
        {
            return new MoveWidgetOperation(_target, _target.PosOffset);
        }
    }
}
