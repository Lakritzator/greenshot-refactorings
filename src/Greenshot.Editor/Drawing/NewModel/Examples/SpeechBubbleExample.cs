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

using System.Drawing;
using Dapplo.Windows.Common.Structs;
using Greenshot.Editor.Drawing.NewModel.Models;
using Greenshot.Editor.Drawing.NewModel.Renderers;

namespace Greenshot.Editor.Drawing.NewModel.Examples
{
    /// <summary>
    /// Example demonstrating the use of SpeechBubbleShape with IAdornerConfiguration
    /// </summary>
    public static class SpeechBubbleExample
    {
        /// <summary>
        /// Example showing how to create and render a speech bubble with custom adorners
        /// </summary>
        public static void Example_SpeechBubbleWithCustomAdorners(Graphics graphics)
        {
            var canvas = new ShapeCanvas();
            var renderer = new CanvasRenderer();

            // Register the speech bubble renderer
            renderer.RegisterRenderer(new SpeechBubbleRenderer());

            // Create a speech bubble
            // The bubble body is a rounded rectangle at (100, 50) with size 150x80
            // The tail points to position (200, 200) - where someone is speaking from
            var bubbleBounds = new NativeRect(100, 50, 150, 80);
            var tailPosition = new Point(200, 200);
            var bubbleStyle = new ShapeStyle(Color.Black, 2, Color.Yellow, true);

            var speechBubble = new SpeechBubbleShape(bubbleBounds, tailPosition, bubbleStyle);
            canvas.AddShape(speechBubble);

            // Create editor state to show the speech bubble as selected
            var state = new ShapeEditorState(speechBubble)
            {
                IsSelected = true,
                ShowAdorners = true
            };

            // Render the canvas with the speech bubble
            // The shape will be rendered with its custom adorners:
            // - 8 white adorners around the bubble rectangle for resizing
            // - 1 orange adorner at the tail tip for moving the tail
            renderer.RenderCanvas(graphics, canvas, new[] { state });

            // The IAdornerConfiguration interface allows the SpeechBubbleShape to define:
            // 1. Position of each adorner (8 standard + 1 tail)
            // 2. Color of each adorner (white for standard, orange for tail to distinguish it)
            // 3. Size of each adorner
            //
            // When the user interacts with the adorners:
            // - Dragging the 8 white adorners resizes the bubble rectangle
            // - Dragging the orange adorner moves the tail tip position
            //
            // The adorner configuration is queried each time rendering happens,
            // so if the tail position or bubble bounds change, the adorners
            // automatically update to the new positions.
        }

        /// <summary>
        /// Example showing how to programmatically move the tail of a speech bubble
        /// </summary>
        public static void Example_MovingSpeechBubbleTail(Graphics graphics)
        {
            var canvas = new ShapeCanvas();
            var renderer = new CanvasRenderer();
            renderer.RegisterRenderer(new SpeechBubbleRenderer());

            // Create speech bubble
            var bubble = new SpeechBubbleShape(
                new NativeRect(50, 50, 200, 100),
                new Point(150, 250),
                new ShapeStyle(Color.DarkBlue, 2, Color.LightBlue, false)
            );
            canvas.AddShape(bubble);

            // Simulate moving the tail (e.g., user dragging the tail adorner)
            bubble.TailPosition = new Point(180, 280);

            // When re-rendered, the adorners will automatically be at the new positions
            // because GetAdorners() is called with the current shape state
            var state = new ShapeEditorState(bubble) { IsSelected = true, ShowAdorners = true };
            renderer.RenderCanvas(graphics, canvas, new[] { state });
        }

        /// <summary>
        /// Example showing multiple speech bubbles with different tail positions
        /// </summary>
        public static void Example_MultipleSpeechBubbles(Graphics graphics)
        {
            var canvas = new ShapeCanvas();
            var renderer = new CanvasRenderer();
            renderer.RegisterRenderer(new SpeechBubbleRenderer());

            // Create a conversation with multiple speech bubbles
            var bubble1 = new SpeechBubbleShape(
                new NativeRect(50, 50, 180, 60),
                new Point(100, 150),
                new ShapeStyle(Color.Black, 2, Color.LightYellow, false)
            );

            var bubble2 = new SpeechBubbleShape(
                new NativeRect(250, 100, 200, 80),
                new Point(350, 220),
                new ShapeStyle(Color.Black, 2, Color.LightGreen, false)
            );

            var bubble3 = new SpeechBubbleShape(
                new NativeRect(100, 200, 150, 70),
                new Point(175, 300),
                new ShapeStyle(Color.Black, 2, Color.LightBlue, false)
            );

            canvas.AddShape(bubble1);
            canvas.AddShape(bubble2);
            canvas.AddShape(bubble3);

            // Select bubble2 to show its custom adorners
            var state = new ShapeEditorState(bubble2) { IsSelected = true, ShowAdorners = true };

            renderer.RenderCanvas(graphics, canvas, new[] { state });
        }
    }
}
