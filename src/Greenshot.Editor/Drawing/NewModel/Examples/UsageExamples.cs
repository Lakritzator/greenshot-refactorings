/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2004-2026 Thomas Braun, Jens Klingen, Robin Krom
 *
 * For more information see: https://getgreenshot.org/
 * The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Greenshot.Editor.Drawing.NewModel.Models;
using Greenshot.Editor.Drawing.NewModel.Renderers;
using System.Drawing;
using Dapplo.Windows.Common.Structs;

namespace Greenshot.Editor.Drawing.NewModel.Examples
{
    /// <summary>
    /// Demonstrates usage of the new drawing surface architecture.
    /// This class shows how to create shapes, apply styles, manage canvas, and render.
    /// </summary>
    public static class UsageExamples
    {
        /// <summary>
        /// Example 1: Basic shape creation and rendering
        /// </summary>
        public static void Example1_BasicShapes(Graphics graphics)
        {
            // Create a canvas to hold shapes
            var canvas = new ShapeCanvas();

            // Create a rectangle with a red border
            var redBorderStyle = new ShapeStyle(Color.Red, 2, Color.Empty, false);
            var rectangle = new RectangleShape(new NativeRect(10, 10, 100, 50), redBorderStyle);
            canvas.AddShape(rectangle);

            // Create an ellipse with blue fill
            var blueFillStyle = new ShapeStyle(Color.Blue, 1, Color.LightBlue, false);
            var ellipse = new EllipseShape(new NativeRect(50, 50, 80, 60), blueFillStyle);
            canvas.AddShape(ellipse);

            // Create a text shape
            var textStyle = new ShapeStyle(Color.Black, 1, Color.Yellow, false);
            var text = new TextShape(
                new NativeRect(120, 10, 150, 40),
                textStyle,
                "Hello, World!",
                new Font("Arial", 12)
            );
            canvas.AddShape(text);

            // Render the canvas
            var renderer = new CanvasRenderer();
            renderer.RenderCanvas(graphics, canvas);
        }

        /// <summary>
        /// Example 2: Using the StyleManager for reusable styles
        /// </summary>
        public static void Example2_StyleManager(Graphics graphics)
        {
            var canvas = new ShapeCanvas();
            var styleManager = new StyleManager();

            // Create and register a custom style
            var highlightStyle = new ShapeStyle(Color.Orange, 3, Color.FromArgb(100, Color.Orange), true);
            styleManager.RegisterStyle("Highlight", highlightStyle);

            // Create shapes using predefined styles
            var rect1 = new RectangleShape(new NativeRect(10, 10, 80, 40), null);
            var rect2 = new RectangleShape(new NativeRect(100, 10, 80, 40), null);
            var rect3 = new RectangleShape(new NativeRect(190, 10, 80, 40), null);

            canvas.AddShape(rect1);
            canvas.AddShape(rect2);
            canvas.AddShape(rect3);

            // Apply styles using StyleManager
            styleManager.ApplyStyle(rect1, "RedBorder");
            styleManager.ApplyStyle(rect2, "BlueFilled");
            styleManager.ApplyStyle(rect3, "Highlight");

            // Render
            var renderer = new CanvasRenderer();
            renderer.RenderCanvas(graphics, canvas);
        }

        /// <summary>
        /// Example 3: Applying the same style to multiple shapes
        /// </summary>
        public static void Example3_ApplyStyleToMultipleShapes(Graphics graphics)
        {
            var canvas = new ShapeCanvas();
            var styleManager = new StyleManager();

            // Create multiple shapes
            var shapes = new List<IShape>
            {
                new RectangleShape(new NativeRect(10, 10, 50, 50), null),
                new EllipseShape(new NativeRect(70, 10, 50, 50), null),
                new RectangleShape(new NativeRect(130, 10, 50, 50), null)
            };

            foreach (var shape in shapes)
            {
                canvas.AddShape(shape);
            }

            // Apply same style to all shapes at once
            styleManager.ApplyStyleToShapes(shapes, "GreenHighlight");

            // Render
            var renderer = new CanvasRenderer();
            renderer.RenderCanvas(graphics, canvas);
        }

        /// <summary>
        /// Example 4: Working with editor state (selection and adorners)
        /// </summary>
        public static void Example4_EditorState(Graphics graphics)
        {
            var canvas = new ShapeCanvas();

            // Create shapes
            var rect = new RectangleShape(new NativeRect(10, 10, 100, 60), ShapeStyle.Default());
            var ellipse = new EllipseShape(new NativeRect(120, 10, 100, 60), ShapeStyle.Default());

            canvas.AddShape(rect);
            canvas.AddShape(ellipse);

            // Create editor states
            var states = new List<ShapeEditorState>();

            // Rectangle is selected with adorners visible
            var rectState = new ShapeEditorState(rect);
            rectState.IsSelected = true;
            rectState.ShowAdorners = true;
            states.Add(rectState);

            // Ellipse is selected but adorners not shown
            var ellipseState = new ShapeEditorState(ellipse);
            ellipseState.IsSelected = true;
            ellipseState.ShowAdorners = false;
            states.Add(ellipseState);

            // Render with editor state (shows selection borders and adorners)
            var renderer = new CanvasRenderer();
            renderer.RenderCanvas(graphics, canvas, states);
        }

