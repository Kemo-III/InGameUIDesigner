extern alias MountAndBlade;
using System;
using System.Linq;
using System.Xml;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using MountAndBlade.System.Numerics;
using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using HorizontalAlignment = TaleWorlds.GauntletUI.HorizontalAlignment;
using System.Text;
using InGameUIDesigner.EditorOperations;
using TaleWorlds.GauntletUI.ExtraWidgets;
using System.Windows.Controls;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;

namespace InGameUIDesigner
{
    public class UIEditorVM : ViewModel
    {
        public static UIEditorVM Instance;
        
        // See PreventNullScopePatch.cs
        public static bool DontDisconnectScopeFromRoot 
        { 
            get => Instance == null ? false : Instance._dontDisconnectScopeFromRoot; 
            set
            {
                if (Instance != null) Instance._dontDisconnectScopeFromRoot = value;
            }
        }
        public static Widget PreviewRootWidget { get => Instance?._previewRootWidget; }
        [DataSourceProperty]
        public MBBindingList<WidgetResourceVM> AvailableBasicWidgetNames
        {
            get => _availableBasicWidgetNames;
            set
            {
                if (value != _availableBasicWidgetNames)
                {
                    _availableBasicWidgetNames = value;
                    OnPropertyChangedWithValue(value, "AvailableBasicWidgetNames");
                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<WidgetResourceVM> AvailablePrefabWidgetNames
        {
            get => _availablePrefabWidgetNames;
            set
            {
                if (value != _availablePrefabWidgetNames)
                {
                    _availablePrefabWidgetNames = value;
                    OnPropertyChangedWithValue(value, "AvailablePrefabWidgetNames");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<WidgetPropertyVM> SelectedWidgetProperties
        {
            get => _selectedWidgetProperties;
            set
            {
                if (value != _selectedWidgetProperties)
                {
                    _selectedWidgetProperties = value;
                    OnPropertyChangedWithValue(value, "SelectedWidgetProperties");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<WidgetHierarchyVM> WidgetHierarchy
        {
            get => _widgetHierarchy;
            set
            {
                if (value != _widgetHierarchy)
                {
                    _widgetHierarchy = value;
                    OnPropertyChangedWithValue(value, "WidgetHierarchy");
                }
            }
        }

        [DataSourceProperty]
        public string BasicWidgetsSearchText
        {
            get => _basicWidgetsSearchText;
            set
            {
                if (value != _basicWidgetsSearchText)
                {
                    _basicWidgetsSearchText = value;
                    OnPropertyChangedWithValue(value, "BasicWidgetsSearchText");
                    FilterBasicWidgetsList();
                }
            }
        }
        [DataSourceProperty]
        public string PrefabWidgetsSearchText
        {
            get => _prefabWidgetsSearchText;
            set
            {
                if (value != _prefabWidgetsSearchText)
                {
                    _prefabWidgetsSearchText = value;
                    OnPropertyChangedWithValue(value, "PrefabWidgetsSearchText");
                    FilterPrefabWidgetsList();
                }
            }
        }
        [DataSourceProperty]
        public string PropertySearchText
        {
            get => _propertySearchText;
            set
            {
                if (value != _propertySearchText)
                {
                    _propertySearchText = value;
                    OnPropertyChangedWithValue(value, "PropertySearchText");
                    FilterPropertiesList();
                }
            }
        }
        public bool WidgetPickerMode { get => _widgetPickerMode; }

        public UIEditorVM(UIEditorScreen screen)
        {
            _ownerScreen = screen;
            Instance = this;
            DontDisconnectScopeFromRoot = false;
            _unfilteredBasicWidgetNames = new MBBindingList<WidgetResourceVM>();
            _unfilteredPrefabWidgetNames = new MBBindingList<WidgetResourceVM>();
            AvailableBasicWidgetNames = new MBBindingList<WidgetResourceVM>();
            AvailablePrefabWidgetNames = new MBBindingList<WidgetResourceVM>();
            SelectedWidgetProperties = new MBBindingList<WidgetPropertyVM>();
            WidgetHierarchy = new MBBindingList<WidgetHierarchyVM>();
            _undoOperationsStack = new DropOutStack<UIEditorOperation>(MAX_UNDO_COUNT);
            _redoOperationsStack = new DropOutStack<UIEditorOperation>(MAX_UNDO_COUNT);
            var factory = UIResourceManager.WidgetFactory;
            foreach (var name in factory.GetWidgetTypes())
            {
                // Ignore any widget containing __ in its name, since it's most likely an auto-generated widget
                if (name.Contains("__") || name == "DragCarrierWidget" || name == "TextureWidget" || name == "Container" || name == "EditorWigetScalerWidget") continue;
                if (factory.IsBuiltinType(name)) _unfilteredBasicWidgetNames.Add(new WidgetResourceVM(name, this));
                else if (factory.IsCustomType(name)) _unfilteredPrefabWidgetNames.Add(new WidgetResourceVM(name, this));
            }
            BasicWidgetsSearchText = "";
            PrefabWidgetsSearchText = "";
            PropertySearchText = "";
            AddExcludedProperties();
        }

        private void AddExcludedProperties()
        {
            // These properties are excluded from being viewed in the selected widget properties
            _excludedWidgetProperties = new List<string>();
            _excludedWidgetProperties.Add("ParentWidget");
            _excludedWidgetProperties.Add("DragWidget");
            _excludedWidgetProperties.Add("Tag");
            _excludedWidgetProperties.Add("PosOffset");
            _excludedWidgetProperties.Add("LayoutImp");
            _excludedWidgetProperties.Add("ScaledSuggestedWidth");
            _excludedWidgetProperties.Add("ScaledSuggestedHeight");
            _excludedWidgetProperties.Add("ScaledPositionXOffset");
            _excludedWidgetProperties.Add("ScaledPositionYOffset");
            _excludedWidgetProperties.Add("VisualDefinition");

            _excludedBrushProperties = new List<string>();
            _excludedBrushProperties.Add("Name");
            _excludedBrushProperties.Add("DefaultStyle");
            _excludedBrushProperties.Add("SoundProperties");
            _excludedBrushProperties.Add("Layers");
            _excludedBrushProperties.Add("DefaultStyleLayer");
            _excludedBrushProperties.Add("DefaultLayer");
        }

        public void Initialize(Widget previewRoot, UIContext uiContext, WidgetFactory widgetFactory, GauntletMovie movie)
        {
            _previewRootWidget = previewRoot;
            _selectedWidgetScaler = (EditorWigetScalerWidget) uiContext.Root.FindChild("ActiveWidgetScaler", true);
            _context = uiContext;
            _widgetFactory = widgetFactory;
            _widgetFactory.PrefabExtensionContext.AddExtension(new PrefabWidgetExtensions());
            _movie = movie;
        }

        public override void OnFinalize()
        {
            Instance = null;
            base.OnFinalize();
        }

        private void FilterBasicWidgetsList()
        {
            if (BasicWidgetsSearchText.Length < 4) AvailableBasicWidgetNames = _unfilteredBasicWidgetNames;
            else
            {
                AvailableBasicWidgetNames = new MBBindingList<WidgetResourceVM>();
                foreach (var widget in _unfilteredBasicWidgetNames)
                {
                    if (widget.Name.ToLower().Contains(BasicWidgetsSearchText.ToLower())) AvailableBasicWidgetNames.Add(widget);
                }
            }
        }
        private void FilterPrefabWidgetsList()
        {
            if (PrefabWidgetsSearchText.Length < 4) AvailablePrefabWidgetNames = _unfilteredPrefabWidgetNames;
            else
            {
                AvailablePrefabWidgetNames = new MBBindingList<WidgetResourceVM>();
                foreach (var widget in _unfilteredPrefabWidgetNames)
                {
                    if (widget.Name.ToLower().Contains(PrefabWidgetsSearchText.ToLower())) AvailablePrefabWidgetNames.Add(widget);
                }
            }
        }
        private void FilterPropertiesList()
        {
            if (_selectedWidget == null) return;
            if (PropertySearchText.Length < 4) SelectedWidgetProperties = _selectedWidget.EditorGetProperties();
            else
            {
                SelectedWidgetProperties = new MBBindingList<WidgetPropertyVM>();
                foreach (var property in _selectedWidget.EditorGetProperties())
                {
                    if (property.DisplayedPropertyName.ToLower().Contains(PropertySearchText.ToLower())) SelectedWidgetProperties.Add(property);
                }
            }
        }
        
        public void ExecuteDone()
        {
            ScreenManager.PopScreen();
        }
        public void ExecuteCancel()
        {
            ScreenManager.PopScreen();
        }
        
        public void ShowBrushList()
        {
            _ownerScreen.ShowBrushPreviewList();
        }
        public void ShowSpriteList()
        {
            _ownerScreen.ShowSpritePreviewList();
        }
        public void ExportButtonAction()
        {
            _ownerScreen.OpenExportPrompt();
        }
        /// <summary/>
        /// <param name="property">Colour property that is going to change</param>
        public void EnterColourPickerMode(WidgetPropertyVM property)
        {
            _ownerScreen.ShowColourPicker(property);
        }
        public void Exit()
        {
            _ownerScreen.OpenExitPrompt();
        }
        public void ExportPrefab(string path)
        {
            var xmlDoc = new XmlDocument();
            var prefabNode = xmlDoc.CreateElement("Prefab");
            var windowNode = xmlDoc.CreateElement("Window");

            AppendXMLNodeFromWidget(_previewRootWidget, _context, windowNode, xmlDoc);
            prefabNode.AppendChild(windowNode);
            xmlDoc.AppendChild(prefabNode);

            xmlDoc.Save(path);
        }
        private void AppendXMLNodeFromWidget(Widget widget, UIContext uiContext, XmlNode parentNode, XmlDocument document)
        {
            var nodeName = widget.EditorIsPrefabRoot() ? widget.EditorGetPrefabName() : widget.GetType().Name;
            if (string.IsNullOrEmpty(nodeName)) nodeName="ErrorWith" + widget.GetType().Name;
            var widgetNode = document.CreateElement(nodeName);
            // Create a widget of the same type to compare properties. Any property that is unchanged isn't exported.
            // This is to create a smaller XML which is less cluttered with useless properties.
            var defaultWidgetOfType = widget.GetType().GetConstructor(new Type[] { typeof(UIContext) }).Invoke(new object[] { uiContext });
            var widgetTypeProperties = widget.GetType().GetProperties();
            foreach (var propertyInfo in widgetTypeProperties)
            {
                // Ignore the Id property of the PreviewRootWidget, as well any methods without a public setter function
                // Ignore properties that are unchanged from the default value, and empty strings
                if ((widget == _previewRootWidget && propertyInfo.Name == "Id") || _excludedWidgetProperties.Contains(propertyInfo.Name) || propertyInfo.SetMethod == null || !propertyInfo.SetMethod.IsPublic) continue;
                var defaultValue = propertyInfo.GetValue(defaultWidgetOfType);
                if (widget.EditorIsPrefabRoot())
                {
                    var widgetPrefab = UIResourceManager.WidgetFactory.GetCustomType(widget.EditorGetPrefabName());
                    if (widgetPrefab.RootTemplate.Attributes[typeof(WidgetAttributeKeyTypeAttribute)].TryGetValue(propertyInfo.Name, out var attributeTemplate))
                    {
                        var templateDefault = WidgetPropertyVM.CastStringToValue(attributeTemplate.Value, defaultValue.GetType(), widget);
                        if (templateDefault != null) defaultValue = templateDefault;
                    }
                }
                var widgetValue = propertyInfo.GetValue(widget);
                if (widgetValue == null) continue;
                if (widgetValue is string str && str == String.Empty) continue;
                if (widgetValue.Equals(defaultValue)) continue;

                var attribute = document.CreateAttribute(propertyInfo.Name);
                widgetNode.Attributes.Append(attribute);
                if (propertyInfo.PropertyType == typeof(Brush))
                {
                    // For brushes, set the name to the original brush (without (Cloned) ) and then export all properties that are
                    // different from the original brush
                    var brush = widgetValue as Brush;
                    var val = brush.ClonedFrom == null ? brush.Name : brush.ClonedFrom.Name;
                    widgetNode.SetAttribute(attribute.Name, val);
                    foreach (var brushPropertyInfo in typeof(Brush).GetProperties())
                    {
                        if (_excludedBrushProperties.Contains(brushPropertyInfo.Name) || brushPropertyInfo.SetMethod == null || !brushPropertyInfo.SetMethod.IsPublic) continue;
                        var currentBrushValue = brushPropertyInfo.GetValue(brush);
                        var defaultBrushValue = brushPropertyInfo.GetValue(brush.ClonedFrom);
                        if (currentBrushValue == null || currentBrushValue.Equals(defaultBrushValue)) continue;
                        var brushAttribute = document.CreateAttribute("Brush." + brushPropertyInfo.Name);
                        brushAttribute.Value = currentBrushValue.ToString();
                        widgetNode.Attributes.Append(brushAttribute);
                    }
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    // Bools are only valid in lower-case when imported
                    widgetNode.SetAttribute(attribute.Name, widgetValue.ToString().ToLower());
                }
                else if (typeof(Widget).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    // Get the path of the property widget relative to the current widget
                    var widgetPath = GetWidgetPathFromWidget(widget, propertyInfo.GetValue(widget) as Widget, true);
                    widgetNode.SetAttribute(attribute.Name, widgetPath);
                }
                else widgetNode.SetAttribute(attribute.Name, widgetValue.ToString());
            }
            // Prefab parameters exporting
            if (widget.EditorIsPrefabRoot())
            {
                foreach (var parameterKeyValuePair in widget.EditorGetParameterValues())
                {
                    // Key is parameter name, Value is the parameter value
                    var attribute = document.CreateAttribute("Parameter." + parameterKeyValuePair.Key);
                    widgetNode.Attributes.Append(attribute);
                    widgetNode.SetAttribute(attribute.Name, parameterKeyValuePair.Value.ToString());
                }
            }
            // Nested properties are currently unsupported, so list panels and grids get special export rules
            if (widget is ListPanel listPanel)
            {
                var attribute = document.CreateAttribute("StackLayout.LayoutMethod");
                widgetNode.Attributes.Append(attribute);
                widgetNode.SetAttribute(attribute.Name, listPanel.StackLayout.LayoutMethod.ToString());
            }
            else if (widget is GridWidget grid)
            {
                var directionAttribute = document.CreateAttribute("GridLayout.Direction");
                widgetNode.Attributes.Append(directionAttribute);
                widgetNode.SetAttribute(directionAttribute.Name, grid.GridLayout.Direction.ToString());
                var horzLayoutAttribute = document.CreateAttribute("GridLayout.HorizontalLayoutMethod");
                widgetNode.Attributes.Append(horzLayoutAttribute);
                widgetNode.SetAttribute(horzLayoutAttribute.Name, grid.GridLayout.HorizontalLayoutMethod.ToString());
                var vertLayoutAttribute = document.CreateAttribute("GridLayout.VerticalLayoutMethod");
                widgetNode.Attributes.Append(vertLayoutAttribute);
                widgetNode.SetAttribute(vertLayoutAttribute.Name, grid.GridLayout.VerticalLayoutMethod.ToString());
            }

            var childrenNode = document.CreateElement("Children");
            foreach (var child in widget.Children)
            {
                // Only export children that are shown in the hierarchy. i.e. extrinsic children of prefabs and not the intrinsic ones
                if (!child.EditorIsShownInHierarchy()) continue;
                AppendXMLNodeFromWidget(child, uiContext, childrenNode, document);
            }
            // When using logical children location there is a discrepency between actual parent widget in-game, and the parent widget
            // in the xml file. If a logical children location is found for this widget, its children are exported under this widget
            // rather than the logical children location widget itself.
            var logicalChildrenLocation = widget.EditorGetLogicalChildrenLocation();
            if (logicalChildrenLocation != null && logicalChildrenLocation != widget)
            {
                foreach (var child in logicalChildrenLocation.Children)
                {
                    if (!child.EditorIsShownInHierarchy()) continue;
                    AppendXMLNodeFromWidget(child, uiContext, childrenNode, document);
                }
            }
            if (childrenNode.ChildNodes.Count > 0) widgetNode.AppendChild(childrenNode);
            parentNode.AppendChild(widgetNode);
        }
        public Widget AddWidget(Widget parent, string typeName)
        {
            if (_widgetFactory.IsBuiltinType(typeName))
            {
                var type = _widgetFactory.GetBuiltinType(typeName);
                if (Game.Current == null && IsTableauWidgetType(type))
                {
                    InformationManager.DisplayMessage(new InformationMessage("Could not add TableauWidget. Try starting the editor within a campaign or custom battle screen",
                        Color.FromVector3(new Vec3(1f, 0.1f, 0.1f))));
                    return null;
                }
                var widget = (Widget)type.GetConstructor(new Type[] { typeof(UIContext) }).Invoke(new object[] { _context });
                if (parent == null) _previewRootWidget.AddChild(widget);
                else parent.AddChild(widget);
                InitializeWidgetProperties(widget);
                UpdateWidgetHierarchy();
                UpdateWidgetProperties(widget);
                AddUndoOp(new AddWidgetOperation(widget));
                return widget;
            }
            else if (_widgetFactory.IsCustomType(typeName))
            {
                if (parent == null) parent = _previewRootWidget;
                var prefab = _widgetFactory.GetCustomType(typeName);
                var createData = new WidgetCreationData(_context, _widgetFactory, parent);
                createData.AddExtensionData(_movie);

                var widget = prefab.Instantiate(createData).Widget;
                widget.EditorSetPrefabName(typeName);
                widget.EditorSetShownInHierarchy(true);
                widget.EditorSetLocked(false);
                AddItemTemplateWidgets(widget, 0);
                UpdateWidgetHierarchy();
                UpdateWidgetProperties(widget);
                AddUndoOp(new AddWidgetOperation(widget));
                return widget;
            }
            return null;
        }

        /// <summary>
        /// Adds a prefab as a regular widget and allows its intrinsic children to be edited
        /// </summary>
        /// <param name="prefabName"></param>
        public void ImportPrefab(string prefabName)
        {
            var widget = AddWidget(_previewRootWidget, prefabName);
            PrefabUnlockChildrenRecursive(widget, widget);
            widget.GetComponent<WidgetEditorData>().IsPrefabRoot = false;
            widget.EditorGetProperties().Clear();
            UpdateWidgetProperties(widget);
            UpdateWidgetHierarchy();
        }
        
        private void AddItemTemplateWidgets(Widget widget, int numberOfAddedTemplates)
        {
            PrefabWidgetExtensions.IsAddingItemTemplate = true;
            var view = widget.GetComponent<GauntletView>();
            var createData = new WidgetCreationData(_context, _widgetFactory, widget);
            createData.AddExtensionData(_movie);
            // Some prefabs refer to themselves as item templates, numberOfAddedTemplates makes sure we don't enter a infinite loop
            // A side effect is that any item template after a 'depth' of 20 will not be added, but I think actual prefabs will rarely 
            // reach that many item templates.
            if (view != null && view.ItemTemplateUsageWithData != null && numberOfAddedTemplates <= 20)
            { 
                view.ItemTemplateUsageWithData.DefaultItemTemplate.Instantiate(createData, view.ItemTemplateUsageWithData.GivenParameters);
                numberOfAddedTemplates++;
            }
            for (int i = 0; i < widget.ChildCount; i++)
            {
                AddItemTemplateWidgets(widget.GetChild(i), numberOfAddedTemplates);
            }
            PrefabWidgetExtensions.IsAddingItemTemplate = false;
        }

        private void PrefabUnlockChildrenRecursive(Widget parent, Widget root)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var widget = parent.GetChild(i);
                if (widget.EditorIntrinsicChildOf() == root)
                {
                    widget.EditorSetShownInHierarchy(true);
                    widget.EditorSetLocked(false);
                    UpdateWidgetProperties(widget);
                }
                PrefabUnlockChildrenRecursive(widget, root);
            }
        }
        public Widget DuplicateWidget(Widget source)
        {
            var typeName = source.GetType().Name;
            var type = _widgetFactory.GetBuiltinType(typeName);
            if (Game.Current == null && IsTableauWidgetType(type))
            {
                InformationManager.DisplayMessage(new InformationMessage("Could not add TableauWidget. Try starting the editor within a campaign or custom battle screen",
                    Color.FromVector3(new Vec3(1f, 0.1f, 0.1f))));
                return null;
            }
            var widget = (Widget)type.GetConstructor(new Type[] { typeof(UIContext) }).Invoke(new object[] { _context });
            source.ParentWidget.AddChild(widget);
            var equivalentWidgets = new Dictionary<Widget, Widget>();
            equivalentWidgets.Add(source, widget);
            foreach (var child in source.Children)
            {
                DuplicateWidgetRecursive(child, widget, equivalentWidgets);
            }
            foreach (var sourceChild in source.AllChildrenAndThis)
            {
                CopyWidgetProperties(equivalentWidgets[sourceChild], sourceChild, equivalentWidgets);
            }
            UpdateWidgetHierarchy();
            UpdateWidgetProperties(widget);
            AddUndoOp(new AddWidgetOperation(widget));
            return widget;
        }
        private Widget DuplicateWidgetRecursive(Widget source, Widget parent, Dictionary<Widget, Widget> equivalentWidgets)
        {
            var typeName = source.GetType().Name;
            var type = _widgetFactory.GetBuiltinType(typeName);
            if (Game.Current == null && IsTableauWidgetType(type))
            {
                InformationManager.DisplayMessage(new InformationMessage("Could not add TableauWidget. Try starting the editor within a campaign or custom battle screen",
                    Color.FromVector3(new Vec3(1f, 0.1f, 0.1f))));
                return null;
            }
            var widget = (Widget)type.GetConstructor(new Type[] { typeof(UIContext) }).Invoke(new object[] { _context });
            parent.AddChild(widget);
            equivalentWidgets.Add(source, widget);
            foreach (var child in source.Children)
            {
                DuplicateWidgetRecursive(child, widget, equivalentWidgets);
            }
            UpdateWidgetProperties(widget);
            return widget;
        }

        /// <summary>
        /// Sets a place-holder value for some common properties, and adds the GauntletView, and WidgetEditorData to the widget
        /// </summary>
        /// <param name="widget"></param>
        private void InitializeWidgetProperties(Widget widget)
        {
            widget.VerticalAlignment = VerticalAlignment.Center;
            widget.HorizontalAlignment = HorizontalAlignment.Center;
            widget.SuggestedHeight = 200;
            widget.SuggestedWidth = 200;
            var parentGauntletView = widget.ParentWidget.GetComponent<GauntletView>();
            var gauntletViewConstructor = AccessTools.Constructor(typeof(GauntletView), new Type[] { typeof(GauntletMovie), typeof(GauntletView), typeof(Widget), typeof(int) });
            var gauntletView = (GauntletView)gauntletViewConstructor.Invoke(new Object[] { _movie, parentGauntletView, widget, 64 });
            widget.AddComponent(gauntletView);
            widget.AddComponent(new WidgetEditorData(widget));
            gauntletView.RefreshBindingWithChildren();
            if (widget is TextWidget text)
            {
                text.Text = "Text";
            }
            else if (widget is RichTextWidget richText)
            {
                richText.Text = "Text";
            }
            else if (widget is FillBar fillBar)
            {
                fillBar.Brush = UIResourceManager.BrushFactory.GetBrush("FillBarBrush");
            }
            else if (widget is CharacterTableauWidget charTableau)
            {
                charTableau.BodyProperties = "<BodyProperties version=\"4\" age=\"21.66\" weight=\"0.4938\" build=\"0.307\"  key=\"001BE40D8000261283CACD43ADCD7C3AB6495819F4BB5ADA30C177898038A892038000350863C503000000000000000000000000000000000000000073182046\"  />";
                charTableau.IsFemale = false;
                charTableau.EquipmentCode = "+0-broad_ild_sword_t3-@null+1-@null-@null+2-@null-@null+3-@null-@null+4-@null-@null+5-@null-@null+6-khuzait_civil_coat-@null+7-leather_shoes-@null+8-@null-@null+9-@null-@null+10-@null-@null+11-@null-@null";
                charTableau.CharStringId = "Character";
                charTableau.StanceIndex = 0;
                charTableau.MountCreationKey = MountCreationKey.GetRandomMountKeyString(null, 0);
                charTableau.ArmorColor1 = 4282552449;
                charTableau.ArmorColor2 = 4293904784;
                charTableau.Race = 0;
            }
            else if (widget is ItemTableauWidget itemTableau)
            {
                itemTableau.StringId = "cotton";
                itemTableau.ItemModifierId = "";
                itemTableau.BannerCode = "";
            }
            else if (widget is BannerTableauWidget bannerTableau)
            {
                bannerTableau.BannerCodeText = "34.25.10.1536.1536.764.764.1.0.0.314.22.35.444.444.764.764.0.0.270";
            }
            // Create place-holder widgets of the appropriate types to prevent any properties from being null.
            // This prevents crashes in widgets that are dependent on other widgets.
            foreach (var propertyInfo in widget.GetType().GetProperties())
            {
                if (propertyInfo.Name == "DragWidget" || propertyInfo.Name == "ParentWidget") continue;
                if (typeof(Widget).IsAssignableFrom(propertyInfo.PropertyType) && propertyInfo.SetMethod != null)
                {
                    var widgetProperty = AddWidget(widget, propertyInfo.PropertyType.Name);
                    // Place-holder ID for the new widget to be obvious in the editor hierarchy
                    widgetProperty.Id = propertyInfo.Name;
                    propertyInfo.SetValue(widget, widgetProperty);
                    // Stores this property as a dependent property in the new widget. If the new widget is removed it will set this
                    // property to null.
                    widgetProperty.EditorAddDependentProperty(widget, propertyInfo);
                }
            }
        }
        private bool IsTableauWidgetType(Type type)
        {
            return typeof(BannerTableauWidget).IsAssignableFrom(type) || typeof(CharacterTableauWidget).IsAssignableFrom(type) || typeof(ItemTableauWidget).IsAssignableFrom(type);
        }
        public void CopyWidgetProperties(Widget widget, Widget source, Dictionary<Widget, Widget> equivalentWidgets)
        {
            // Make sure the source and target are the same type to avoid having different properties
            if (widget.GetType() != source.GetType()) return;
            var parentGauntletView = widget.ParentWidget?.GetComponent<GauntletView>();
            var gauntletViewConstructor = AccessTools.Constructor(typeof(GauntletView), new Type[] { typeof(GauntletMovie), typeof(GauntletView), typeof(Widget), typeof(int) });
            var gauntletView = (GauntletView)gauntletViewConstructor.Invoke(new Object[] { _movie, parentGauntletView, widget, 64 });
            widget.AddComponent(gauntletView);
            var widgetEditorData = source.GetComponent<WidgetEditorData>().CreateCopy(widget, equivalentWidgets);
            widget.AddComponent(widgetEditorData);
            gauntletView.RefreshBindingWithChildren();
            foreach (var propertyInfo in widget.GetType().GetProperties())
            {
                if (propertyInfo.SetMethod == null || propertyInfo.SetMethod.IsPrivate || propertyInfo.GetMethod == null || _excludedWidgetProperties.Contains(propertyInfo.Name)) continue;
                if (propertyInfo.PropertyType == typeof(Brush))
                {
                    var brush = propertyInfo.GetValue(source) as Brush;
                    propertyInfo.SetValue(widget, brush);
                    var newBrush = propertyInfo.GetValue(widget) as Brush;
                    if (newBrush != null && brush != null) newBrush.Name = brush.Name;
                }
                else if (typeof(Widget).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    var propertyWidget = propertyInfo.GetValue(source) as Widget;
                    if (propertyWidget != null)
                    {
                        if (equivalentWidgets.TryGetValue(propertyWidget, out var equivalentWidget));
                        else equivalentWidget = propertyWidget;
                        propertyInfo.SetValue(widget, equivalentWidget);
                    }
                }
                else propertyInfo.SetValue(widget, propertyInfo.GetValue(source));
            }
            if (widget is ListPanel listPanel)
            {
                listPanel.StackLayout.LayoutMethod = (source as ListPanel).StackLayout.LayoutMethod;
            }
            else if (widget is GridWidget grid)
            {
                grid.GridLayout.Direction = (source as GridWidget).GridLayout.Direction;
                grid.GridLayout.HorizontalLayoutMethod = (source as GridWidget).GridLayout.HorizontalLayoutMethod;
                grid.GridLayout.VerticalLayoutMethod = (source as GridWidget).GridLayout.VerticalLayoutMethod;
            }
            else if (widget is TextWidget text)
            {
                text.Text = (source as TextWidget).Text;
            }
        }

        /// <summary>
        /// Removes the widget temporarily without clearing its editor data properties or its clearing its events
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="addUndoOp"></param>
        public void RemoveWidget(Widget widget, bool addUndoOp)
        {
            if (addUndoOp) AddUndoOp(new RemoveWidgetOperation(widget, widget.ParentWidget));
            widget.ParentWidget = null;
            foreach (var child in widget.AllChildrenAndThis)
            {
                child.EditorRemoveAllDependencies();
            }
            SelectWidget(null);
            UpdateWidgetHierarchy();
        }
        /// <summary>
        /// Removes the widget permanently and clears its editor data properties and events
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="addUndoOp"></param>
        public void RemoveWidgetPerma(Widget widget)
        {
            widget.ParentWidget = null;
            var ClearEventHandlersWithChildren = AccessTools.Method(typeof(GauntletView), "ClearEventHandlersWithChildren");
            var view = widget.GetComponent<GauntletView>();
            if (view != null) ClearEventHandlersWithChildren.Invoke(view, new object[] { });
            foreach (var child in widget.AllChildrenAndThis)
            {
                child.EditorGetProperties()?.Clear();
                child.EditorRemoveAllDependencies();
            }
        }
        public void RemoveSelectedWidget()
        {
            if (_selectedWidget == null) return;
            RemoveWidget(_selectedWidget, true);
        }
        public void MoveSelectedWidgetUpChain()
        {
            if (_selectedWidget == null) return;
            var myIndex = _selectedWidget.GetSiblingIndex();
            if (myIndex > 0) _selectedWidget.SetSiblingIndex(myIndex - 1);
            UpdateWidgetHierarchy();
        }
        public void MoveSelectedWidgetDownChain()
        {
            if (_selectedWidget == null) return;
            var myIndex = _selectedWidget.GetSiblingIndex();
            if (myIndex < _selectedWidget.ParentWidget.ChildCount - 1) _selectedWidget.SetSiblingIndex(myIndex + 1);
            UpdateWidgetHierarchy();
        }
        public void ParentSelectedWidget()
        {
            if (_selectedWidget == null) return;
            var myIndex = _selectedWidget.GetSiblingIndex();
            var nextSibling = _selectedWidget.ParentWidget.GetChild(myIndex + 1);
            if (nextSibling == null) return;
            DontDisconnectScopeFromRoot = true;
            var newParent = nextSibling.EditorGetLogicalChildrenLocation();
            var newOffset = newParent.GetLocalPoint(_selectedWidget.GlobalPosition);
            _selectedWidget.ParentWidget = newParent;
            //_selectedWidget.PosOffset = newOffset;
            UpdateWidgetHierarchy();
            DontDisconnectScopeFromRoot = false;
        }
        public void UnparentSelectedWidget()
        {
            if (_selectedWidget == null) return;
            if (_selectedWidget.ParentWidget == _previewRootWidget) return;
            DontDisconnectScopeFromRoot = true;
            var logicalParent = _selectedWidget.ParentWidget.EditorGetIAmLogicalChildLocationFor();
            var grandParent = logicalParent == null ? _selectedWidget.ParentWidget.ParentWidget : logicalParent.ParentWidget;
            var oldParentOffset = _selectedWidget.ParentWidget.PosOffset;
            _selectedWidget.ParentWidget = grandParent;
            //_selectedWidget.PosOffset += oldParentOffset;
            UpdateWidgetHierarchy();
            DontDisconnectScopeFromRoot = false;
        }
        public void DuplicateSelectedWidget()
        {
            if (_selectedWidget == null) return;
            SelectWidget(DuplicateWidget(_selectedWidget));
        }
        public void SelectWidget(Widget widget)
        {
            if (_widgetPickerMode)
            {
                OnWidgetPicked(widget);
                return;
            }
            if (_selectedWidget == widget) return;
            if (_selectedWidget != null) _selectedWidget.EditorSetSelected(false);
            _selectedWidget = widget;
            _selectedWidgetScaler.SelectedWidget = widget;
            if (widget == null)
            {
                SelectedWidgetProperties = _emptyWidgetProperties;
                _selectedWidgetScaler.IsDisabled = true;
                _selectedWidgetScaler.IsVisible = false;
                return;
            }
            _selectedWidget.EditorSetSelected(true);
            // Refresh the properties of the selected widget to reflect any changes
            UpdateWidgetProperties(_selectedWidget);
            FilterPropertiesList();
            //SelectedWidgetProperties = _selectedWidget.EditorGetProperties();
            _selectedWidgetScaler.IsEnabled = true;
            _selectedWidgetScaler.IsVisible = true;
            UpdateSelectedWidgetScaler();
        }
        private void UpdateSelectedWidgetScaler()
        {
            // Updates the position and scale of the box that marks the selected widget
            var newX = _selectedWidget.GlobalPosition.X * _context.InverseScale;
            var newY = _selectedWidget.GlobalPosition.Y * _context.InverseScale;
            _selectedWidgetScaler.PositionXOffset = newX;
            _selectedWidgetScaler.PositionYOffset = newY;
            _selectedWidgetScaler.SuggestedWidth = _selectedWidget.Size.X * _context.InverseScale;
            _selectedWidgetScaler.SuggestedHeight = _selectedWidget.Size.Y * _context.InverseScale;
        }
        
        private void UpdateWidgetProperties(Widget widget)
        {
            if (widget == null) return;
            var widgetPropertiesVM = widget.EditorGetProperties();
            // If property view models have been already added, only update their values
            if (widgetPropertiesVM.Count > 0)
            {
                foreach (var property in widgetPropertiesVM)
                {
                    property.UpdateEditorValueFromObjectProperty();
                }
                return;
            }
            var propList = new MBBindingList<WidgetPropertyVM>();
            if (widget.EditorIsPrefabRoot())
            {
                var values = widget.EditorGetParameterValues();
                for (int i = values.Keys.Count - 1; i >= 0; i--)
                {
                    var param = values.Keys.ElementAt(i);
                    if (!widget.EditorGetParameterBindingData().TryGetValue(param, out var paramBindingData)) continue;
                    propList.Add(new WidgetPropertyVM(prefabWidget: widget, param, paramBindingData));
                }
            }
            else if (widget is ListPanel listPanel)
            {
                propList.Add(new WidgetPropertyVM(widget, AccessTools.Property(typeof(StackLayout), nameof(StackLayout.LayoutMethod)), listPanel.StackLayout));
            }
            else if (widget is GridWidget grid)
            {
                propList.Add(new WidgetPropertyVM(widget, AccessTools.Property(typeof(GridLayout), nameof(GridLayout.Direction)), grid.GridLayout));
                propList.Add(new WidgetPropertyVM(widget, AccessTools.Property(typeof(GridLayout), nameof(GridLayout.VerticalLayoutMethod)), grid.GridLayout));
                propList.Add(new WidgetPropertyVM(widget, AccessTools.Property(typeof(GridLayout), nameof(GridLayout.HorizontalLayoutMethod)), grid.GridLayout));
            }
            var widgetTypeProperties = widget.GetType().GetProperties();
            foreach (var propertyInfo in widgetTypeProperties)
            {
                if (_excludedWidgetProperties.Contains(propertyInfo.Name) || propertyInfo.SetMethod == null || !propertyInfo.SetMethod.IsPublic) continue;
                propList.Add(new WidgetPropertyVM(widget, propertyInfo));
                if (typeof(Brush).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    foreach (var brushPropertyInfo in typeof(Brush).GetProperties())
                    {
                        if (_excludedBrushProperties.Contains(brushPropertyInfo.Name) || brushPropertyInfo.SetMethod == null || !brushPropertyInfo.SetMethod.IsPublic) continue;
                        propList.Add(new WidgetPropertyVM(widget, propertyInfo, brushPropertyInfo));
                    }
                }             
            }
            widget.EditorSetProperties(propList);
        }
        public void SelectWidgetResource(string widgetTypeName)
        {
            if (_selectedWidget == null) SelectWidget(AddWidget(_previewRootWidget, widgetTypeName));
            else
            {
                SelectWidget(AddWidget(_selectedWidget.EditorGetLogicalChildrenLocation(), widgetTypeName));
            }
        }
        public int GetChildLevel(Widget widget, ref int level)
        {
            if (widget.ParentWidget == _previewRootWidget || widget.ParentWidget == null) return level;
            else
            {
                if (widget.EditorIsShownInHierarchy()) level++;
                return GetChildLevel(widget.ParentWidget, ref level);
            }
        }
        public void UpdateWidgetHierarchy()
        {
            WidgetHierarchy.Clear();
            foreach (var child in _previewRootWidget.AllChildren)
            {
                if (!child.EditorIsShownInHierarchy()) continue;
                var wasCollapsed = (child.EditorGetWidgetHierarchyVM() == null) ? false : child.EditorGetWidgetHierarchyVM().IsCollapsed;
                var vm = new WidgetHierarchyVM(child, this);
                WidgetHierarchy.Add(vm);
                vm.IsCollapsed = wasCollapsed;
                child.EditorSetWidgetHierarchyVM(vm);
            }
        }
        public void Tick(float dt)
        {
            if (_selectedWidget == null) return;
            var mousePos = _context.EventManager.MousePosition;
            if (_mouseDown)
            {
                if (_isScaling) HandleScaleSelectedWidget(mousePos);
                else if (Vector2.DistanceSquared(mousePos, _lastClickPosition) > 100)
                {
                    _isDragging = true;
                }
            }
            if (_isDragging)
            {
                HandleDragSelectedWidget(mousePos);
            }
            UpdateSelectedWidgetScaler();
        }
        private void HandleDragSelectedWidget(Vector2 mousePos)
        {
            if (Input.IsKeyDown(InputKey.LeftShift))
            {
                var vecToMouse = mousePos - _lastClickPosition;
                vecToMouse = Vector2.Normalize(vecToMouse);
                if (MathF.Abs(Vector2.Dot(Vector2.UnitX, vecToMouse)) < 0.7f)
                {
                    _selectedWidget.PositionYOffset = _draggedWidgetInitOffset.Y + (mousePos.Y - _lastClickPosition.Y) * _context.InverseScale;
                    _selectedWidget.PositionXOffset = _draggedWidgetInitOffset.X;
                }
                else
                {
                    _selectedWidget.PositionXOffset = _draggedWidgetInitOffset.X + (mousePos.X - _lastClickPosition.X) * _context.InverseScale;
                    _selectedWidget.PositionYOffset = _draggedWidgetInitOffset.Y;
                }
            }
            else _selectedWidget.PosOffset = _draggedWidgetInitOffset + (mousePos - _lastClickPosition) * _context.InverseScale;
        }
        private void HandleScaleSelectedWidget(Vector2 mousePos)
        {
            var deltaMouseX = (mousePos.X - _lastClickPosition.X) * _context.InverseScale;
            var deltaMouseY = (mousePos.Y - _lastClickPosition.Y) * _context.InverseScale;
            if (Input.IsKeyDown(InputKey.LeftShift))
            {
                if (_scalingRight || _scalingDown) _scalingRight = _scalingDown = true;
                else if (_scalingLeft || _scalingUp) _scalingLeft = _scalingUp = true;
                var aspectRatio = _scaledWidgetInitSize.X / _scaledWidgetInitSize.Y;
                deltaMouseY = deltaMouseX / aspectRatio;
            }
            if (_scalingRight)
            {
                var newScale = _scaledWidgetInitSize.X * _context.InverseScale + deltaMouseX;
                _selectedWidget.SuggestedWidth = MathF.Abs(newScale);
                switch (_selectedWidget.HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        _selectedWidget.PositionXOffset = _draggedWidgetInitOffset.X + deltaMouseX / 2f;
                        break;
                    case HorizontalAlignment.Right:
                        _selectedWidget.PositionXOffset = _draggedWidgetInitOffset.X + deltaMouseX;
                        break;
                }
            }
            else if (_scalingLeft)
            {
                var newScale = _scaledWidgetInitSize.X * _context.InverseScale - deltaMouseX;
                _selectedWidget.SuggestedWidth = MathF.Abs(newScale);
                switch (_selectedWidget.HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        _selectedWidget.PositionXOffset = _draggedWidgetInitOffset.X + deltaMouseX / 2f;
                        break;
                    case HorizontalAlignment.Left:
                        _selectedWidget.PositionXOffset = _draggedWidgetInitOffset.X + deltaMouseX;
                        break;

                }
            }
            if (_scalingDown)
            {
                var newScale = _scaledWidgetInitSize.Y * _context.InverseScale + deltaMouseY;
                _selectedWidget.SuggestedHeight = MathF.Abs(newScale);
                switch (_selectedWidget.VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                        _selectedWidget.PositionYOffset = _draggedWidgetInitOffset.Y + deltaMouseY / 2f;
                        break;
                    case VerticalAlignment.Bottom:
                        _selectedWidget.PositionYOffset = _draggedWidgetInitOffset.Y + deltaMouseY;
                        break;

                }
            }
            else if (_scalingUp)
            {
                var newScale = _scaledWidgetInitSize.Y * _context.InverseScale - deltaMouseY;
                _selectedWidget.SuggestedHeight = MathF.Abs(newScale);
                switch (_selectedWidget.VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                        _selectedWidget.PositionYOffset = _draggedWidgetInitOffset.Y + deltaMouseY / 2f;
                        break;
                    case VerticalAlignment.Top:
                        _selectedWidget.PositionYOffset = _draggedWidgetInitOffset.Y + deltaMouseY;
                        break;

                }
            }
        }
        public void OnMousePressed()
        {
            var mousePos = _context.EventManager.MousePosition;
            if (!_previewRootWidget.IsPointInsideMeasuredArea(mousePos)) return;
            _mouseDown = true;
            Widget clickedWidget = null;
            foreach (var editWidget in _previewRootWidget.AllChildren)
            {
                if (editWidget.EditorIsLocked()) continue;
                if (editWidget.IsVisible && editWidget.IsPointInsideMeasuredArea(mousePos))
                {
                    clickedWidget = editWidget;
                }
            }
            if (clickedWidget != null)
            {
                _draggedWidgetInitOffset = clickedWidget.PosOffset;
                _scaledWidgetInitSize = clickedWidget.Size;
                var localMousePos = clickedWidget.GetLocalPoint(mousePos);
                if (clickedWidget.WidthSizePolicy == SizePolicy.Fixed)
                {
                    var scalingMarginX = MathF.Min(_WIDGET_SCALING_MINIMUM_MARGIN, clickedWidget.Size.X * _context.InverseScale / 4f);
                    if (localMousePos.X < scalingMarginX) _scalingLeft = true;
                    else if (localMousePos.X > clickedWidget.Size.X - scalingMarginX) _scalingRight = true;
                }
                if (clickedWidget.HeightSizePolicy == SizePolicy.Fixed)
                {
                    var scalingMarginY = MathF.Min(_WIDGET_SCALING_MINIMUM_MARGIN, clickedWidget.Size.Y * _context.InverseScale / 4f);
                    if (localMousePos.Y < scalingMarginY) _scalingUp = true;
                    else if (localMousePos.Y > clickedWidget.Size.Y - scalingMarginY) _scalingDown = true;
                }
            }
            SelectWidget(clickedWidget);
            _lastClickPosition = mousePos;
        }

        private void OnWidgetPicked(Widget pickedWidget)
        {
            if (_widgetPickerCurrentPropertyVM != null)
            {
                _widgetPickerCurrentPropertyVM.SetPickedWidget(pickedWidget);
                _widgetPickerCurrentPropertyVM = null;
            }
            _widgetPickerMode = false;
        }

        public void OnMouseReleased()
        {
            if (_isDragging)
            {

                UpdateWidgetProperties(_selectedWidget);
                FilterPropertiesList();
                AddUndoOp(new MoveWidgetOperation(_selectedWidget, _draggedWidgetInitOffset));
            }
            else if (_isScaling)
            {
                UpdateWidgetProperties(_selectedWidget);
                FilterPropertiesList();
                AddUndoOp(new RescaleWidgetOperation(_selectedWidget, _draggedWidgetInitOffset, _scaledWidgetInitSize, _context));
            }
            _mouseDown = false;
            _isDragging = false;
            _scalingRight = false;
            _scalingLeft = false;
            _scalingUp = false;
            _scalingDown = false;
        }
        
        public void EnterWidgetPickerMode(WidgetPropertyVM propertiesVM)
        {
            _widgetPickerCurrentPropertyVM = propertiesVM;
            _widgetPickerMode = true;
        }
        public void EnterWidgetPickerModeFromCreation()
        {
            _widgetPickerMode = true;
        }
        public void SelectedWidgetOffsetToMargins()
        {
            if (_selectedWidget == null) return;
            var correctedParentSize = _selectedWidget.ParentWidget.Size * _context.InverseScale;
            var correctedSize = _selectedWidget.Size * _context.InverseScale;
            var xOffsetRegardlessOfAlignment = _selectedWidget.PositionXOffset;
            if (_selectedWidget.HorizontalAlignment == HorizontalAlignment.Center)
            {
                xOffsetRegardlessOfAlignment += correctedParentSize.X / 2f;
            }
            else if (_selectedWidget.HorizontalAlignment == HorizontalAlignment.Right)
            {
                xOffsetRegardlessOfAlignment += correctedParentSize.X;
            }
            var yOffsetRegardlessOfAlignment = _selectedWidget.PositionYOffset;
            if (_selectedWidget.VerticalAlignment == VerticalAlignment.Center)
            {
                yOffsetRegardlessOfAlignment += correctedParentSize.Y / 2f;
            }
            else if (_selectedWidget.VerticalAlignment == VerticalAlignment.Bottom)
            {
                yOffsetRegardlessOfAlignment += correctedParentSize.Y;
            }
            if (_selectedWidget.LocalPosition.X < _selectedWidget.ParentWidget.Size.X)
            {

                _selectedWidget.MarginLeft = xOffsetRegardlessOfAlignment;
                _selectedWidget.MarginRight = 0;
                _selectedWidget.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                _selectedWidget.MarginLeft = 0;
                _selectedWidget.MarginRight = correctedParentSize.X - xOffsetRegardlessOfAlignment;
                _selectedWidget.HorizontalAlignment = HorizontalAlignment.Right;
            }
            if (_selectedWidget.LocalPosition.Y < _selectedWidget.ParentWidget.Size.Y)
            {

                _selectedWidget.MarginTop = yOffsetRegardlessOfAlignment;
                _selectedWidget.MarginBottom = 0;
                _selectedWidget.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                _selectedWidget.MarginTop = 0;
                _selectedWidget.MarginBottom = correctedParentSize.Y - yOffsetRegardlessOfAlignment;
                _selectedWidget.VerticalAlignment = VerticalAlignment.Bottom;
            }
            _selectedWidget.PosOffset = Vector2.Zero;
        }

        public string GetWidgetPathFromWidget(Widget widget, Widget target, bool forExporting)
        {
            if (widget.AllChildren.Contains(target)) return GetChildPathFromWidget(widget, target, forExporting);
            else if (widget.ParentWidget != null) return "..\\" + GetWidgetPathFromWidget(widget.ParentWidget, target, forExporting);
            else return "";
        }
        private string GetChildPathFromWidget(Widget widget, Widget child, bool forExporting)
        {
            var myName = string.IsNullOrEmpty(child.Id) || (forExporting && widget == _previewRootWidget)? child.GetType().Name : child.Id;
            StringBuilder stringBuilder = new StringBuilder(myName);
            for (Widget parentWidget = child.ParentWidget; parentWidget != widget && parentWidget != null; parentWidget = parentWidget.ParentWidget)
            {
                stringBuilder.Insert(0, (string.IsNullOrEmpty(parentWidget.Id) ? parentWidget.GetType().Name : parentWidget.Id) + "\\");
            }
            return stringBuilder.ToString();
        }

        public void AddUndoOp(UIEditorOperation op)
        {
            foreach (var redoOp in _redoOperationsStack)
            {
                if (redoOp is RemoveWidgetOperation remove) RemoveWidgetPerma(remove.RemovedWidget);
            }
            _redoOperationsStack.Clear();
            var removedOp = _undoOperationsStack.Push(op);
            if (removedOp is RemoveWidgetOperation remove2) RemoveWidgetPerma(remove2.RemovedWidget);
        }
        public void Undo()
        {
            if (_undoOperationsStack.Count <= 0) return;
            var lastOp = _undoOperationsStack.Pop();
            var removedRedo = _redoOperationsStack.Push(lastOp.GetRedoOp());
            if (removedRedo is RemoveWidgetOperation remove) RemoveWidgetPerma(remove.RemovedWidget);
            lastOp.Undo();
        }
        public void Redo()
        {
            if (_redoOperationsStack.Count <= 0) return;
            var redoneOp = _redoOperationsStack.Pop();
            _undoOperationsStack.Push(redoneOp.GetRedoOp());
            redoneOp.Undo();
        }

        private UIEditorScreen _ownerScreen;
        private Widget _selectedWidget;
        private MBBindingList<WidgetPropertyVM> _selectedWidgetProperties;
        private MBBindingList<WidgetPropertyVM> _emptyWidgetProperties = new MBBindingList<WidgetPropertyVM>();

        private Vector2 _draggedWidgetInitOffset;
        private Vector2 _lastClickPosition;
        private bool _mouseDown;
        private bool _isDragging;
        private Vector2 _scaledWidgetInitSize;
        private bool _isScaling { get => _scalingDown || _scalingUp || _scalingLeft || _scalingRight; }
        private bool _scalingRight = false;
        private bool _scalingLeft = false;
        private bool _scalingUp = false;
        private bool _scalingDown = false;
        private EditorWigetScalerWidget _selectedWidgetScaler;

        private MBBindingList<WidgetResourceVM> _availableBasicWidgetNames;
        private MBBindingList<WidgetResourceVM> _availablePrefabWidgetNames;
        private MBBindingList<WidgetResourceVM> _unfilteredBasicWidgetNames;
        private MBBindingList<WidgetResourceVM> _unfilteredPrefabWidgetNames;
        private string _basicWidgetsSearchText;
        private string _prefabWidgetsSearchText;
        private string _propertySearchText;

        private MBBindingList<WidgetHierarchyVM> _widgetHierarchy;

        private bool _widgetPickerMode;
        private WidgetPropertyVM _widgetPickerCurrentPropertyVM;

        private Widget _previewRootWidget;
        private UIContext _context;
        private WidgetFactory _widgetFactory;
        private GauntletMovie _movie;

        private bool _dontDisconnectScopeFromRoot;

        private List<string> _excludedWidgetProperties;
        private List<string> _excludedBrushProperties;
        public const float _WIDGET_SCALING_MINIMUM_MARGIN = 40f;

        private DropOutStack<UIEditorOperation> _undoOperationsStack;
        private DropOutStack<UIEditorOperation> _redoOperationsStack;
        private const int MAX_UNDO_COUNT = 20;
    }
}
