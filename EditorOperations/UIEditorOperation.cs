using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InGameUIDesigner.EditorOperations
{
    public abstract class UIEditorOperation
    {
        public abstract void Undo();
        public abstract UIEditorOperation GetRedoOp();
        public abstract UIEditorOperationType OperationType { get; }
    }

    public enum UIEditorOperationType
    {
        Move,
        Rescale,
        PropertyChanged,
        AddedWidget,
        RemovedWidget,
        HierarchyMove,
        HierarchyParent,
        HierarchyUnparent,
    }
}
