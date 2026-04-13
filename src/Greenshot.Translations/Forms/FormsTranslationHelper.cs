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
using System.Windows.Forms;
using Greenshot.Translations.Core;

namespace Greenshot.Translations.Forms
{
    /// <summary>
    /// Recursively applies translations to all <see cref="ITranslatable"/>
    /// controls in a Windows Forms <see cref="Form"/>.
    ///
    /// <para>
    /// Call <see cref="ApplyTranslations"/> once after the form's controls
    /// are initialised.  The helper subscribes to
    /// <see cref="ITranslationProvider.LanguageChanged"/> so the form stays
    /// up to date whenever the user switches language — no further code is
    /// required.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// public MyForm(ITranslationProvider provider)
    /// {
    ///     InitializeComponent();
    ///     FormsTranslationHelper.ApplyTranslations(this, provider);
    /// }
    /// </code>
    /// </example>
    public static class FormsTranslationHelper
    {
        /// <summary>
        /// Applies translations immediately and subscribes to
        /// <see cref="ITranslationProvider.LanguageChanged"/> to reapply
        /// whenever the active language changes.
        /// </summary>
        /// <param name="form">The root form whose control tree is traversed.</param>
        /// <param name="provider">The translation provider to read strings from.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="form"/> or <paramref name="provider"/> is <c>null</c>.
        /// </exception>
        public static void ApplyTranslations(Form form, ITranslationProvider provider)
        {
            if (form is null) throw new ArgumentNullException(nameof(form));
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            ApplyToControl(form, provider);

            provider.LanguageChanged += (_, _) =>
            {
                // Switch back to the UI thread if necessary
                if (form.InvokeRequired)
                {
                    form.BeginInvoke(new Action(() => ApplyToControl(form, provider)));
                }
                else
                {
                    ApplyToControl(form, provider);
                }
            };
        }

        /// <summary>
        /// Recursively visits every control in the subtree rooted at
        /// <paramref name="control"/> and sets its <c>Text</c> when it
        /// implements <see cref="ITranslatable"/>.
        /// </summary>
        private static void ApplyToControl(Control control, ITranslationProvider provider)
        {
            if (control is ITranslatable translatable
                && !string.IsNullOrEmpty(translatable.TranslationKey)
                && provider.TryGetString(translatable.TranslationKey, out var text))
            {
                control.Text = text;
            }

            foreach (Control child in control.Controls)
            {
                ApplyToControl(child, provider);
            }
        }
    }
}
