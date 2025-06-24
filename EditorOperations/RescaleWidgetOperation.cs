extern alias MountAndBlade;
using System;
using System.Collections.Generic;
using System.Linq;
using MountAndBlade.System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI;

namespace InGameUIDesigner.EditorOperations
{
    public class RescaleWidgetOperation : UIEditorOperation
    {
        public RescaleWidgetOperation(Widget target, Vector2 oldOffset, Vector2 oldScale, UIContext context)
        {
            _target = target;
            _oldOffset = oldOffset;
            _oldScale = oldScale;
            _context = context;
        }

        private Widget _target;
        private Vector2 _oldScale;
        private Vector2 _oldOffset;
        private UIContext _context;
        public override UIEditorOperationType OperationType => UIEditorOperationType.Rescale;

        public override void Undo()
        {
            _target.SuggestedWidth = _oldScale.X * _context.InverseScale;
            _target.SuggestedHeight = _oldScale.Y * _context.InverseScale;
            _target.PosOffset = _oldOffset;
            UIEditorVM.Instance.SelectWidget(_target);
        }

        public override UIEditorOperation GetRedoOp()
        {
            return new RescaleWidgetOperation(_target, _target.PosOffset, _target.Size, _context);
        }
    }
}
