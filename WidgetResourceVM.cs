using System;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace InGameUIDesigner
{
    public class WidgetResourceVM : ViewModel
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

        public WidgetResourceVM(string name, UIEditorVM owner)
        {
            Name = name;
            _owner = owner;
        }
        public void SetAsSelectedResource()
        {
            _owner.SelectWidgetResource(Name);
        }

        public void Import()
        {
            _owner.ImportPrefab(Name);
        }

        private string _name;
        private Type _widgetType;
        private UIEditorVM _owner;
    }
}
