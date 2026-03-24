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
using System.Collections.Generic;
using System.Drawing;
using Dapplo.Windows.Common.Structs;
using Dapplo.Windows.User32.Enums;
using Dapplo.Windows.User32.Structs;
using Greenshot.Base.Interfaces.Drawing;
using Greenshot.Editor.Drawing.Fields;

namespace Greenshot.Editor.Configuration
{
    public partial class EditorConfigurationImpl
    {
        public void OnAfterLoad()
        {
            RecentColors ??= new List<Color>();

            if (FreehandSensitivity < 1)
            {
                FreehandSensitivity = 1;
            }
        }

        /// <param name="requestingType">Type of the class for which to create the field</param>
        /// <param name="fieldType">FieldType of the field to construct</param>
        /// <param name="preferredDefaultValue"></param>
        /// <returns>a new Field of the given fieldType, with the scope of it's value being restricted to the Type scope</returns>
        public IField CreateField(Type requestingType, IFieldType fieldType, object preferredDefaultValue)
        {
            string requestingTypeName = requestingType.Name;
            string requestedField = requestingTypeName + "." + fieldType.Name;
            object fieldValue = preferredDefaultValue;

            LastUsedFieldValues ??= new Dictionary<string, object>();

            if (LastUsedFieldValues.ContainsKey(requestedField))
            {
                if (LastUsedFieldValues[requestedField] != null)
                {
                    fieldValue = LastUsedFieldValues[requestedField];
                }
                else
                {
                    LastUsedFieldValues[requestedField] = fieldValue;
                }
            }
            else
            {
                LastUsedFieldValues.Add(requestedField, fieldValue);
            }

            return new Field(fieldType, requestingType)
            {
                Value = fieldValue
            };
        }

        public void UpdateLastFieldValue(IField field)
        {
            string requestedField = field.Scope + "." + field.FieldType.Name;
            LastUsedFieldValues ??= new Dictionary<string, object>();

            if (LastUsedFieldValues.ContainsKey(requestedField))
            {
                LastUsedFieldValues[requestedField] = field.Value;
            }
            else
            {
                LastUsedFieldValues.Add(requestedField, field.Value);
            }
        }

        public void ResetEditorPlacement()
        {
            WindowNormalPosition = new NativeRect(100, 100, 400, 400);
            WindowMaxPosition = new NativePoint(-1, -1);
            WindowMinPosition = new NativePoint(-1, -1);
            WindowPlacementFlags = 0;
            ShowWindowCommand = ShowWindowCommands.Normal;
        }

        public WindowPlacement GetEditorPlacement()
        {
            WindowPlacement placement = WindowPlacement.Create();
            placement.NormalPosition = WindowNormalPosition;
            placement.MaxPosition = WindowMaxPosition;
            placement.MinPosition = WindowMinPosition;
            placement.ShowCmd = ShowWindowCommand;
            placement.Flags = WindowPlacementFlags;
            return placement;
        }

        public void SetEditorPlacement(WindowPlacement placement)
        {
            WindowNormalPosition = placement.NormalPosition;
            WindowMaxPosition = placement.MaxPosition;
            WindowMinPosition = placement.MinPosition;
            ShowWindowCommand = placement.ShowCmd;
            WindowPlacementFlags = placement.Flags;
        }
    }
}
