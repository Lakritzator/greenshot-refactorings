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
using System.ComponentModel;
using System.Drawing;
using Dapplo.Ini;
using Dapplo.Windows.Common.Structs;
using Greenshot.Base.Core.Enums;

namespace Greenshot.Base.Core
{
    [IniSection("Core", Description = "Greenshot core configuration")]
    public interface ICoreConfiguration : IIniSection, INotifyPropertyChanged, IAfterLoad, IBeforeSave
    {
        [IniValue(Description = "The language in IETF format (e.g. en-US)")]
        string Language { get; set; }

        [IniValue(KeyName = "BetaTester", Description = "The user wants to be beta-tester, this enables some features not available otherwise.")]
        bool IsBetaTester { get; set; }

        [IniValue(Description = "Hotkey for starting the region capture", DefaultValue = "PrintScreen")]
        string RegionHotkey { get; set; }

        [IniValue(Description = "Hotkey for starting the window capture", DefaultValue = "Alt + PrintScreen")]
        string WindowHotkey { get; set; }

        [IniValue(Description = "Hotkey for starting the fullscreen capture", DefaultValue = "Ctrl + PrintScreen")]
        string FullscreenHotkey { get; set; }

        [IniValue(Description = "Hotkey for starting the last region capture", DefaultValue = "Shift + PrintScreen")]
        string LastregionHotkey { get; set; }

        [IniValue(Description = "Hotkey for opening the clipboard contents into the editor")]
        string ClipboardHotkey { get; set; }

        [IniValue(Description = "Is this the first time launch?", DefaultValue = "true")]
        bool IsFirstLaunch { get; set; }

        [IniValue(KeyName = "Destinations", Description = "Which destinations? Possible options (more might be added by plugins) are: Editor, FileDefault, FileWithDialog, Clipboard, Printer, EMail, Picker", DefaultValue = "Picker")]
        List<string> OutputDestinations { get; set; }

        [IniValue(Description = "Specify which formats we copy on the clipboard? Options are: PNG, HTML, HTMLDATAURL and DIB", DefaultValue = "PNG,DIB")]
        List<ClipboardFormat> ClipboardFormats { get; set; }

        [IniValue(Description = "Should the mouse be captured?", DefaultValue = "true")]
        bool CaptureMousepointer { get; set; }

        [IniValue(Description = "Use interactive window selection to capture? (false=Capture active window)", DefaultValue = "false")]
        bool CaptureWindowsInteractive { get; set; }

        [IniValue(Description = "Capture delay in milliseconds.", DefaultValue = "100")]
        int CaptureDelay { get; set; }

        [IniValue(Description = "The capture mode used to capture a screen. (Auto, FullScreen, Fixed)", DefaultValue = "Auto")]
        ScreenCaptureMode ScreenCaptureMode { get; set; }

        [IniValue(Description = "The screen number to capture when using ScreenCaptureMode Fixed.", DefaultValue = "1")]
        int ScreenToCapture { get; set; }

        [IniValue(Description = "The capture mode used to capture a Window (Screen, GDI, Aero, AeroTransparent, Auto).", DefaultValue = "Auto")]
        WindowCaptureMode WindowCaptureMode { get; set; }

        [IniValue(Description = "Enable/disable capture all children, very slow but will make it possible to use this information in the editor.", DefaultValue = "False")]
        bool WindowCaptureAllChildLocations { get; set; }

        [IniValue(Description = "The background color for a DWM window capture.")]
        Color DWMBackgroundColor { get; set; }

        [IniValue(Description = "Play a camera sound after taking a capture.", DefaultValue = "false")]
        bool PlayCameraSound { get; set; }

        [IniValue(Description = "Show a notification from the systray when a capture is taken.", DefaultValue = "true")]
        bool ShowTrayNotification { get; set; }

        [IniValue(Description = "Output file path.")]
        string OutputFilePath { get; set; }

        [IniValue(Description = "If the target file already exists True will make Greenshot always overwrite and False will display a 'Save-As' dialog.", DefaultValue = "true")]
        bool OutputFileAllowOverwrite { get; set; }

        [IniValue(Description = "Filename pattern for screenshot.", DefaultValue = "${capturetime:d\"yyyy-MM-dd HH_mm_ss\"}-${title}")]
        string OutputFileFilenamePattern { get; set; }

        [IniValue(Description = "Default file type for writing screenshots. (bmp, gif, jpg, png, tiff)", DefaultValue = "png")]
        OutputFormat OutputFileFormat { get; set; }

        [IniValue(Description = "If set to true, than the colors of the output file are reduced to 256 (8-bit) colors", DefaultValue = "false")]
        bool OutputFileReduceColors { get; set; }

        [IniValue(Description = "If set to true the amount of colors is counted and if smaller than 256 the color reduction is automatically used.", DefaultValue = "false")]
        bool OutputFileAutoReduceColors { get; set; }

        [IniValue(Description = "Amount of colors to reduce to, when reducing", DefaultValue = "256")]
        int OutputFileReduceColorsTo { get; set; }

