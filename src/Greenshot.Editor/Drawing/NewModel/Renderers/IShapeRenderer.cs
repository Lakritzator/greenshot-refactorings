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
    /// Responsible for rendering shapes to a graphics context.
    /// Separates drawing logic from data model.
    /// </summary>
    public interface IShapeRenderer
    {
        /// <summary>
        /// Renders a shape to the graphics context
        /// </summary>
        /// <param name="graphics">Graphics context to render to</param>
        /// <param name="shape">Shape to render</param>
        void Render(Graphics graphics, IShape shape);

        /// <summary>
        /// Checks if this renderer can render the given shape type
        /// </summary>
        bool CanRender(IShape shape);
    }
}
