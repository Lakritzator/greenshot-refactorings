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

using System.ComponentModel;
using System.Windows.Forms;

namespace Greenshot.Translations.Forms
{
    /// <summary>
    /// A <see cref="Button"/> that implements <see cref="ITranslatable"/>
    /// so that <see cref="FormsTranslationHelper"/> can set its
    /// <see cref="Control.Text"/> from a translation key without reflection.
    /// </summary>
    /// <example>
    /// Drop onto a form in the designer and set
    /// <see cref="TranslationKey"/> to e.g. <c>"cancel"</c>.
    /// </example>
    public class TranslatableButton : Button, ITranslatable
    {
        /// <summary>
        /// Gets or sets the translation key used to populate this button's
        /// <see cref="Control.Text"/> property.
        /// </summary>
        [Category("Translation")]
        [DefaultValue(null)]
        [Description("The translation key that provides the button's display text.")]
        public string TranslationKey { get; set; }
    }
}
