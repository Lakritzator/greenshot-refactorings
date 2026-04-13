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

namespace Greenshot.Translations.Core
{
    /// <summary>
    /// Discovers available translation files by inspecting filenames in
    /// registered directories, without reading file contents.
    ///
    /// <para>
    /// <b>Plugin registration:</b> each plugin calls <see cref="AddPath"/>
    /// from its initialise method, passing the directory that contains its
    /// INI translation files.  No other registration step is required.
    /// The host application creates the <see cref="ITranslationManager"/>
    /// and passes it to each plugin during initialisation so all plugins
    /// share a single provider instance.
    /// </para>
    /// </summary>
    public interface ITranslationDiscovery
    {
        /// <summary>
        /// Returns metadata for every translation file found in all
        /// registered search paths.  Only filenames are examined — file
        /// contents are not parsed during discovery.
        /// </summary>
        IReadOnlyList<TranslationFile> GetAvailableLanguages();

        /// <summary>
        /// Registers a directory to be scanned for translation files.
        /// Call this once per component (host app or plugin) from the
        /// component's initialise method.  Duplicate paths are silently ignored.
        /// </summary>
        /// <param name="directoryPath">
        /// Absolute path to the directory containing <c>*.ini</c> translation files.
        /// </param>
        void AddPath(string directoryPath);
    }
}
