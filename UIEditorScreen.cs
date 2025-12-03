using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;
using TaleWorlds.MountAndBlade.View.Screens;
using System.Windows.Forms;
using Path = System.IO.Path;

namespace InGameUIDesigner
{
    [GameStateScreen(typeof(UIEditorState))]
    public class UIEditorScreen : ScreenBase, IGameStateListener
    {
        public UIEditorScreen(UIEditorState state)
        {
            _editorState = state;
            _editorState.RegisterListener(this);
            _alreadyLoadedCategories = new List<SpriteCategory>();

            _saveFile = new SaveFileDialog();
            _saveFile.Title = "Export Prefab";
            _saveFile.FileName = "prefab";
            _saveFile.DefaultExt = ".xml";
            _saveFile.CreatePrompt = false;
            _saveFile.OverwritePrompt = true;
            _saveFile.Filter = "XML File (.xml)|*.xml";
            _saveFile.InitialDirectory = Path.GetFullPath(BasePath.Name + "Modules");
            _saveFile.AddExtension = true;

            _openFile = new OpenFileDialog();
            _openFile.Title = "Import Prefab";
            _openFile.FileName = "";
            _openFile.DefaultExt = ".xml";
            _openFile.Filter = "XML File (.xml)|*.xml";
            _openFile.InitialDirectory = Path.GetFullPath(BasePath.Name + "Modules");
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);
            if (_editorDataSource == null || _editorLayer == null) return;
            if (!_popupDataSource.PopUpsEnabled)
            {
                if (_editorLayer.Input.IsKeyPressed(InputKey.LeftMouseButton)) _editorDataSource.OnMousePressed();
                if (_editorLayer.Input.IsKeyReleased(InputKey.LeftMouseButton)) _editorDataSource.OnMouseReleased();
                if (_editorLayer.Input.IsKeyPressed(InputKey.Z) && _editorLayer.Input.IsKeyDown(InputKey.LeftControl))
                {
                    if (_editorLayer.Input.IsKeyDown(InputKey.LeftShift)) _editorDataSource.Redo();
                    else _editorDataSource.Undo();
                }
            }
            if (_editorLayer.Input.IsHotKeyPressed(_doneKey.Id))
            {
                OpenExportPrompt();
            }
            else if (_editorLayer.Input.IsHotKeyPressed(_exitKey.Id))
            {
                if (_popupDataSource.PopUpsEnabled) HideBrushAndSpritePreviewList();
                else OpenExitPrompt();
            }
            _editorDataSource.Tick(dt);
        }
        void IGameStateListener.OnActivate()
        {
            _doneKey = HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm");
            _exitKey = HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit");
            _alreadyLoadedCategories.Clear();
            foreach (var spriteCategory in UIResourceManager.SpriteData.SpriteCategories.Values)
            {
                if (spriteCategory.IsLoaded) _alreadyLoadedCategories.Add(spriteCategory);
                else spriteCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);
            }
            _editorDataSource = new UIEditorVM(this);
            _editorLayer = new GauntletLayer("UIEditorLayer", 3000, true);
            _editorMovie = _editorLayer.LoadMovie("UIEditor", _editorDataSource).Movie;
            _editorLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _editorLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _editorLayer.IsFocusLayer = true;
            AddLayer(_editorLayer);
            ScreenManager.TrySetFocus(_editorLayer);
            if (BannerlordConfig.ForceVSyncInMenus)
            {
                Utilities.SetForceVsync(true);
            }
            InitializeVM();

            _popupDataSource = new UIEditorPopUpsVM();
            _popUpLayer = new GauntletLayer("UIEditorPopUpsLayer", 4000, true);
            _popUpMovie = _popUpLayer.LoadMovie("UIEditor.PopUps", _popupDataSource).Movie;
            _popupDataSource.SetColourPickerWidget(_popUpMovie.RootWidget.FindChild("HueSaturationPicker", true));
            _popUpLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            AddLayer(_popUpLayer);
        }

        void IGameStateListener.OnDeactivate()
        {
            Utilities.SetForceVsync(false);
            _editorLayer.InputRestrictions.ResetInputRestrictions();
            RemoveLayer(_editorLayer);
            _editorLayer = null;
            _popUpLayer.InputRestrictions.ResetInputRestrictions();
            RemoveLayer(_popUpLayer);
            _popUpLayer = null;
            foreach (var spriteCategory in UIResourceManager.SpriteData.SpriteCategories.Values)
            {
                if (_alreadyLoadedCategories.Contains(spriteCategory)) continue;
                spriteCategory.Unload();
            }

        }

        void IGameStateListener.OnInitialize()
        {
               
        }

        void IGameStateListener.OnFinalize()
        {
            
        }

        public void OpenExportPrompt()
        {
            if (_saveFile.ShowDialog() == DialogResult.OK)
            {
                _editorDataSource.ExportPrefab(_saveFile.FileName);
            }
        }
        public void OpenExitPrompt()
        {
            _popupDataSource.ShowExitConfirmation();
        }
        public void ShowBrushPreviewList()
        {
            _popupDataSource.ShowBrushList();
        }
        public void ShowSpritePreviewList()
        {
            _popupDataSource.ShowSpriteList();
        }
        public void HideBrushAndSpritePreviewList()
        {
            _popupDataSource.ClosePopUps();
        }
        public void ShowColourPicker(WidgetPropertyVM property)
        {
            _popupDataSource.ShowColourPickerMethod(property);
        }

        private void InitializeVM()
        {
            var previewRootWidget = _editorMovie.RootWidget.FindChild("PreviewRootWidget", true);
            _editorDataSource.Initialize(previewRootWidget, _editorLayer.UIContext, UIResourceManager.WidgetFactory, (GauntletMovie)_editorMovie);
        }

        private UIEditorState _editorState;
        private UIEditorVM _editorDataSource;
        private GauntletLayer _editorLayer;
        private IGauntletMovie _editorMovie;
        private UIEditorPopUpsVM _popupDataSource;
        private GauntletLayer _popUpLayer;
        private IGauntletMovie _popUpMovie;
        private SaveFileDialog _saveFile;
        private OpenFileDialog _openFile;
        private HotKey _doneKey;
        private HotKey _exitKey;
        private List<SpriteCategory> _alreadyLoadedCategories;
    }
}
