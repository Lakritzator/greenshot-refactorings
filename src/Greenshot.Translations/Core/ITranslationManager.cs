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

namespace Greenshot.Translations.Core
{
    /// <summary>
    /// Central translation manager that combines language-file discovery,
    /// translation lookup, and runtime language switching.
    /// </summary>
    public interface ITranslationManager : ITranslationProvider, ITranslationDiscovery
    {
        /// <summary>
        /// Gets or sets the active IETF language tag (e.g. <c>"en-US"</c>,
        /// <c>"de-DE"</c>, <c>"zh-TW"</c>).
        /// Setting this property reloads translations and fires
        /// <see cref="ITranslationProvider.LanguageChanged"/>.
        /// </summary>
        string CurrentLanguage { get; set; }

        /// <summary>
        /// The IETF tag used as the ultimate fallback when a key is not found
        /// in the active language.  Defaults to <c>"en-US"</c>.
        /// </summary>
        string FallbackLanguage { get; set; }
    }
}