        /// <summary>
        /// Example 5: Z-order management (bring to front, send to back)
        /// </summary>
        public static void Example5_ZOrder(Graphics graphics)
        {
            var canvas = new ShapeCanvas();

            // Create overlapping shapes
            var redRect = new RectangleShape(
                new NativeRect(10, 10, 100, 100),
                new ShapeStyle(Color.Red, 2, Color.FromArgb(150, Color.Red), false)
            );

            var blueRect = new RectangleShape(
                new NativeRect(50, 50, 100, 100),
                new ShapeStyle(Color.Blue, 2, Color.FromArgb(150, Color.Blue), false)
            );

            var greenRect = new RectangleShape(
                new NativeRect(90, 90, 100, 100),
                new ShapeStyle(Color.Green, 2, Color.FromArgb(150, Color.Green), false)
            );

            canvas.AddShape(redRect);
            canvas.AddShape(blueRect);
            canvas.AddShape(greenRect);

            // Bring red rectangle to front (it will be drawn last, appearing on top)
            canvas.BringToFront(redRect);

            // Render
            var renderer = new CanvasRenderer();
            renderer.RenderCanvas(graphics, canvas);
        }

        /// <summary>
        /// Example 6: Modifying styles immutably
        /// </summary>
        public static void Example6_ImmutableStyles(Graphics graphics)
        {
            var canvas = new ShapeCanvas();

            // Create a base style
            var baseStyle = new ShapeStyle(Color.Black, 1, Color.White, false);

            // Create variations using With* methods (creates new style instances)
            var redVersion = baseStyle.WithLineColor(Color.Red);
            var thickVersion = baseStyle.WithLineThickness(3);
            var shadowVersion = baseStyle.WithShadow(true);

            // Create shapes with different style variations
            canvas.AddShape(new RectangleShape(new NativeRect(10, 10, 80, 60), baseStyle));
            canvas.AddShape(new RectangleShape(new NativeRect(100, 10, 80, 60), redVersion));
            canvas.AddShape(new RectangleShape(new NativeRect(190, 10, 80, 60), thickVersion));
            canvas.AddShape(new RectangleShape(new NativeRect(280, 10, 80, 60), shadowVersion));

            // Render
            var renderer = new CanvasRenderer();
            renderer.RenderCanvas(graphics, canvas);
        }

        /// <summary>
        /// Example 7: Shape cloning (for copy/paste operations)
        /// </summary>
        public static void Example7_Cloning(Graphics graphics)
        {
            var canvas = new ShapeCanvas();

            // Create an original shape
            var original = new RectangleShape(
                new NativeRect(10, 10, 80, 60),
                new ShapeStyle(Color.Blue, 2, Color.LightBlue, false)
            );
            canvas.AddShape(original);

            // Clone the shape (creates a new shape with new ID)
            var clone = (RectangleShape)original.Clone();

            // Modify the clone's position
            clone.Bounds = new NativeRect(100, 10, 80, 60);
            canvas.AddShape(clone);

            // Both shapes share the same style instance (memory efficient)
            // But they can have different styles assigned independently
            clone.Style = clone.Style.WithLineColor(Color.Red);

            // Render
            var renderer = new CanvasRenderer();
            renderer.RenderCanvas(graphics, canvas);
        }

        /// <summary>
        /// Example 8: Complete workflow - create, select, modify style
        /// </summary>
        public static void Example8_CompleteWorkflow(Graphics graphics)
        {
            var canvas = new ShapeCanvas();
            var styleManager = new StyleManager();
            var renderer = new CanvasRenderer();

            // Step 1: Create shapes
            var rect = new RectangleShape(new NativeRect(10, 10, 100, 60), ShapeStyle.Default());
            var ellipse = new EllipseShape(new NativeRect(120, 10, 80, 60), ShapeStyle.Default());
            canvas.AddShape(rect);
            canvas.AddShape(ellipse);

            // Step 2: Select rectangle (in UI)
            var states = new List<ShapeEditorState>();
            var rectState = new ShapeEditorState(rect);
            rectState.IsSelected = true;
            rectState.ShowAdorners = true;
            states.Add(rectState);

            // Step 3: Apply style to selected shape
            styleManager.ApplyStyle(rect, "RedBorder");

            // Step 4: Render with selection
            renderer.RenderCanvas(graphics, canvas, states);

            // Step 5: Deselect and select ellipse
            rectState.IsSelected = false;
            rectState.ShowAdorners = false;

            var ellipseState = new ShapeEditorState(ellipse);
            ellipseState.IsSelected = true;
            ellipseState.ShowAdorners = true;
            states.Clear();
            states.Add(ellipseState);

            // Step 6: Apply different style
            styleManager.ApplyStyle(ellipse, "BlueFilled");

            // Step 7: Render again
            renderer.RenderCanvas(graphics, canvas, states);
        }
    }
}
