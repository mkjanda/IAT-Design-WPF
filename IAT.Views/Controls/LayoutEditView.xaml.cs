using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using IAT.ViewModels;

namespace IAT.Views.Controls
{
    /// <summary>
    /// Interaction logic for LayoutEditView.xaml
    /// </summary>
    public partial class LayoutEditView : UserControl
    {
        private const double MinElementSize = 24.0;

        private bool _isDraggingElement;
        private string? _dragElementTag;
        private Point _dragStartInCanvas;
        private double _dragOriginX;
        private double _dragOriginY;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutEditView"/> class.
        /// </summary>
        public LayoutEditView()
        {
            InitializeComponent();
            Loaded += (_, _) => TryFitToHost();
            DataContextChanged += (_, _) => TryFitToHost();
        }

        /// <summary>
        /// Handles the SizeChanged event of the preview host. If the new size is valid, it executes the FitToWindowCommand 
        /// on the LayoutViewModel to adjust the layout to fit within the new size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreviewHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is LayoutViewModel vm && e.NewSize.Width > 0 && e.NewSize.Height > 0)
                vm.FitToWindowCommand.Execute(e.NewSize);
        }

        /// <summary>
        /// Attempts to fit the layout to the host window by executing the FitToWindowCommand on the LayoutViewModel.
        /// </summary>
        private void TryFitToHost()
        {
            if (DataContext is not LayoutViewModel vm)
                return;
            if (ActualWidth > 1 && ActualHeight > 1)
                vm.FitToWindowCommand.Execute(new Size(Math.Max(100, ActualWidth - 32), Math.Max(100, ActualHeight - 32)));
        }

        /// <summary>
        /// Handles the DragDelta event of the resize thumbs. It calculates the new size and position of the associated layout 
        /// element based on the drag delta and updates the LayoutViewModel accordingly.
        /// </summary>
        /// <param name="sender">The thumb that is being dragged.</param>
        /// <param name="e">The event data for the drag delta.</param>
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

        /// <summary>
        /// Handles the MouseLeftButtonDown event for layout elements. It initiates a drag operation if the 
        /// layout is in edit mode, capturing the mouse and storing the initial position and tag of the element being dragged.
        /// </summary>
        /// <param name="sender">The layout element that is being clicked.</param>
        /// <param name="e">The event data for the mouse button down event.</param>
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

        /// <summary>
        /// Determines whether the specified node is a descendant of a Thumb control in the visual tree. This is 
        /// used to prevent drag operations from starting when clicking on resize thumbs.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <returns>True if the node is a descendant of a Thumb; otherwise, false.</returns>
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

        /// <summary>
        /// Handles the MouseMove event for layout elements. If a drag operation is in progress, it calculates 
        /// the new position of the dragged element based on the current mouse position and updates the 
        /// LayoutViewModel accordingly.
        /// </summary>
        /// <param name="sender">The layout element that is being moved.</param>
        /// <param name="e">The event data for the mouse move event.</param>
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

        /// <summary>
        /// Handles the MouseLeftButtonUp event for layout elements. It ends the drag operation, releases mouse 
        /// capture, and resets the dragging state.
        /// </summary>
        /// <param name="sender">The layout element that was being dragged.</param>
        /// <param name="e">The event data for the mouse button event.</param>
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

        /// <summary>
        /// Resolves the LayoutViewModel associated with a given FrameworkElement by checking its DataContext and 
        /// traversing up the visual tree if necessary. This is used to find the relevant ViewModel for layout 
        /// editing operations.
        /// </summary>
        /// <param name="element">The FrameworkElement for which to resolve the LayoutViewModel.</param>
        /// <returns>The resolved LayoutViewModel, or null if none is found.</returns>
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

        /// <summary>
        /// Finds the nearest ancestor Canvas of a given DependencyObject in the visual tree. This is used to 
        /// determine the coordinate space for drag operations.
        /// </summary>
        /// <param name="child">The starting DependencyObject from which to search for an ancestor Canvas.</param>
        /// <returns>The nearest ancestor Canvas, or null if none is found.</returns>
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

        /// <summary>
        /// Gets the current position of a layout element based on its tag from the LayoutViewModel. This is used to 
        /// determine the coordinates for layout editing operations.
        /// </summary>
        /// <param name="vm">The LayoutViewModel containing the element's position data.</param>
        /// <param name="tag">The tag identifying the layout element.</param>
        /// <returns>A tuple containing the X and Y coordinates of the element.</returns>
        private static (double x, double y) GetElementPosition(LayoutViewModel vm, string tag) => tag switch
        {
            "Stimulus" => (vm.StimulusX, vm.StimulusY),
            "BlockInstructions" => (vm.BlockInstructionsX, vm.BlockInstructionsY),
            "ErrorMark" => (vm.ErrorMarkX, vm.ErrorMarkY),
            "LeftKey" => (vm.LeftKeyX, vm.LeftKeyY),
            "RightKey" => (vm.RightKeyX, vm.RightKeyY),
            _ => (0, 0)
        };

        /// <summary>
        /// Sets the position of a layout element in the LayoutViewModel based on its tag. It ensures that the 
        /// new position does not go below a minimum threshold and updates the corresponding properties in the ViewModel.
        /// </summary>
        /// <param name="vm">The LayoutViewModel containing the element's position data.</param>
        /// <param name="tag">The tag identifying the layout element.</param>
        /// <param name="x">The new X coordinate for the element.</param>
        /// <param name="y">The new Y coordinate for the element.</param>
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
