using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace InGameUIDesigner
{
    public class DummyVM : ViewModel
    {
        private bool _dummyBool;
        private string _dummyBoolAsString;
        private string _dummyText;
        private float _dummyNumber;
        private Sprite _dummySprite;
        private string _dummySpriteName;
        private Brush _dummyBrush;
        private string _dummyBrushName;
        private MBBindingList<DummyVM> _childViewModels;

        public DummyVM()
        {
            DummyBool = false;
            DummyText = "VM Text";
            DummyNumber = 0f;
            DummySpriteName = String.Empty;
            DummyBrushName = String.Empty;
            ChildViewModels = new MBBindingList<DummyVM>();
        }

        [DataSourceProperty]
        public bool DummyBool
        {
            get => _dummyBool;
            set
            {
                if (value != _dummyBool)
                {
                    _dummyBool = value;
                    OnPropertyChangedWithValue(value, "DummyBool");
                }
            }
        }
        [DataSourceProperty]
        public string DummyBoolAsString
        {
            get => _dummyBoolAsString;
            set
            {
                if (value != _dummyBoolAsString)
                {
                    _dummyBoolAsString = value;
                    OnPropertyChangedWithValue(value, "DummyBoolAsString");
                    if (bool.TryParse(_dummyBoolAsString, out var newBoolValue))
                    {
                        DummyBool = newBoolValue;
                    }
                }
            }
        }
        [DataSourceProperty]
        public string DummyText
        {
            get => _dummyText;
            set
            {
                if (value != _dummyText)
                {
                    _dummyText = value;
                    OnPropertyChangedWithValue(value, "DummyText");
                }
            }
        }
        [DataSourceProperty]
        public float DummyNumber
        {
            get => _dummyNumber;
            set
            {
                if (value != _dummyNumber)
                {
                    _dummyNumber = value;
                    OnPropertyChangedWithValue(value, "DummyNumber");
                }
            }
        }
        [DataSourceProperty]
        public Sprite DummySprite
        {
            get => _dummySprite;
            set
            {
                if (value != _dummySprite)
                {
                    _dummySprite = value;
                    OnPropertyChangedWithValue(value, "DummySprite");
                }
            }
        }
        [DataSourceProperty]
        public string DummySpriteName
        {
            get => _dummySpriteName;
            set
            {
                if (value != _dummySpriteName)
                {
                    _dummySpriteName = value;
                    OnPropertyChangedWithValue(value, "DummySpriteName");
                    DummySprite = UIResourceManager.SpriteData.GetSprite(_dummySpriteName);
                }
            }
        }
        [DataSourceProperty]
        public Brush DummyBrush
        {
            get => _dummyBrush;
            set
            {
                if (value != _dummyBrush)
                {
                    _dummyBrush = value;
                    OnPropertyChangedWithValue(value, "DummyBrush");
                }
            }
        }
        [DataSourceProperty]
        public string DummyBrushName
        {
            get => _dummyBrushName;
            set
            {
                if (value != _dummyBrushName)
                {
                    _dummyBrushName = value;
                    OnPropertyChangedWithValue(value, "DummyBrushName");
                    var newBrush = UIResourceManager.BrushFactory.GetBrush(_dummyBrushName);
                    if (newBrush == null) DummyBrush = UIResourceManager.BrushFactory.DefaultBrush;
                    else DummyBrush = newBrush;
                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<DummyVM> ChildViewModels
        {
            get => _childViewModels;
            set
            {
                if (value != _childViewModels)
                {
                    _childViewModels = value;
                    OnPropertyChangedWithValue(value, "ChildViewModels");
                }
            }
        }
    }
}
