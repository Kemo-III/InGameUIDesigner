using System;
using System.Net;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace InGameUIDesigner
{
    public class WidgetHierarchyVM : ViewModel
    {
        [DataSourceProperty]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChangedWithValue(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public float Margin
        {
            get
            {
                return _margin;
            }
            set
            {
                if (value != _margin)
                {
                    _margin = value;
                    OnPropertyChangedWithValue(value, "Margin");
                }
            }
        }
        [DataSourceProperty]
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    OnPropertyChangedWithValue(value, "Selected");
                }
            }
        }
        [DataSourceProperty]
        public bool Locked
        {
            get
            {
                return _locked;
            }
            set
            {
                if (value != _locked)
                {
                    _locked = value;
                    OnPropertyChangedWithValue(value, "Locked");
                }
            }
        }
        [DataSourceProperty]
        public bool IsCollapsed
        {
            get
            {
                return _isCollapsed;
            }
            set
            {
                if (value != _isCollapsed)
                {
                    _isCollapsed = value;
                    OnPropertyChangedWithValue(value, "IsCollapsed");
                }
            }
        }

        public WidgetHierarchyVM(Widget widget, UIEditorVM owner)
        {
            _widget = widget;
            _owner = owner;
            IsCollapsed = false;
            _childrenCollapsed = false;
            UpdateName();
            UpdateMargin();
            Selected = widget.EditorIsSelected();
            Locked = widget.EditorIsLocked();
        }
        public void SetAsSelected()
        {
            _owner.SelectWidget(_widget);
        }

        public void LockWidget()
        {
            //_owner.LockOrUnlockWidget(_widget);
            var shouldLock = !_widget.EditorIsLocked();
            _widget.EditorSetLocked(shouldLock);
            if (!Input.IsKeyDown(InputKey.LeftShift)) return;
            foreach (var child in _widget.GetAllChildrenRecursive())
            {
                if (!child.EditorIsShownInHierarchy()) continue;
                child.EditorSetLocked(shouldLock);
                var vm = child.EditorGetWidgetHierarchyVM();
                if (vm != null) vm.Locked = shouldLock;
            }
        }

        public void ToggleCollapseChildren()
        {
            _childrenCollapsed = !_childrenCollapsed;
            foreach (var child in _widget.GetAllChildrenRecursive())
            {
                if (!child.EditorIsShownInHierarchy()) continue;
                var vm = child.EditorGetWidgetHierarchyVM();
                if (vm != null) vm.IsCollapsed = _childrenCollapsed;
            }
        }
        public void UpdateName()
        {
            var widgetTypeName = _widget.EditorIsPrefabRoot() ? _widget.EditorGetPrefabName() : _widget.GetType().Name;
            if (String.IsNullOrEmpty(_widget.Id))
                Name = widgetTypeName;
            else Name = _widget.Id + " (" + widgetTypeName + ")";
        }

        public void UpdateMargin()
        {
            var childLevel = 0;
            _owner.GetChildLevel(_widget, ref childLevel);
            Margin = 30 + 30 * childLevel;
        }

        private string _name;
        private Widget _widget;
        private float _margin;
        private bool _selected;
        private bool _locked;
        private UIEditorVM _owner;
        private bool _isCollapsed;
        private bool _childrenCollapsed;
    }
}
