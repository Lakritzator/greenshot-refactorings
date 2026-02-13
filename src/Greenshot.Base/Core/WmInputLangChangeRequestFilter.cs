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

namespace Greenshot.Base.Core
{
    /// <summary>
    /// This IMessageFilter filters out all WM_INPUTLANGCHANGEREQUEST messages which go to a handle which is >32 bits.
    /// The need for this is documented here: https://stackoverflow.com/a/32021586
    /// </summary>
    public class WmInputLangChangeRequestFilter : IMessageFilter
    {
        // Windows message constants for input language changes
        private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
        private const int WM_INPUTLANGCHANGE = 0x0051;

        /// <summary>
        /// This will do some filtering
        /// </summary>
        /// <param name="m">Message</param>
        /// <returns>true if the message should be filtered</returns>
        public bool PreFilterMessage(ref Message m)
        {
            return PreFilterMessageExternal(ref m);
        }

        /// <summary>
        /// Also used in the MainForm WndProc
        /// </summary>
        /// <param name="m">Message</param>
        /// <returns>true if the message should be filtered</returns>
        public static bool PreFilterMessageExternal(ref Message m)
        {
            // Optimize by comparing raw message value first to avoid enum casting overhead
            int msg = m.Msg;
            if (msg != WM_INPUTLANGCHANGEREQUEST && msg != WM_INPUTLANGCHANGE)
            {
                return false;
            }

            // For now we always return true
            return true;
            // But it could look something like this:
            //return (m.LParam.ToInt64() | 0x7FFFFFFF) != 0;
        }
    }
}