extern alias MountAndBlade;
using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using EditorAttribute = TaleWorlds.GauntletUI.EditorAttribute;
using MountAndBlade.System.Numerics;
using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.GauntletUI.Data;
using JetBrains.Annotations;
using TaleWorlds.MountAndBlade;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using System.Net;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using System.Runtime.InteropServices;
using TaleWorlds.Engine;
using System.Windows.Controls.Primitives;
using System.CodeDom;
using static TaleWorlds.Core.ItemCategory;
using System.Net.NetworkInformation;
using TaleWorlds.CampaignSystem.ViewModelCollection.Barter;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using System.Windows.Forms;
using HorizontalAlignment = TaleWorlds.GauntletUI.HorizontalAlignment;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Text;
using System.Diagnostics;

namespace InGameUIDesigner
{
    public class UIEditorPopUpsVM : ViewModel
    {
        [DataSourceProperty]
        public bool PopUpsEnabled
        {
            get => _popUpsEnabled;
            set
            {
                if (value != _popUpsEnabled)
                {
                    _popUpsEnabled = value;
                    OnPropertyChangedWithValue(value, "PopUpsEnabled");
                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<BrushItemVM> PreviewBrushList
        {
            get => _previewBrushList;
            set
            {
                if (value != _previewBrushList)
                {
                    _previewBrushList = value;
                    OnPropertyChangedWithValue(value, "PreviewBrushList");
                }
            }
        }

        [DataSourceProperty]
        public bool ShowPreviewBrushList
        {
            get => _showPreviewBrushList;
            set
            {
                if (value != _showPreviewBrushList)
                {
                    _showPreviewBrushList = value;
                    OnPropertyChangedWithValue(value, "ShowPreviewBrushList");
                }
            }
        }
        [DataSourceProperty]
        public string BrushSearchText
        {
            get => _brushSearchText;
            set
            {
                if (value != _brushSearchText)
                {
                    _brushSearchText = value;
                    OnPropertyChangedWithValue(value, "BrushSearchText");
                    FilterBrushList();
                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<SpriteItemVM> PreviewSpriteList
        {
            get => _previewSpriteList;
            set
            {
                if (value != _previewSpriteList)
                {
                    _previewSpriteList = value;
                    OnPropertyChangedWithValue(value, "PreviewSpriteList");
                }
            }
        }
        [DataSourceProperty]
        public bool ShowPreviewSpritesList
        {
            get => _showPreviewSpritesList;
            set
            {
                if (value != _showPreviewSpritesList)
                {
                    _showPreviewSpritesList = value;
                    OnPropertyChangedWithValue(value, "ShowPreviewSpritesList");
                }
            }
        }
        [DataSourceProperty]
        public string SpriteSearchText
        {
            get => _spriteSearchText;
            set
            {
                if (value != _spriteSearchText)
                {
                    _spriteSearchText = value;
                    OnPropertyChangedWithValue(value, "SpriteSearchText");
                    FilterSpriteList();
                }
            }
        }
        [DataSourceProperty]
        public bool ShowColourPicker
        {
            get => _showColourPicker;
            set
            {
                if (value != _showColourPicker)
                {
                    _showColourPicker = value;
                    OnPropertyChangedWithValue(value, "ShowColourPicker");
                }
            }
        }
        [DataSourceProperty]
        public ColourPickerVM ColourPickerDataSource
        {
            get => _colourPickerDataSource;
            set
            {
                if (value != _colourPickerDataSource)
                {
                    _colourPickerDataSource = value;
                    OnPropertyChangedWithValue(value, "ColourPickerDataSource");
                }
            }
        }

        public UIEditorPopUpsVM()
        {
            PopUpsEnabled = false;
            ShowPreviewBrushList = false;
            _unfilteredBrushList = new MBBindingList<BrushItemVM>();
            PreviewBrushList = new MBBindingList<BrushItemVM>();
            foreach (var brush in UIResourceManager.BrushFactory.Brushes)
            {
                _unfilteredBrushList.Add(new BrushItemVM(brush));
            }
            PreviewBrushList = _unfilteredBrushList;
            ShowPreviewSpritesList = false;
            _unfilteredSpriteList = new MBBindingList<SpriteItemVM>();
            PreviewSpriteList = new MBBindingList<SpriteItemVM>();
            foreach (var sprite in UIResourceManager.SpriteData.SpriteNames.Values)
            {
                _unfilteredSpriteList.Add(new SpriteItemVM(sprite));
            }
            PreviewSpriteList = _unfilteredSpriteList;
            ShowColourPicker = false;
            ColourPickerDataSource = new ColourPickerVM(this);
        }

        public void SetColourPickerWidget(Widget hueSaturationPickerWidget)
        {
            ColourPickerDataSource.SetColourPickerWidget(hueSaturationPickerWidget);
        }
        public void ShowBrushList()
        {
            PopUpsEnabled = true;
            ShowPreviewSpritesList = false;
            ShowPreviewBrushList = true;
            ShowColourPicker = false;
        }
        public void ShowSpriteList()
        {
            PopUpsEnabled = true;
            ShowPreviewBrushList = false;
            ShowPreviewSpritesList = true;
            ShowColourPicker = false;
        }
        public void ShowColourPickerMethod(WidgetPropertyVM propertyVM)
        {
            PopUpsEnabled = true;
            ShowPreviewBrushList = false;
            ShowPreviewSpritesList = false;
            ShowColourPicker = true;
            ColourPickerProperty = propertyVM;
            if (!ColourPickerProperty.UsesColourPicker || ColourPickerProperty.TextboxValue.Length != "#FFFFFFFF".Length) return;
            var color = Color.ConvertStringToColor(ColourPickerProperty.TextboxValue);
            ColourPickerDataSource.FinalColour = color;
            ColourPickerDataSource.UpdateHSVFromColour();
        }
        public void ClosePopUps()
        {
            PopUpsEnabled = false;
            ShowPreviewBrushList = false;
            ShowPreviewSpritesList = false;
            ShowColourPicker = false;
            ColourPickerProperty = null;
        }
        public void OnColourPicked(Color colour)
        {
            if (ColourPickerProperty == null) return;
            ColourPickerProperty.TextboxValue = colour.ToString();
        }
        public void ShowExitConfirmation()
        {
            var inq = new InquiryData("Are you sure?", "Are you sure you want to exit? Any unsaved data will be lost.",
                true, true, "Exit", "Cancel", ConfirmExit, null);
            InformationManager.ShowInquiry(inq);
        }
        public void ConfirmExit()
        {
            GameStateManager.Current.PopState();
        }
        private void FilterBrushList()
        {
            if (BrushSearchText.Length < 3)
            {
                PreviewBrushList = _unfilteredBrushList;
                return;
            }
            PreviewBrushList = new MBBindingList<BrushItemVM>();
            foreach (var item in _unfilteredBrushList)
            {
                if (item.BrushName.ToLower().Contains(BrushSearchText.ToLower())) PreviewBrushList.Add(item);
            }
        }
        private void FilterSpriteList()
        {
            if (SpriteSearchText.Length < 3)
            {
                PreviewSpriteList = _unfilteredSpriteList;
                return;
            }
            PreviewSpriteList = new MBBindingList<SpriteItemVM>();
            foreach (var item in _unfilteredSpriteList)
            {
                if (item.SpriteName.ToLower().Contains(SpriteSearchText.ToLower())) PreviewSpriteList.Add(item);
            }
        }

        private bool _popUpsEnabled;
        private bool _showPreviewBrushList;
        private MBBindingList<BrushItemVM> _previewBrushList;
        private MBBindingList<BrushItemVM> _unfilteredBrushList;
        private string _brushSearchText;
        private bool _showPreviewSpritesList;
        private MBBindingList<SpriteItemVM> _previewSpriteList;
        private MBBindingList<SpriteItemVM> _unfilteredSpriteList;
        private string _spriteSearchText;
        private bool _showColourPicker;
        private ColourPickerVM _colourPickerDataSource;
        private WidgetPropertyVM ColourPickerProperty;
    }
}
