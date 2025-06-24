using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using TaleWorlds.TwoDimension;
using Color = TaleWorlds.Library.Color;
using Brush = TaleWorlds.GauntletUI.Brush;
using TaleWorlds.MountAndBlade;
using System.Dynamic;
using System.Runtime.InteropServices;
using InGameUIDesigner.EditorOperations;

namespace InGameUIDesigner
{
    public class WidgetPropertyVM : ViewModel
    {
        [DataSourceProperty]
        public string DisplayedPropertyName
        {
            get => _displayedPropertyName;
            set
            {
                if (value != _displayedPropertyName)
                {
                    _displayedPropertyName = value;
                    OnPropertyChangedWithValue(value, "DisplayedPropertyName");
                }
            }
        }
        [DataSourceProperty]
        public string TextboxValue
        {
            get => _textboxValue;
            set
            {
                if (value != _textboxValue)
                {
                    _textboxValue = value;
                    if (_shouldUpdateActualProperty)
                    {
                        var type = _usesPrefabParameters ? _parameterData.FirstOrDefault().Item2.PropertyType : _property.PropertyType;
                        UpdateActualProperty(CastStringToValue(value, type, _widget));
                    }
                    OnPropertyChangedWithValue(value, "TextboxValue");
                }
                _shouldUpdateActualProperty = true;
            }
        }
        [DataSourceProperty]
        public bool UsesTextBox
        {
            get => _usesTextBox;
            set
            {
                if (value != _usesTextBox)
                {
                    _usesTextBox = value;
                    UsesDropDownMenu = false;
                    UsesToggleButton = false;
                    UsesWidgetPicker = false;
                    UsesColourPicker = false;
                    OnPropertyChangedWithValue(value, "UsesTextBox");
                }
            }
        }
        [DataSourceProperty]
        public bool UsesToggleButton
        {
            get => _usesToggleButton;
            set
            {
                if (value != _usesToggleButton)
                {
                    _usesToggleButton = value;
                    UsesTextBox = false;
                    UsesDropDownMenu = false;
                    UsesWidgetPicker = false;
                    UsesColourPicker = false;
                    OnPropertyChangedWithValue(value, "UsesToggleButton");
                }
            }
        }
        [DataSourceProperty]
        public bool ToggleButtonValue
        {
            get => _toggleButtonValue;
            set
            {
                if (value != _toggleButtonValue)
                {
                    _toggleButtonValue = value;
                    OnPropertyChangedWithValue(value, "ToggleButtonValue");
                }
            }
        }
        [DataSourceProperty]
        public bool UsesDropDownMenu
        {
            get => _usesDropDownMenu;
            set
            {
                if (value != _usesDropDownMenu)
                {
                    _usesDropDownMenu = value;
                    UsesToggleButton = false;
                    UsesTextBox = false;
                    UsesWidgetPicker = false;
                    UsesColourPicker = false;
                    OnPropertyChangedWithValue(value, "UsesDropDownMenu");
                }
            }
        }
        [DataSourceProperty]
        public bool UsesWidgetPicker
        {
            get => _usesWidgetPicker;
            set
            {
                if (value != _usesWidgetPicker)
                {
                    _usesWidgetPicker = value;
                    UsesToggleButton = false;
                    UsesTextBox = false;
                    UsesDropDownMenu = false;
                    UsesColourPicker = false;
                    OnPropertyChangedWithValue(value, "UsesWidgetPicker");
                }
            }
        }
        [DataSourceProperty]
        public bool UsesColourPicker
        {
            get => _usesColourPicker;
            set
            {
                if (value != _usesColourPicker)
                {
                    _usesColourPicker = value;
                    UsesToggleButton = false;
                    UsesTextBox = false;
                    UsesDropDownMenu = false;
                    OnPropertyChangedWithValue(value, "UsesColourPicker");
                }
            }
        }
        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> EnumSelector
        {
            get => _enumSelector;
            set
            {
                if (value != _enumSelector)
                {
                    _enumSelector = value;
                    OnPropertyChangedWithValue(value, "EnumSelector");
                }
            }
        }
        public object PropertyValue 
        { 
            get
            {
                if (_usesPrefabParameters) return _widget.EditorGetParameterValues()[_parameterName];
                return _property.GetValue(PropertyOwner);
            } 
        }
       
