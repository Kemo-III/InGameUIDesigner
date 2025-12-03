using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace InGameUIDesigner
{
    public class WidgetEditorData : WidgetComponent
    {
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (HierarchyVM != null) HierarchyVM.Selected = value;
            }
        }
        private bool _isSelected = false;
        public bool Locked = false;
        public bool ShowInHierarchy = true;
        public bool IsPrefabRoot = false;
        public string PrefabName = String.Empty;
        public Widget IntrinsicChildOf;
        public Widget LogicalChildrenLocation;
        public Widget IAmLogicalChildLocationFor;
        public Dictionary<string, List<Tuple<Widget, PropertyInfo>>> ParameterBindingData;
        public Dictionary<string, object> ParameterValues;
        //public string DataSourceName;
        // Key is the property name. Value is the name of the binding after the '@'
        //public Dictionary<string, string> DataSourceBindingProperties;
        public WidgetHierarchyVM HierarchyVM;
        public MBBindingList<WidgetPropertyVM> WidgetProperties;
        public Dictionary<Widget, List<PropertyInfo>> WidgetPropertyDependencies;
        public WidgetEditorData(Widget target) : base(target)
        {
            //DataSourceName = String.Empty;
            //DataSourceBindingProperties = new Dictionary<string, string>();
            WidgetProperties = new MBBindingList<WidgetPropertyVM>();
            ParameterValues = new Dictionary<string, object>();
            ParameterBindingData = new Dictionary<string, List<Tuple<Widget, PropertyInfo>>>();
            WidgetPropertyDependencies = new Dictionary<Widget, List<PropertyInfo>>();
            LogicalChildrenLocation = target;
            IAmLogicalChildLocationFor = null;
        }
        public void RemoveProperty(string propertyName)
        {
            if (propertyName == string.Empty) return;
            //if (DataSourceBindingProperties.ContainsKey(propertyName)) DataSourceBindingProperties.Remove(propertyName);
        }
        public void AddOrChangeProperty(string propertyName, string propertyBindingValue)
        {
            propertyBindingValue = propertyBindingValue.Replace("@", "");
            propertyBindingValue = propertyBindingValue.Replace("!", "");
            if (propertyName == string.Empty || propertyBindingValue == string.Empty) return;
            //if (DataSourceBindingProperties.ContainsKey(propertyName)) DataSourceBindingProperties[propertyName] = propertyBindingValue;
            //else DataSourceBindingProperties.Add(propertyName, propertyBindingValue);
        }
        public string GetPropertyValue(string propertyName)
        {
            /*if (DataSourceBindingProperties.TryGetValue(propertyName, out var propertyValue))
            {
                return propertyValue;
            }*/
            return string.Empty;
        }
        
        public void AddDependency(Widget widget, PropertyInfo property)
        {
            if (!typeof(Widget).IsAssignableFrom(property.PropertyType)) return;
            if (!WidgetPropertyDependencies.ContainsKey(widget)) WidgetPropertyDependencies.Add(widget, new List<PropertyInfo>());
            WidgetPropertyDependencies[widget].Add(property);
        }
        public void RemoveDependency(Widget widget, PropertyInfo property)
        {
            if (!typeof(Widget).IsAssignableFrom(property.PropertyType)) return;
            if (!WidgetPropertyDependencies.TryGetValue(widget, out var listOfDependenentProperties)) return;
            if (listOfDependenentProperties.Contains(property))
            {
                listOfDependenentProperties.Remove(property);
                property.SetValue(widget, null);
            }
        }

        public void ChangeDepndency(Widget oldWidget, Widget newWidget, PropertyInfo property)
        {
            RemoveDependency(oldWidget, property);
            AddDependency(newWidget, property);
        }

        public WidgetEditorData CreateCopy(Widget newTargetWidget, Dictionary<Widget, Widget> equivalentWidgets)
        {
            var newData = new WidgetEditorData(newTargetWidget);
            newData.CopyFrom(this, equivalentWidgets);
            return newData;
        }
        public void CopyFrom(WidgetEditorData source, Dictionary<Widget, Widget> equivalentWidgets)
        {
            Locked = source.Locked;
            ShowInHierarchy = source.ShowInHierarchy;
            IsPrefabRoot = source.IsPrefabRoot;
            PrefabName = source.PrefabName;
            ParameterValues = new Dictionary<string, object>(source.ParameterValues);
            ParameterBindingData = new Dictionary<string, List<Tuple<Widget, PropertyInfo>>>();
            foreach (var paramBindingDataKeyValuePair in source.ParameterBindingData)
            {
                var newList = new List<Tuple<Widget, PropertyInfo>>();
                foreach (var paramBindingData in paramBindingDataKeyValuePair.Value)
                {
                    var newWidget = GetEquivalentWidget(source.Target, paramBindingData.Item1, equivalentWidgets);
                    newList.Add(new Tuple<Widget, PropertyInfo>(newWidget, paramBindingData.Item2));
                }
                ParameterBindingData.Add(paramBindingDataKeyValuePair.Key, newList);
            }
            //DataSourceName = source.DataSourceName;
            //DataSourceBindingProperties = new Dictionary<string, string>(source.DataSourceBindingProperties);
            foreach (var dependencyKeyValuePair in source.WidgetPropertyDependencies)
            {
                var equivalentWidget = GetEquivalentWidget(source.Target, dependencyKeyValuePair.Key, equivalentWidgets);
                foreach (var dependentProperty in dependencyKeyValuePair.Value)
                {
                    AddDependency(equivalentWidget, dependentProperty);
                }
            }
            IntrinsicChildOf = source.IntrinsicChildOf;
            LogicalChildrenLocation = GetEquivalentWidget(source.Target, source.LogicalChildrenLocation, equivalentWidgets);
            //newData.IAmLogicalChildLocationFor;
            // HierarchyVM is assigned in UpdateWidgetHierarchy
            // WidgetProperties is assigned in UpdateWidgetProperties
        }
        private Widget GetEquivalentWidget(Widget checkedWidget, Widget equivalentOf, Dictionary<Widget, Widget> equivalentWidgets)
        {
            if (equivalentWidgets.TryGetValue(equivalentOf, out var equivalentWidget));
            else equivalentWidget = equivalentOf;
            return equivalentWidget;
        }
    }

    public static class WidgetEditorExtensions
    {
        public static bool EditorIsSelected(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? false : editorData.IsSelected;
        }
        public static void EditorSetSelected(this Widget widget, bool selected)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            editorData.IsSelected = selected;
        }

        public static bool EditorIsLocked(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? true : editorData.Locked;
        }
        public static void EditorSetLocked(this Widget widget, bool locked)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            editorData.Locked = locked;
        }

        public static bool EditorIsShownInHierarchy(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? false : editorData.ShowInHierarchy;
        }
        public static void EditorSetShownInHierarchy(this Widget widget, bool shownInHierarchy)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            editorData.ShowInHierarchy = shownInHierarchy;
        }

        /*public static string EditorGetDataSourceName(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? String.Empty : editorData.DataSourceName;
        }
        public static void EditorSetDataSourceName(this Widget widget, string newName)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            editorData.DataSourceName = newName;
        }*/

        public static MBBindingList<WidgetPropertyVM> EditorGetProperties(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? new MBBindingList<WidgetPropertyVM>() : editorData.WidgetProperties;
        }
        public static void EditorSetProperties(this Widget widget, MBBindingList<WidgetPropertyVM> newPropertiesList)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            editorData.WidgetProperties = newPropertiesList;
        }

        public static WidgetHierarchyVM EditorGetWidgetHierarchyVM(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? null : editorData.HierarchyVM;
        }
        public static void EditorSetWidgetHierarchyVM(this Widget widget, WidgetHierarchyVM newViewModel)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            editorData.HierarchyVM = newViewModel;
        }

        /*public static string EditorGetDataSourcePropertyValue(this Widget widget, string propertyName)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? String.Empty : editorData.GetPropertyValue(propertyName);
        }
        public static void EditorRemoveDataSourceProperty(this Widget widget, string propertyName)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            editorData.RemoveProperty(propertyName);
        }

        public static void EditorAddOrChangeDataSourceProperty(this Widget widget, string propertyName, string propertyBindingValue)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            editorData.AddOrChangeProperty(propertyName, propertyBindingValue);
        }
        public static new Dictionary<string, string> EditorGetDataSourceProperties(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? new Dictionary<string, string>() : editorData.DataSourceBindingProperties;
        }*/

        public static bool EditorIsPrefabRoot(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? false : editorData.IsPrefabRoot;
        }
        /*public static bool EditorIsPartOfPrefab(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? false : editorData.IntrinsicPartOfPrefab;
        }*/
        public static Widget EditorIntrinsicChildOf(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? null : editorData.IntrinsicChildOf;
        }

        public static string EditorGetPrefabName(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? String.Empty : editorData.PrefabName;
        }

        public static void EditorSetPrefabName(this Widget widget, string prefabName)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            editorData.PrefabName = prefabName;
        }
        public static Dictionary<string, object> EditorGetParameterValues(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? new Dictionary<string, object>() : editorData.ParameterValues;
        }
        public static Dictionary<string, List<Tuple<Widget, PropertyInfo>>> EditorGetParameterBindingData(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? new Dictionary<string, List<Tuple<Widget, PropertyInfo>>>() : editorData.ParameterBindingData;
        }

        public static Dictionary<Widget, List<PropertyInfo>> EditorGetDependentProperties(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? new Dictionary<Widget, List<PropertyInfo>>() : editorData.WidgetPropertyDependencies;
        }

        public static void EditorAddDependentProperty(this Widget widget, Widget dependentWidget, PropertyInfo dependentProperty)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            editorData?.AddDependency(dependentWidget, dependentProperty);
        }

        public static void EditorRemoveDependentProperty(this Widget widget, Widget dependentWidget, PropertyInfo dependentProperty)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            editorData?.RemoveDependency(dependentWidget, dependentProperty);
        }

        public static void EditorRemoveAllDependencies(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData == null) return;
            for (int i = editorData.WidgetPropertyDependencies.Count - 1; i >=0; i--)
            {
                var dependentWidget = editorData.WidgetPropertyDependencies.ElementAt(i);
                for (int j = dependentWidget.Value.Count - 1; j >= 0; j--)
                {
                    editorData.RemoveDependency(dependentWidget.Key, dependentWidget.Value[j]);
                }
            }
        }
        public static void EditorChangeDependentProperty(this Widget widget, Widget oldDependentWidget, Widget newDependentWidget, PropertyInfo dependentProperty)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            editorData?.ChangeDepndency(oldDependentWidget, newDependentWidget, dependentProperty);
        }

        public static Widget EditorGetLogicalChildrenLocation(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? widget : editorData.LogicalChildrenLocation;
        }
        public static void EditorSetLogicalChildrenLocation(this Widget widget, Widget logicalChildrenLocation)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData != null) editorData.LogicalChildrenLocation = logicalChildrenLocation;
        }
        public static Widget EditorGetIAmLogicalChildLocationFor(this Widget widget)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            return editorData == null ? null : editorData.IAmLogicalChildLocationFor;
        }
        public static void EditorSetIAmLogicalChildLocationFor(this Widget widget, Widget iAmLogicalChildLocationFor)
        {
            var editorData = widget.GetComponent<WidgetEditorData>();
            if (editorData != null) editorData.IAmLogicalChildLocationFor = iAmLogicalChildLocationFor;
        }
    }
}
