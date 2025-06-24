using System;
using System.Windows.Forms;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace InGameUIDesigner
{
    public class BrushItemVM : ViewModel
    {
        [DataSourceProperty]
        public string BrushName
        {
            get
            {
                return _brushName;
            }
            set
            {
                if (value != _brushName)
                {
                    _brushName = value;
                    OnPropertyChangedWithValue(value, "BrushName");
                }
            }
        }
        [DataSourceProperty]
        public Brush Brush
        {
            get
            {
                return _brush;
            }
            set
            {
                if (value != _brush)
                {
                    _brush = value;
                    OnPropertyChangedWithValue(value, "Brush");
                }
            }
        }

        public void CopyNameToClipboard()
        {
            Clipboard.SetText(BrushName);
            InformationManager.DisplayMessage(new InformationMessage("Copied brush name to clipboard: " + BrushName));
        }

        public BrushItemVM(Brush brush)
        {
            Brush = brush;
            BrushName = brush.Name;
        }

        private string _brushName;
        private Brush _brush;
    }

    public class SpriteItemVM : ViewModel
    {
        [DataSourceProperty]
        public string SpriteName
        {
            get
            {
                return _spriteName;
            }
            set
            {
                if (value != _spriteName)
                {
                    _spriteName = value;
                    OnPropertyChangedWithValue(value, "SpriteName");
                }
            }
        }
        [DataSourceProperty]
        public Sprite Sprite
        {
            get
            {
                return _sprite;
            }
            set
            {
                if (value != _sprite)
                {
                    _sprite = value;
                    OnPropertyChangedWithValue(value, "Sprite");
                }
            }
        }

        public void CopyNameToClipboard()
        {
            Clipboard.SetText(SpriteName);
            InformationManager.DisplayMessage(new InformationMessage("Copied sprite name to clipboard: " + SpriteName));
        }
        public SpriteItemVM(Sprite sprite)
        {
            Sprite = sprite;
            SpriteName = sprite.Name;
        }

        private string _spriteName;
        private Sprite _sprite;
    }
}