        public WidgetPropertyVM(Widget widget, PropertyInfo property, object propertyOwner = null)
        {
            _widget = widget;
            PropertyOwner = propertyOwner == null ? _widget : propertyOwner;
            _property = property;
            DisplayedPropertyName = _property.Name + ":";
            UpdateEditorValueFromObjectProperty();
            DetermineInputMethod();
        }
        public WidgetPropertyVM(Widget widget, PropertyInfo widgetProperty, PropertyInfo brushProperty)
        {
            _widget = widget;
            PropertyOwner = widgetProperty.GetValue(widget);
            _property = brushProperty;
            DisplayedPropertyName = widgetProperty.Name + "." + _property.Name + ":";
            UpdateEditorValueFromObjectProperty();
            DetermineInputMethod();
        }
        public WidgetPropertyVM(Widget prefabWidget, string parameterName, List<Tuple<Widget, PropertyInfo>> parameterData)
        {
            _usesPrefabParameters = true;
            _parameterName = parameterName;
            _parameterData = parameterData;
            _widget = prefabWidget;
            DisplayedPropertyName = "Parameter." + parameterName + ":";
            UpdateEditorValueFromObjectProperty();
            DetermineInputMethod();
        }
        private void DetermineInputMethod()
        {
            Type type = null;
            if (_usesPrefabParameters)
            {
                type = _parameterData.FirstOrDefault()?.Item2.PropertyType;
            }
            else type = _property.PropertyType;
            // If the property is a boolean, use a toggle box. If it's an enum, use a dropdown box. Otherwise use an editable text box
            if (type == typeof(bool))
            {
                bool.TryParse(TextboxValue, out var boolVal);
                UsesToggleButton = true;
                ToggleButtonValue = boolVal;
            }
            else if (type != null && type.IsEnum)
            {
                var selectedIndex = 0;
                var valueArray = Enum.GetValues(type);
                for (int i = 0; i < valueArray.Length; i++)
                {
                    if (valueArray.GetValue(i).ToString() == TextboxValue) selectedIndex = i;
                }
                EnumSelector = new SelectorVM<SelectorItemVM>(type.GetEnumNames(), selectedIndex, DropDownValueChanged);
                UsesDropDownMenu = true;
            }
            else if (typeof(Widget).IsAssignableFrom(type))
            {
                UsesWidgetPicker = true;
            }
            else if (type == typeof(Color)) UsesColourPicker = true;
            else UsesTextBox = true;
        }
        public void ToggleButtonValueChanged()
        {
            UpdateActualProperty(ToggleButtonValue);
            //TextboxValue = ToggleButtonValue.ToString();
        }
        public void DropDownValueChanged(SelectorVM<SelectorItemVM> selector)
        {
            TextboxValue = selector.SelectedItem.StringItem;
        }

        public void EnterWidgetPickerMode()
        {
            UIEditorVM.Instance.EnterWidgetPickerMode(this);
        }
        public void SetPickedWidget(Widget widget)
        {
            if (widget != null && widget.EditorIsPrefabRoot())
            {
                ShowSelectWidgetMultiSelectionInquiry(widget);
            }
            else
            {
                UpdateActualProperty(widget);
                UpdateEditorValueFromObjectProperty();
            }
        }
        public void EnterColourPickerMode()
        {
            UIEditorVM.Instance.EnterColourPickerMode(this);
        }

