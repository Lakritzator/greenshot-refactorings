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

namespace Greenshot.Translations.Core
{
    /// <summary>
    /// Provides translated strings for a single language.
    /// Implementations must be thread-safe for reads after initialisation.
    /// </summary>
    public interface ITranslationProvider
    {
        /// <summary>
        /// Raised after the active language has been changed and the new
        /// translations are loaded.  Subscribers should refresh any cached strings.
        /// </summary>
        event EventHandler LanguageChanged;

        /// <summary>
        /// Returns the translated string for <paramref name="key"/>.
        /// Returns a sentinel value of the form <c>###key###</c> when the key is
        /// not found, so missing translations are visible without throwing.
        /// </summary>
        /// <param name="key">
        /// The translation key, e.g. <c>"cancel"</c> or <c>"box.upload_menu_item"</c>.
        /// </param>
        string GetString(string key);

        /// <summary>
        /// Returns <c>true</c> and populates <paramref name="value"/> when
        /// <paramref name="key"/> has a translation in the active language.
        /// </summary>
        bool TryGetString(string key, out string value);

        /// <summary>
        /// Returns <c>true</c> when <paramref name="key"/> exists in the
        /// currently loaded translations.
        /// </summary>
        bool HasKey(string key);
    }
}
