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

using System;

namespace Greenshot.Editor.Drawing.NewModel.Models
{
    /// <summary>
    /// Represents a layer in the canvas for organizing shapes
    /// </summary>
    public class Layer
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public bool IsVisible { get; set; }
        public bool IsLocked { get; set; }
        public int ZIndex { get; set; }

        public Layer(string name, int zIndex = 0)
        {
            Id = Guid.NewGuid();
            Name = name ?? "Layer";
            IsVisible = true;
            IsLocked = false;
            ZIndex = zIndex;
        }

        /// <summary>
        /// Creates a background layer (implicit layer for background image)
        /// </summary>
        public static Layer CreateBackgroundLayer()
        {
            return new Layer("Background", int.MinValue) { IsLocked = true };
        }

        /// <summary>
        /// Creates a default layer for shapes
        /// </summary>
        public static Layer CreateDefaultLayer()
        {
            return new Layer("Default", 0);
        }
    }
}