        public void UpdateEditorValueFromObjectProperty()
        {
            _shouldUpdateActualProperty = false;
            TextboxValue = PropertyValue == null ? String.Empty : PropertyValue.ToString();
        }
        public void UpdateActualProperty(object value)
        {
            if (_usesPrefabParameters)
            {
                UIEditorVM.Instance.AddUndoOp(new ChangePropertyOperation(this, PropertyValue));
                foreach (var param in _parameterData)
                {
                    SetProperty(param.Item1, param.Item2, value);
                }
                _widget.EditorGetParameterValues()[_parameterName] = value;
            }
            else
            {
                UIEditorVM.Instance.AddUndoOp(new ChangePropertyOperation(this, PropertyValue));
                SetProperty(PropertyOwner, _property, value);
            }  
        }
        public void UpdateActualPropertyByUndo(object value)
        {
            if (_usesPrefabParameters)
            {
                foreach (var param in _parameterData)
                {
                    SetProperty(param.Item1, param.Item2, value);
                }
                _widget.EditorGetParameterValues()[_parameterName] = value;
            }
            else
            {
                SetProperty(PropertyOwner, _property, value);
            }
            UpdateEditorValueFromObjectProperty();
        }
        public static object CastStringToValue(string s, Type type, Widget widget)
        {
            if (type == typeof(int))
            {
                if (int.TryParse(s, out var newVal)) return newVal;                
            }
            else if (type == typeof(float))
            {
                if (float.TryParse(s, out var newVal)) return newVal;
            }
            else if (type == typeof(string))
            {
                return s;
            }
            else if (type == typeof(bool))
            {
                if (bool.TryParse(s, out var newVal)) return newVal;
            }
            else if (type.IsEnum)
            {
                if (Enum.GetNames(type).Contains(s))
                {
                    return Enum.Parse(type, s, true);
                }
            }
            else if (type == typeof(Color))
            {
                if (s.Length != "#FFFFFFFF".Length) return null;
                return Color.ConvertStringToColor(s);
            }
            else if (type == typeof(Sprite))
            {
                if (UIResourceManager.SpriteData == null) return null;
                return UIResourceManager.SpriteData.GetSprite(s);
            }
            else if (typeof(Widget).IsAssignableFrom(type))
            {
                return widget.FindChild(s);
            }
            else if (type == typeof(Brush))
            {
                if (UIResourceManager.BrushFactory == null) return null;
                return UIResourceManager.BrushFactory.GetBrush(s);
            }
            else if (type == typeof(Font))
            {
                if (UIResourceManager.FontFactory == null) return null;
                return UIResourceManager.FontFactory.GetFont(s);
            }
            return null;
        }
        private void SetProperty(object propertyOwner, PropertyInfo property, object value)
        {
            var type = property.PropertyType;
            var nullButNotSprite = value == null && type != typeof(Sprite);
            var notSameType = value != null && !type.IsAssignableFrom(value.GetType());
            if (nullButNotSprite || notSameType) return;
            if (type == typeof(string))
            {
                property.SetValue(propertyOwner, value);
                if (property.Name == "Id")
                {
                    UIEditorVM.Instance.UpdateWidgetHierarchy();
                }
            }
            else if (typeof(Widget).IsAssignableFrom(type))
            {
                var oldWidget = PropertyValue as Widget;
                if (propertyOwner is not Widget) return;
                if (oldWidget != null) oldWidget.EditorRemoveDependentProperty(propertyOwner as Widget, property);
                property.SetValue(propertyOwner, value);
                (value as Widget).EditorAddDependentProperty(propertyOwner as Widget, property);
            }
            else if (type == typeof(Brush))
            {
                var oldBrush = property.GetValue(propertyOwner);
                var brushProperties = _widget.EditorGetProperties().Where(p => p.PropertyOwner == oldBrush);
                property.SetValue(propertyOwner, value);
                var clonedBrush = property.GetValue(propertyOwner) as Brush;
                clonedBrush.Name = clonedBrush.ClonedFrom.Name;
                foreach (var brushProperty in brushProperties)
                {
                    brushProperty.PropertyOwner = property.GetValue(propertyOwner);
                    brushProperty.UpdateEditorValueFromObjectProperty();
                }
            }
            else property.SetValue(propertyOwner, value);
        }

        private void ShowSelectWidgetMultiSelectionInquiry(Widget widget)
        {
            if (widget == null)
            {
                SetPickedWidget(null);
                return;
            }
            var pickWidgetList = new List<InquiryElement>();
            var widgetName = "Parent: " + (string.IsNullOrEmpty(widget.Id) ? widget.GetType().Name : widget.Id + " (" + widget.GetType().Name + ")");
            pickWidgetList.Add(new InquiryElement(widget, widgetName, null));
            foreach (var child in widget.Children)
            {
                var childName = string.IsNullOrEmpty(child.Id) ? child.GetType().Name : child.Id + " (" + child.GetType().Name + ")";
                pickWidgetList.Add(new InquiryElement(child, childName, null));
            }
            var data = new MultiSelectionInquiryData("Choose Widget", "Selected widget is a prefab, do you wish to select it or one of its children?",
                pickWidgetList, false, 0, 1, "Select", "Show Children of Picked Option", PickAnotherWidget, ViewChildrenOfChosenWidget);
            MBInformationManager.ShowMultiSelectionInquiry(data);
        }

        private void ViewChildrenOfChosenWidget(List<InquiryElement> pickedWidgetsList)
        {
            if (pickedWidgetsList.Count > 0) ShowSelectWidgetMultiSelectionInquiry(pickedWidgetsList.First().Identifier as Widget);
            else SetPickedWidget(null);
        }

        private void PickAnotherWidget(List<InquiryElement> pickedWidgetsList)
        {
            if (pickedWidgetsList.Count > 0) SetPickedWidget(pickedWidgetsList.First().Identifier as Widget);
            else SetPickedWidget(null);
        }

        private string _displayedPropertyName;
        private string _parameterName;
        private string _textboxValue;
        private bool _usesTextBox;
        private bool _usesToggleButton;
        private bool _toggleButtonValue;
        private bool _usesWidgetPicker;
        private bool _usesDropDownMenu;
        private bool _usesColourPicker;
        private SelectorVM<SelectorItemVM> _enumSelector;
        private Widget _widget;
        public object PropertyOwner;
        private PropertyInfo _property;
        private bool _usesPrefabParameters = false;
        private List<Tuple<Widget, PropertyInfo>> _parameterData;
        private bool _shouldUpdateActualProperty;
    }
}
