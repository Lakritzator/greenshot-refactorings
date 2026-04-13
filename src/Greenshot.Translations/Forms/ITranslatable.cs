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

using System.Windows.Forms;

namespace Greenshot.Translations.Forms
{
    /// <summary>
    /// Marks a WinForms control as having an associated translation key.
    /// Used by <see cref="FormsTranslationHelper"/> to apply the correct
    /// translated text without reflection.
    /// </summary>
    /// <example>
    /// <code>
    /// var btn = new TranslatableButton { TranslationKey = "cancel" };
    /// FormsTranslationHelper.ApplyTranslations(this, provider);
    /// </code>
    /// </example>
    public interface ITranslatable
    {
        /// <summary>
        /// The translation key whose value will be used as the control's
        /// <c>Text</c> property.
        /// </summary>
        string TranslationKey { get; }
    }
}
