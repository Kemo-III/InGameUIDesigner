using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace InGameUIDesigner
{
    public class EditorWidgetScalerWidget : Widget
    {
        private const float _WIDGET_SCALING_MINIMUM_MARGIN = 40f;
        public Widget SelectedWidget = null;

        public EditorWidgetScalerWidget(UIContext context) : base(context)
        {
        }

        protected override void OnUpdate(float dt)
        {
            if (UIEditorVM.Instance.WidgetPickerMode)
            {
                Context.ActiveCursorOfContext = UIContext.MouseCursors.RightClickLink;
                return;
            }
            if (SelectedWidget == null || SelectedWidget.EditorIsLocked()) return;
            var localMousePos = GetLocalPoint(Context.EventManager.MousePosition);
            if (localMousePos.X < 0 || localMousePos.X > Size.X || localMousePos.Y < 0 || localMousePos.Y > Size.Y) return;

            var scalingMarginX = MathF.Min(_WIDGET_SCALING_MINIMUM_MARGIN, Size.X * Context.InverseScale / 4f);
            var scalingMarginY = MathF.Min(_WIDGET_SCALING_MINIMUM_MARGIN, Size.Y * Context.InverseScale / 4f);
            var canScaleX = SelectedWidget.WidthSizePolicy == SizePolicy.Fixed;
            var canScaleY = SelectedWidget.HeightSizePolicy == SizePolicy.Fixed;
            if (localMousePos.X < scalingMarginX && canScaleX)
            {
                if (localMousePos.Y < scalingMarginY && canScaleY)
                    Context.ActiveCursorOfContext = UIContext.MouseCursors.DiagonalLeftResize;
                else if (localMousePos.Y > Size.Y - scalingMarginY && canScaleY)
                    Context.ActiveCursorOfContext = UIContext.MouseCursors.DiagonalRightResize;
                else
                    Context.ActiveCursorOfContext = UIContext.MouseCursors.HorizontalResize;
            }
            else if (localMousePos.X > Size.X - scalingMarginX && canScaleX)
            {
                if (localMousePos.Y < scalingMarginY && canScaleY)
                    Context.ActiveCursorOfContext = UIContext.MouseCursors.DiagonalRightResize;
                else if (localMousePos.Y > Size.Y - scalingMarginY && canScaleY)
                    Context.ActiveCursorOfContext = UIContext.MouseCursors.DiagonalLeftResize;
                else
                    Context.ActiveCursorOfContext = UIContext.MouseCursors.HorizontalResize;
            }
            else if (canScaleY && (localMousePos.Y < scalingMarginY || localMousePos.Y > Size.Y - scalingMarginY))
                Context.ActiveCursorOfContext = UIContext.MouseCursors.VerticalResize;
            else if (localMousePos.X > 0 && localMousePos.X < Size.X && localMousePos.Y > 0 && localMousePos.Y < Size.Y)
                Context.ActiveCursorOfContext = UIContext.MouseCursors.Move;
        }
    }
}
