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
using Greenshot.Editor.Drawing.NewModel.Models;

namespace Greenshot.Editor.Drawing.NewModel.Renderers
{
    /// <summary>
    /// Renderer for CursorShape with hotspot visualization
    /// </summary>
    public class CursorRenderer : IShapeRenderer
    {
        public bool CanRender(IShape shape)
        {
            return shape is CursorShape;
        }

        public void Render(Graphics graphics, IShape shape)
        {
            if (!(shape is CursorShape cursorShape))
            {
                return;
            }

            var imageData = cursorShape.ImageData;
            if (imageData == null)
            {
                return;
            }

            // Render the cursor image
            if (imageData is BitmapImageData bitmapData)
            {
                var bounds = shape.Bounds;
                var destRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                graphics.DrawImage(bitmapData.Bitmap, destRect);

                // Optionally visualize the hotspot (for debugging/editing)
                // var hotspotX = bounds.X + cursorShape.Hotspot.X;
                // var hotspotY = bounds.Y + cursorShape.Hotspot.Y;
                // using (var pen = new Pen(Color.Red, 2))
                // {
                //     graphics.DrawEllipse(pen, hotspotX - 3, hotspotY - 3, 6, 6);
                // }
            }
        }
    }
}
