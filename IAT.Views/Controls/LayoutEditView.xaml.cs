using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using IAT.ViewModels;

namespace IAT.Views.Controls
{
    public partial class LayoutEditView : UserControl
    {
        private const double MinElementSize = 24.0;

        private bool _isDraggingElement;
        private string? _dragElementTag;
        private Point _dragStartInCanvas;
        private double _dragOriginX;
        private double _dragOriginY;

        public LayoutEditView()
        {
            InitializeComponent();
            Loaded += (_, _) => TryFitToHost();
            DataContextChanged += (_, _) => TryFitToHost();
        }

        private void OnPreviewHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is LayoutViewModel vm && e.NewSize.Width > 0 && e.NewSize.Height > 0)
                vm.FitToWindowCommand.Execute(e.NewSize);
        }

        private void TryFitToHost()
        {
            if (DataContext is not LayoutViewModel vm)
                return;
            if (ActualWidth > 1 && ActualHeight > 1)
                vm.FitToWindowCommand.Execute(new Size(Math.Max(100, ActualWidth - 32), Math.Max(100, ActualHeight - 32)));
        }

        private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is not Thumb thumb || thumb.Tag is not string tag)
                return;

            var layoutVm = ResolveLayoutViewModel(thumb);
            if (layoutVm is null)
                return;

            var parts = tag.Split('_');
            if (parts.Length != 2)
                return;

            string element = parts[0];
            string corner = parts[1];

            double scale = layoutVm.ScaleFactor > 0 ? layoutVm.ScaleFactor : 1.0;
            double dx = e.HorizontalChange / scale;
            double dy = e.VerticalChange / scale;

            bool isRightSide = corner is "NE" or "SE";
            bool isBottomSide = corner is "SW" or "SE";

            double widthDelta = isRightSide ? dx : -dx;
            double heightDelta = isBottomSide ? dy : -dy;

            switch (element)
            {
                case "Stimulus":
                    layoutVm.StimulusWidth = Math.Max(MinElementSize, layoutVm.StimulusWidth + widthDelta);
                    layoutVm.StimulusHeight = Math.Max(MinElementSize, layoutVm.StimulusHeight + heightDelta);
                    layoutVm.StimulusX -= widthDelta / 2;
                    layoutVm.StimulusY -= heightDelta / 2;
                    break;

                case "BlockInstructions":
                    layoutVm.BlockInstructionsWidth = Math.Max(MinElementSize, layoutVm.BlockInstructionsWidth + widthDelta);
                    layoutVm.BlockInstructionsHeight = Math.Max(MinElementSize, layoutVm.BlockInstructionsHeight + heightDelta);
                    layoutVm.BlockInstructionsX -= widthDelta / 2;
                    layoutVm.BlockInstructionsY -= heightDelta / 2;
                    break;

                case "ErrorMark":
                    layoutVm.ErrorMarkWidth = Math.Max(MinElementSize, layoutVm.ErrorMarkWidth + widthDelta);
                    layoutVm.ErrorMarkHeight = Math.Max(MinElementSize, layoutVm.ErrorMarkHeight + heightDelta);
                    layoutVm.ErrorMarkX -= widthDelta / 2;
                    layoutVm.ErrorMarkY -= heightDelta / 2;
                    break;

                case "LeftKey":
                    layoutVm.KeyWidth = Math.Max(MinElementSize, layoutVm.KeyWidth + widthDelta);
                    layoutVm.KeyHeight = Math.Max(MinElementSize, layoutVm.KeyHeight + heightDelta);
                    layoutVm.LeftKeyX -= widthDelta / 2;
                    layoutVm.LeftKeyY -= heightDelta / 2;
                    layoutVm.RightKeyX -= widthDelta / 2;
                    break;

                case "RightKey":
                    layoutVm.KeyWidth = Math.Max(MinElementSize, layoutVm.KeyWidth + widthDelta);
                    layoutVm.KeyHeight = Math.Max(MinElementSize, layoutVm.KeyHeight + heightDelta);
                    layoutVm.RightKeyX -= widthDelta / 2;
                    layoutVm.RightKeyY -= heightDelta / 2;
                    layoutVm.LeftKeyX -= widthDelta / 2;
                    break;
            }
        }

        /// <summary>
        /// Required by the ResizeThumbStyle EventSetter. Does not mark Handled so the Thumb
        /// can still perform its own drag; the move handler ignores thumb-originated clicks.
        /// </summary>
        private void OnThumbPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Intentionally empty — presence of the handler + ZIndex keeps thumbs hittable.
            // Move logic is gated by IsDescendantOfThumb below.
        }

        private void OnElementMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element || element.Tag is not string tag)
                return;

            // Ignore clicks that originated on a resize thumb (or any visual child of one).
            if (IsDescendantOfThumb(e.OriginalSource as DependencyObject))
                return;

            var layoutVm = ResolveLayoutViewModel(element);
            if (layoutVm is null || !layoutVm.IsLayoutEditMode)
                return;

            _isDraggingElement = true;
            _dragElementTag = tag;

            var canvas = FindAncestorCanvas(element);
            if (canvas is null)
                return;

            double scale = layoutVm.ScaleFactor > 0 ? layoutVm.ScaleFactor : 1.0;
            var pos = e.GetPosition(canvas);
            _dragStartInCanvas = new Point(pos.X * scale, pos.Y * scale);
            (_dragOriginX, _dragOriginY) = GetElementPosition(layoutVm, tag);

            element.CaptureMouse();
            e.Handled = true;
        }

        private static bool IsDescendantOfThumb(DependencyObject? node)
        {
            while (node != null)
            {
                if (node is Thumb)
                    return true;
                node = VisualTreeHelper.GetParent(node);
            }
            return false;
        }

        private void OnElementMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingElement || _dragElementTag is null)
                return;
            if (sender is not FrameworkElement element)
                return;

            var layoutVm = ResolveLayoutViewModel(element);
            if (layoutVm is null)
                return;

            var canvas = FindAncestorCanvas(element);
            if (canvas is null)
                return;

            double scale = layoutVm.ScaleFactor > 0 ? layoutVm.ScaleFactor : 1.0;
            var current = e.GetPosition(canvas);
            double dx = (current.X * scale) - _dragStartInCanvas.X;
            double dy = (current.Y * scale) - _dragStartInCanvas.Y;

            SetElementPosition(layoutVm, _dragElementTag, _dragOriginX + dx, _dragOriginY + dy);
        }

        private void OnElementMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingElement)
                return;

            if (sender is FrameworkElement element)
                element.ReleaseMouseCapture();

            _isDraggingElement = false;
            _dragElementTag = null;
            e.Handled = true;
        }

        private static LayoutViewModel? ResolveLayoutViewModel(FrameworkElement element)
        {
            if (element.DataContext is LayoutViewModel direct)
                return direct;

            var parent = element.Parent as FrameworkElement;
            while (parent != null)
            {
                if (parent.DataContext is LayoutViewModel vm)
                    return vm;
                parent = parent.Parent as FrameworkElement;
            }
            return null;
        }

        private static Canvas? FindAncestorCanvas(DependencyObject child)
        {
            var current = child;
            while (current != null)
            {
                if (current is Canvas c)
                    return c;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private static (double x, double y) GetElementPosition(LayoutViewModel vm, string tag) => tag switch
        {
            "Stimulus" => (vm.StimulusX, vm.StimulusY),
            "BlockInstructions" => (vm.BlockInstructionsX, vm.BlockInstructionsY),
            "ErrorMark" => (vm.ErrorMarkX, vm.ErrorMarkY),
            "LeftKey" => (vm.LeftKeyX, vm.LeftKeyY),
            "RightKey" => (vm.RightKeyX, vm.RightKeyY),
            _ => (0, 0)
        };

        private static void SetElementPosition(LayoutViewModel vm, string tag, double x, double y)
        {
            x = Math.Max(-50, x);
            y = Math.Max(-50, y);

            switch (tag)
            {
                case "Stimulus":
                    vm.StimulusX = x;
                    vm.StimulusY = y;
                    break;
                case "BlockInstructions":
                    vm.BlockInstructionsX = x;
                    vm.BlockInstructionsY = y;
                    break;
                case "ErrorMark":
                    vm.ErrorMarkX = x;
                    vm.ErrorMarkY = y;
                    break;
                case "LeftKey":
                    vm.LeftKeyX = x;
                    vm.LeftKeyY = y;
                    break;
                case "RightKey":
                    vm.RightKeyX = x;
                    vm.RightKeyY = y;
                    break;
            }
        }
    }
}
