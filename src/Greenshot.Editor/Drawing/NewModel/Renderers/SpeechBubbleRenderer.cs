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
using System.Drawing.Drawing2D;
using Greenshot.Editor.Drawing.NewModel.Models;

namespace Greenshot.Editor.Drawing.NewModel.Renderers
{
    /// <summary>
    /// Renderer for SpeechBubbleShape - draws a rounded rectangle with a tail pointing to a position
    /// </summary>
    public class SpeechBubbleRenderer : IShapeRenderer
    {
        public bool CanRender(IShape shape)
        {
            return shape is SpeechBubbleShape;
        }

        public void Render(Graphics graphics, IShape shape)
        {
            if (!(shape is SpeechBubbleShape bubble))
            {
                return;
            }

            var bounds = bubble.Bounds;
            var style = bubble.Style;

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            // Create the speech bubble path (rounded rectangle + tail)
            using (var path = CreateSpeechBubblePath(bubble))
            {
                // Draw shadow if needed
                if (style.Shadow)
                {
                    using (var shadowPath = CreateSpeechBubblePath(bubble))
                    {
                        var shadowMatrix = new Matrix();
                        shadowMatrix.Translate(2, 2);
                        shadowPath.Transform(shadowMatrix);

                        using (var shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                        {
                            graphics.FillPath(shadowBrush, shadowPath);
                        }
                    }
                }

                // Fill the bubble if fill color is specified
                if (style.FillColor != Color.Empty)
                {
                    using (var fillBrush = new SolidBrush(style.FillColor))
                    {
                        graphics.FillPath(fillBrush, path);
                    }
                }

                // Draw outline
                if (style.LineThickness > 0 && style.LineColor != Color.Empty)
                {
                    using (var pen = new Pen(style.LineColor, style.LineThickness))
                    {
                        graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a GraphicsPath for the speech bubble (rounded rectangle with tail)
        /// </summary>
        private GraphicsPath CreateSpeechBubblePath(SpeechBubbleShape bubble)
        {
            var path = new GraphicsPath();
            var bounds = bubble.Bounds;
            var tailTip = bubble.TailPosition;

            // Corner radius for rounded rectangle
            int cornerRadius = 10;

            // Create rounded rectangle for the bubble body
            var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            AddRoundedRectangle(path, rect, cornerRadius);

            // Determine which edge of the rectangle is closest to the tail tip
            // and add a tail triangle from that edge
            AddTail(path, bounds, tailTip);

            return path;
        }

        /// <summary>
        /// Adds a rounded rectangle to the path
        /// </summary>
        private void AddRoundedRectangle(GraphicsPath path, Rectangle rect, int radius)
        {
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
        }

        /// <summary>
        /// Adds a tail triangle pointing to the tail tip position
        /// </summary>
        private void AddTail(GraphicsPath path, Dapplo.Windows.Common.Structs.NativeRect bounds, Point tailTip)
        {
            // Simplified tail logic: create a triangle from the bottom edge
            // In a real implementation, you'd calculate which edge is closest
            
            int tailWidth = 20;
            int centerX = bounds.Left + bounds.Width / 2;
            int bottomY = bounds.Bottom;

            // Only add tail if the tip is below the bubble and not too far away
            if (tailTip.Y > bottomY && tailTip.Y < bottomY + 100)
            {
                Point[] tailPoints = new Point[]
                {
                    new Point(centerX - tailWidth / 2, bottomY),
                    new Point(tailTip.X, tailTip.Y),
                    new Point(centerX + tailWidth / 2, bottomY)
                };

                path.AddPolygon(tailPoints);
            }
        }
    }
}
