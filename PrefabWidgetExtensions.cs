using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;
using System.Xml.Serialization;
using HarmonyLib;
using TaleWorlds.GauntletUI.BaseTypes;
using System.Reflection;
using TaleWorlds.LinQuick;
using System.Xml;

namespace InGameUIDesigner
{
    // Defines the editor properties for prefab widgets
    public class PrefabWidgetExtensions : PrefabExtension
    {
        public static bool IsAddingItemTemplate = false;
        protected override void AfterAttributesSet(WidgetCreationData widgetCreationData, WidgetInstantiationResult widgetInstantiationResult, Dictionary<string, WidgetAttributeTemplate> parameters)
        {
            // If the instantiated widget is not a child of the UIEditorVM.PreviewRootWidget then it is not an editable widget
            if (!widgetInstantiationResult.Widget.GetAllParents().Contains(UIEditorVM.PreviewRootWidget)) return;
            // Terms I use: Intrinsic child vs extrinsic child
            // An intrinsic child is a child of the prefab's root widget in the xml file that defines the prefab.
            // i.e. it is a core component of the prefab that will always be present.
            // An extrinsic child is a child of the prefab that is added when the prefab is used like any other widget inside another prefab.

            // Logical children location refers to which widget inside a prefab where extrinsic children are inserted, the default
            // is right underneath the root widget of the prefab.

            var widget = widgetInstantiationResult.Widget;
            var template = widgetInstantiationResult.Template;
            var editorData = new WidgetEditorData(widget);
            Widget prefabRootWidget = GetWidgetFromTemplate(template.RootTemplate, widgetInstantiationResult);
            editorData.IntrinsicChildOf = prefabRootWidget;
            var isPrefab = widgetCreationData.WidgetFactory.IsCustomType(template.Type) || (template == template.RootTemplate);
            if (isPrefab)
            {
                editorData.IsPrefabRoot = true;
                editorData.PrefabName = template.Type;
                prefabRootWidget = widget;
                if (widgetInstantiationResult.CustomWidgetInstantiationData != null)
                {
                    editorData.LogicalChildrenLocation = widgetInstantiationResult.GetLogicalOrDefaultChildrenLocation().Widget;
                    widget.Tag = widgetInstantiationResult.CustomWidgetInstantiationData.Template.Tag;
                }
                else editorData.LogicalChildrenLocation = widgetInstantiationResult.Widget;
            }
            if (template.LogicalChildrenLocation) editorData.IAmLogicalChildLocationFor = prefabRootWidget;
            //By default, any widget that is created as part of a prefab will be locked and won't be shown in the widget hierarchy
            editorData.ShowInHierarchy = false;
            editorData.Locked = true;
            widget.AddComponent(editorData);

            if (prefabRootWidget != null)
            {
                AddParametersFromWidget(widget, prefabRootWidget, widgetInstantiationResult.Template);
                if (widgetInstantiationResult.CustomWidgetInstantiationData != null) AddParametersFromWidget(widget, prefabRootWidget, widgetInstantiationResult.CustomWidgetInstantiationData.Template);
            }
            foreach (var childInstantiationResult in widgetInstantiationResult.Children)
            {
                AfterAttributesSet(widgetCreationData, childInstantiationResult, parameters);
            }
            if (widgetInstantiationResult.CustomWidgetInstantiationData != null)
            {
                foreach (var childInstantiationResult in widgetInstantiationResult.CustomWidgetInstantiationData.Children)
                {
                    AfterAttributesSet(widgetCreationData, childInstantiationResult, parameters);
                }
            }
        }

        private Widget GetWidgetFromTemplate(WidgetTemplate template, WidgetInstantiationResult instantiationResults)
        {
            foreach (var ancestor in  instantiationResults.Widget.GetAllParents())
            {
                // Widget Tag is set to the template's Tag, which is a GUID, when the widget is created
                // Since the widget's Tag doesn't seem to be set elsewhere, it appears safe to use it to find the template that was
                // used to create that widget
                if (ancestor.Tag == template.Tag) return ancestor;
            }
            return null;
        }

        /// <summary>
        /// Finds properties in a widget that use parameters and adds them to the prefab root editor data
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="prefabRootWidget"></param>
        /// <param name="template"></param>
        private void AddParametersFromWidget(Widget widget, Widget prefabRootWidget, WidgetTemplate template)
        {
            foreach (var attribute in template.AllAttributes)
            {
                // WidgetAttributeValueTypeParameter means that the attribute in the xml starts by *, which defines values that use parameters
                // Currently this does not support nested properties such as Brush.Font or StackLayout.Layout
                if (attribute.ValueType is not WidgetAttributeValueTypeParameter) continue;
                var propertyName = attribute.Key;
                var parameterName = attribute.Value;
                var property = AccessTools.Property(widget.GetType(), propertyName);
                if (property == null) continue;
                var paramValues = prefabRootWidget.EditorGetParameterValues();
                var paramBindingData = prefabRootWidget.EditorGetParameterBindingData();
                // Binding data contains a list of widgets and their associated properties that use a parameter
                if (!paramBindingData.ContainsKey(parameterName))
                {
                    paramBindingData.Add(parameterName, new List<Tuple<Widget, PropertyInfo>>());
                }
                paramBindingData[parameterName].Add(new Tuple<Widget, PropertyInfo>(widget, property));
                // For the first time the parameter is added, the value of the parameter is added from the prefab template (i.e. it's xml)
                if (!paramValues.ContainsKey(parameterName))
                {
                    var val = WidgetPropertyVM.CastStringToValue(template.Prefab.Parameters[parameterName], property.PropertyType, widget);
                    paramValues.Add(parameterName, val);
                }
            }
        }
    }
}
