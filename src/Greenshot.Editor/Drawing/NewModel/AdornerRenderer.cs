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

namespace Greenshot.Editor.Drawing.NewModel
{
    /// <summary>
    /// Represents an adorner position (resize handle) on a shape
    /// </summary>
    public enum AdornerPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    /// <summary>
    /// Renders adorners (resize handles, selection borders) for shapes in the editor.
    /// Completely separate from the shape data and rendering logic.
    /// </summary>
    public class AdornerRenderer
    {
        private const int AdornerSize = 7;
        private static readonly Color SelectionBorderColor = Color.Blue;
        private static readonly Color AdornerColor = Color.White;
        private static readonly Color AdornerBorderColor = Color.Black;

        /// <summary>
        /// Renders selection border and adorners for a shape
        /// </summary>
        public void RenderAdorners(Graphics graphics, ShapeEditorState state)
        {
            if (!state.IsSelected)
            {
                return;
            }

            var bounds = state.ResizingBounds ?? state.Shape.Bounds;

            // Draw selection border
            using (var pen = new Pen(SelectionBorderColor, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
            {
                graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }

            if (!state.ShowAdorners)
            {
                return;
            }

            // Draw adorners (resize handles)
            DrawAdorner(graphics, GetAdornerRect(bounds, AdornerPosition.TopLeft));
            DrawAdorner(graphics, GetAdornerRect(bounds, AdornerPosition.TopCenter));
            DrawAdorner(graphics, GetAdornerRect(bounds, AdornerPosition.TopRight));
            DrawAdorner(graphics, GetAdornerRect(bounds, AdornerPosition.MiddleLeft));
            DrawAdorner(graphics, GetAdornerRect(bounds, AdornerPosition.MiddleRight));
            DrawAdorner(graphics, GetAdornerRect(bounds, AdornerPosition.BottomLeft));
            DrawAdorner(graphics, GetAdornerRect(bounds, AdornerPosition.BottomCenter));
            DrawAdorner(graphics, GetAdornerRect(bounds, AdornerPosition.BottomRight));
        }

        private void DrawAdorner(Graphics graphics, Rectangle adornerRect)
        {
            using (var fillBrush = new SolidBrush(AdornerColor))
            {
                graphics.FillRectangle(fillBrush, adornerRect);
            }

            using (var borderPen = new Pen(AdornerBorderColor, 1))
            {
                graphics.DrawRectangle(borderPen, adornerRect);
            }
        }

        /// <summary>
        /// Gets the rectangle for an adorner at the specified position
        /// </summary>
        public Rectangle GetAdornerRect(NativeRect bounds, AdornerPosition position)
        {
            int x = 0, y = 0;

            switch (position)
            {
                case AdornerPosition.TopLeft:
                    x = bounds.Left - AdornerSize / 2;
                    y = bounds.Top - AdornerSize / 2;
                    break;
                case AdornerPosition.TopCenter:
                    x = bounds.Left + bounds.Width / 2 - AdornerSize / 2;
                    y = bounds.Top - AdornerSize / 2;
                    break;
                case AdornerPosition.TopRight:
                    x = bounds.Right - AdornerSize / 2;
                    y = bounds.Top - AdornerSize / 2;
                    break;
                case AdornerPosition.MiddleLeft:
                    x = bounds.Left - AdornerSize / 2;
                    y = bounds.Top + bounds.Height / 2 - AdornerSize / 2;
                    break;
                case AdornerPosition.MiddleRight:
                    x = bounds.Right - AdornerSize / 2;
                    y = bounds.Top + bounds.Height / 2 - AdornerSize / 2;
                    break;
                case AdornerPosition.BottomLeft:
                    x = bounds.Left - AdornerSize / 2;
                    y = bounds.Bottom - AdornerSize / 2;
                    break;
                case AdornerPosition.BottomCenter:
                    x = bounds.Left + bounds.Width / 2 - AdornerSize / 2;
                    y = bounds.Bottom - AdornerSize / 2;
                    break;
                case AdornerPosition.BottomRight:
                    x = bounds.Right - AdornerSize / 2;
                    y = bounds.Bottom - AdornerSize / 2;
                    break;
            }

            return new Rectangle(x, y, AdornerSize, AdornerSize);
        }

        /// <summary>
        /// Checks if a point hits any adorner
        /// </summary>
        public AdornerPosition? HitTestAdorner(NativeRect bounds, Point point)
        {
            var positions = new[]
            {
                AdornerPosition.TopLeft,
                AdornerPosition.TopCenter,
                AdornerPosition.TopRight,
                AdornerPosition.MiddleLeft,
                AdornerPosition.MiddleRight,
                AdornerPosition.BottomLeft,
                AdornerPosition.BottomCenter,
                AdornerPosition.BottomRight
            };

            foreach (var position in positions)
            {
                var adornerRect = GetAdornerRect(bounds, position);
                if (adornerRect.Contains(point))
                {
                    return position;
                }
            }

            return null;
        }
    }
}