        [IniValue(Description = "When saving a screenshot, copy the path to the clipboard?", DefaultValue = "true")]
        bool OutputFileCopyPathToClipboard { get; set; }

        [IniValue(Description = "SaveAs Full path?")]
        string OutputFileAsFullpath { get; set; }

        [IniValue(Description = "JPEG file save quality in %.", DefaultValue = "80")]
        int OutputFileJpegQuality { get; set; }

        [IniValue(Description = "Ask for the quality before saving?", DefaultValue = "false")]
        bool OutputFilePromptQuality { get; set; }

        [IniValue(Description = "The number for the ${NUM} in the filename pattern, is increased automatically after each save.", DefaultValue = "1")]
        uint OutputFileIncrementingNumber { get; set; }

        [IniValue(Description = "Ask for print options when printing?", DefaultValue = "true")]
        bool OutputPrintPromptOptions { get; set; }

        [IniValue(Description = "Allow rotating the picture for fitting on paper?", DefaultValue = "false")]
        bool OutputPrintAllowRotate { get; set; }

        [IniValue(Description = "Allow growing the picture for fitting on paper?", DefaultValue = "false")]
        bool OutputPrintAllowEnlarge { get; set; }

        [IniValue(Description = "Allow shrinking the picture for fitting on paper?", DefaultValue = "true")]
        bool OutputPrintAllowShrink { get; set; }

        [IniValue(Description = "Center image when printing?", DefaultValue = "true")]
        bool OutputPrintCenter { get; set; }

        [IniValue(Description = "Print image inverted (use e.g. for console captures)", DefaultValue = "false")]
        bool OutputPrintInverted { get; set; }

        [IniValue(Description = "Force grayscale printing", DefaultValue = "false")]
        bool OutputPrintGrayscale { get; set; }

        [IniValue(Description = "Force monochrome printing", DefaultValue = "false")]
        bool OutputPrintMonochrome { get; set; }

        [IniValue(Description = "Threshold for monochrome filter (0 - 255), lower value means less black", DefaultValue = "127")]
        byte OutputPrintMonochromeThreshold { get; set; }

        [IniValue(Description = "Print footer on print?", DefaultValue = "true")]
        bool OutputPrintFooter { get; set; }

        [IniValue(Description = "Footer pattern", DefaultValue = "${capturetime:d\"D\"} ${capturetime:d\"T\"} - ${title}")]
        string OutputPrintFooterPattern { get; set; }

        [IniValue(Description = "The wav-file to play when a capture is taken, loaded only once at the Greenshot startup", DefaultValue = "default")]
        string NotificationSound { get; set; }

        [IniValue(Description = "Use your global proxy?", DefaultValue = "True")]
        bool UseProxy { get; set; }

        [IniValue(Description = "Sets how to compare the colors for the autocrop detection, the higher the more is 'selected'. Possible values are from 0 to 255, where everything above ~150 doesn't make much sense!", DefaultValue = "10")]
        int AutoCropDifference { get; set; }

        [IniValue(Description = "Comma separated list of Plugins which are allowed. If something in the list, than every plugin not in the list will not be loaded!")]
        List<string> IncludePlugins { get; set; }

        [IniValue(Description = "Comma separated list of Plugins which are NOT allowed.")]
        List<string> ExcludePlugins { get; set; }

        [IniValue(Description = "Comma separated list of destinations which should be disabled.")]
        List<string> ExcludeDestinations { get; set; }

        [IniValue(Description = "How many days between every update check? (0=no checks)", DefaultValue = "14")]
        int UpdateCheckInterval { get; set; }

        [IniValue(Description = "Last update check")]
        DateTime LastUpdateCheck { get; set; }

        [IniValue(Description = "Enable/disable the access to the settings, can only be changed manually in this .ini", DefaultValue = "False")]
        bool DisableSettings { get; set; }

        [IniValue(Description = "Enable/disable the access to the quick settings, can only be changed manually in this .ini", DefaultValue = "False")]
        bool DisableQuickSettings { get; set; }

        [IniValue(KeyName = "DisableTrayicon", Description = "Disable the trayicon, can only be changed manually in this .ini", DefaultValue = "False")]
        bool HideTrayicon { get; set; }

        [IniValue(Description = "Hide expert tab in the settings, can only be changed manually in this .ini", DefaultValue = "False")]
        bool HideExpertSettings { get; set; }

        [IniValue(Description = "Enable/disable thumbnail previews", DefaultValue = "True")]
        bool ThumnailPreview { get; set; }

        [IniValue(Description = "List of productnames for which GDI capturing is skipped (using fallback).", DefaultValue = "IntelliJ IDEA")]
        List<string> NoGDICaptureForProduct { get; set; }

        [IniValue(Description = "List of productnames for which DWM capturing is skipped (using fallback).", DefaultValue = "Citrix ICA Client")]
        List<string> NoDWMCaptureForProduct { get; set; }

        [IniValue(Description = "Make some optimizations for usage with remote desktop", DefaultValue = "False")]
        bool OptimizeForRDP { get; set; }

        [IniValue(Description = "Disable all optimizations for usage with remote desktop", DefaultValue = "False")]
        bool DisableRDPOptimizing { get; set; }

        [IniValue(Description = "Optimize memory footprint, but with a performance penalty!", DefaultValue = "False")]
        bool MinimizeWorkingSetSize { get; set; }

        [IniValue(Description = "Remove the corners from a window capture", DefaultValue = "True")]
        bool WindowCaptureRemoveCorners { get; set; }

        [IniValue(Description = "Also check for unstable version updates", DefaultValue = "False")]
        bool CheckForUnstable { get; set; }

        [IniValue(Description = "The fixes that are active.")]
        List<string> ActiveTitleFixes { get; set; }

        [IniValue(Description = "The regular expressions to match the title with.")]
        Dictionary<string, string> TitleFixMatcher { get; set; }

        [IniValue(Description = "The replacements for the matchers.")]
        Dictionary<string, string> TitleFixReplacer { get; set; }

        [IniValue(Description = "A list of experimental features, this allows us to test certain features before releasing them.")]
        List<string> ExperimentalFeatures { get; set; }

        [IniValue(Description = "Enable a special DIB clipboard reader", DefaultValue = "True")]
        bool EnableSpecialDIBClipboardReader { get; set; }

        [IniValue(Description = "The cutshape which is used to remove the window corners, is mirrored for all corners", DefaultValue = "5,3,2,1,1")]
        List<int> WindowCornerCutShape { get; set; }

        [IniValue(Description = "Specify what action is made if the tray icon is left clicked, if a double-click action is specified this action is initiated after a delay (configurable via the windows double-click speed)", DefaultValue = "SHOW_CONTEXT_MENU")]
        ClickActions LeftClickAction { get; set; }

        [IniValue(Description = "Specify what action is made if the tray icon is double clicked", DefaultValue = "OPEN_LAST_IN_EXPLORER")]
        ClickActions DoubleClickAction { get; set; }

        [IniValue(Description = "Sets if the zoomer is enabled", DefaultValue = "True")]
        bool ZoomerEnabled { get; set; }

        [IniValue(Description = "Specify the transparency for the zoomer, from 0-1 (where 1 is no transparency and 0 is complete transparent. An useful setting would be 0.7)", DefaultValue = "1")]
        float ZoomerOpacity { get; set; }

        [IniValue(Description = "Maximum length of submenu items in the context menu, making this longer might cause context menu issues on dual screen systems.", DefaultValue = "25")]
        int MaxMenuItemLength { get; set; }

        [IniValue(Description = "The 'to' field for the email destination (settings for Outlook can be found under the Office section)", DefaultValue = "")]
        string MailApiTo { get; set; }

        [IniValue(Description = "The 'CC' field for the email destination (settings for Outlook can be found under the Office section)", DefaultValue = "")]
        string MailApiCC { get; set; }

        [IniValue(Description = "The 'BCC' field for the email destination (settings for Outlook can be found under the Office section)", DefaultValue = "")]
        string MailApiBCC { get; set; }

        [IniValue(Description = "Optional command to execute on a temporary PNG file, the command should overwrite the file and Greenshot will read it back. Note: this command is also executed when uploading PNG's!", DefaultValue = "")]
        string OptimizePNGCommand { get; set; }

        [IniValue(Description = "Arguments for the optional command to execute on a PNG, {0} is replaced by the temp-filename from Greenshot. Note: Temp-file is deleted afterwards by Greenshot.", DefaultValue = "\"{0}\"")]
        string OptimizePNGCommandArguments { get; set; }

        [IniValue(Description = "Version of Greenshot which created this .ini")]
        string LastSaveWithVersion { get; set; }

        [IniValue(Description = "When reading images from files or clipboard, use the EXIF information to correct the orientation", DefaultValue = "True")]
        bool ProcessEXIFOrientation { get; set; }

        [IniValue(Description = "The last used region, for reuse in the capture last region")]
        NativeRect LastCapturedRegion { get; set; }

        [IniValue(Description = "The capture is cropped with these settings, e.g. when you don't want to color around it -1,-1", DefaultValue = "0,0")]
        NativeSize Win10BorderCrop { get; set; }

        [IniValue(KeyName = "BaseIconSize", Description = "Defines the base size of the icons (e.g. for the buttons in the editor), default value 16,16 and it's scaled to the current DPI", DefaultValue = "16,16", NotifyPropertyChanged = true)]
        NativeSize IconSize { get; set; }

        [IniValue(Description = "The connect timeout value for web requests, these are seconds", DefaultValue = "100")]
        int WebRequestTimeout { get; set; }

        [IniValue(Description = "The read/write timeout value for web requests, these are seconds", DefaultValue = "100")]
        int WebRequestReadWriteTimeout { get; set; }
    }
}
