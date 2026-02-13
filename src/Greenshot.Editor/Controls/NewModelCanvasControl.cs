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
using System.Linq;
using System.Windows.Forms;
using Dapplo.Windows.Common.Extensions;
using Dapplo.Windows.Common.Structs;
using Greenshot.Editor.Drawing.NewModel.Models;
using Greenshot.Editor.Drawing.NewModel.Renderers;
using log4net;

namespace Greenshot.Editor.Controls
{
    /// <summary>
    /// Control that displays and allows interaction with the new model-based canvas.
    /// Handles rendering, selection, and adorner interactions.
    /// </summary>
    public class NewModelCanvasControl : Control
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NewModelCanvasControl));

        private ShapeCanvas _canvas;
        private CanvasRenderer _renderer;
        private List<ShapeEditorState> _editorStates = new List<ShapeEditorState>();

        private Point _lastMousePosition;
        private Point _dragStartPosition;
        private bool _isDragging;
        private IShape _draggedShape;
        private int _draggedAdornerIndex = -1;

        public NewModelCanvasControl()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            TabStop = true; // Allow keyboard input
            
            // Ensure control can receive focus
            SetStyle(ControlStyles.Selectable, true);
        }

        public void SetCanvas(ShapeCanvas canvas, CanvasRenderer renderer)
        {
            _canvas = canvas;
            _renderer = renderer;
            Invalidate();
        }

        public IEnumerable<IShape> GetSelectedShapes()
        {
            return _editorStates.Where(s => s.IsSelected).Select(s => s.Shape);
        }

        public void ClearSelection()
        {
            _editorStates.Clear();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_canvas == null || _renderer == null)
            {
                e.Graphics.DrawString("No canvas loaded", Font, Brushes.Gray, 10, 10);
                return;
            }

            try
            {
                // Render the canvas with editor states
                _renderer.RenderCanvas(e.Graphics, _canvas, _editorStates);
            }
            catch (Exception ex)
            {
                Log.Error("Error rendering canvas", ex);
                e.Graphics.DrawString($"Render error: {ex.Message}", Font, Brushes.Red, 10, 10);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Ensure control has focus for keyboard input
            if (!Focused)
            {
                Focus();
            }

            if (_canvas == null) return;

            _lastMousePosition = e.Location;
            _dragStartPosition = e.Location;

            bool ctrlPressed = (ModifierKeys & Keys.Control) == Keys.Control;
            bool shiftPressed = (ModifierKeys & Keys.Shift) == Keys.Shift;

            // Check if clicked on an adorner of a selected shape
            foreach (var state in _editorStates.Where(s => s.IsSelected && s.ShowAdorners))
            {
                var adorners = ((state.Shape as IAdornerConfiguration)?.GetAdorners(state.Shape) ?? GetDefaultAdorners(state.Shape.Bounds)).ToArray();

                for (int i = 0; i < adorners.Length; i++)
                {
                    var adorner = adorners[i];
                    var adornerRect = new Rectangle(adorner.Position.X - 4, adorner.Position.Y - 4, 8, 8);
                    if (adornerRect.Contains(e.Location))
                    {
                        _isDragging = true;
                        _draggedShape = state.Shape;
                        _draggedAdornerIndex = i;
                        return;
                    }
                }
            }

            // Check if clicked on a shape
            var clickedShape = HitTest(e.Location);
            if (clickedShape != null)
            {
                var existingState = _editorStates.FirstOrDefault(s => s.Shape.Id == clickedShape.Id);

                if (ctrlPressed)
                {
                    // Ctrl+Click: Toggle selection of this shape
                    if (existingState != null)
                    {
                        _editorStates.Remove(existingState);
                    }
                    else
                    {
                        var newState = new ShapeEditorState(clickedShape)
                        {
                            IsSelected = true,
                            ShowAdorners = true
                        };
                        _editorStates.Add(newState);
                    }
                }
                else if (shiftPressed)
                {
                    // Shift+Click: Add to selection
                    if (existingState == null)
                    {
                        var newState = new ShapeEditorState(clickedShape)
                        {
                            IsSelected = true,
                            ShowAdorners = true
                        };
                        _editorStates.Add(newState);
                    }
                }
                else
                {
                    // Normal click: Select only this shape
                    if (existingState == null)
                    {
                        // Clear previous selection and select this shape
                        _editorStates.Clear();
                        var newState = new ShapeEditorState(clickedShape)
                        {
                            IsSelected = true,
                            ShowAdorners = true
                        };
                        _editorStates.Add(newState);
                    }
                    // If already selected, keep it selected (for dragging)
                }

                _draggedShape = clickedShape;
                _isDragging = true;
                Invalidate();
            }
            else
            {
                // Clicked on empty space - clear selection unless Ctrl/Shift is pressed
                if (!ctrlPressed && !shiftPressed)
                {
                    _editorStates.Clear();
                    Invalidate();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging && _draggedShape != null)
            {
                int dx = e.X - _lastMousePosition.X;
                int dy = e.Y - _lastMousePosition.Y;

                if (_draggedAdornerIndex >= 0)
                {
                    // Resize shape using adorner
                    ResizeShape(_draggedShape, _draggedAdornerIndex, dx, dy);
                }
                else
                {
                    // Move shape
                    var newBounds = _draggedShape.Bounds;
                    newBounds.Offset(dx, dy);
                    _draggedShape.Bounds = newBounds;
                }

                _lastMousePosition = e.Location;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isDragging = false;
            _draggedShape = null;
            _draggedAdornerIndex = -1;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle Ctrl+A for select all
            if (keyData == (Keys.Control | Keys.A))
            {
                if (_canvas != null)
                {
                    _editorStates.Clear();
                    foreach (var shape in _canvas.Shapes)
                    {
                        var state = new ShapeEditorState(shape)
                        {
                            IsSelected = true,
                            ShowAdorners = true
                        };
                        _editorStates.Add(state);
                    }
                    Invalidate();
                    return true;
                }
            }

            // Handle Delete key
            if (keyData == Keys.Delete)
            {
                var selectedShapes = _editorStates.Where(s => s.IsSelected).Select(s => s.Shape).ToList();
                foreach (var shape in selectedShapes)
                {
                    _canvas?.RemoveShape(shape);
                }
                _editorStates.Clear();
                Invalidate();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private IShape HitTest(Point location)
        {
            // Test in reverse Z-order (top to bottom)
            var shapes = _canvas.GetShapesOrderedByLayer().Reverse();
            
            foreach (var shape in shapes)
            {
                if (shape.Bounds.Contains(location.X, location.Y))
                {
                    return shape;
                }
            }

            return null;
        }

        private void ResizeShape(IShape shape, int adornerIndex, int dx, int dy)
        {
            var bounds = shape.Bounds;

            // Standard 8-position adorner resize
            switch (adornerIndex)
            {
                case 0: // Top-left
                    bounds = new NativeRect(bounds.Left + dx, bounds.Top + dy, bounds.Width - dx, bounds.Height - dy);
                    break;
                case 1: // Top-center
                    bounds = new NativeRect(bounds.Left, bounds.Top + dy, bounds.Width, bounds.Height - dy);
                    break;
                case 2: // Top-right
                    bounds = new NativeRect(bounds.Left, bounds.Top + dy, bounds.Width + dx, bounds.Height - dy);
                    break;
                case 3: // Right-center
                    bounds = new NativeRect(bounds.Left, bounds.Top, bounds.Width + dx, bounds.Height);
                    break;
                case 4: // Bottom-right
                    bounds = new NativeRect(bounds.Left, bounds.Top, bounds.Width + dx, bounds.Height + dy);
                    break;
                case 5: // Bottom-center
                    bounds = new NativeRect(bounds.Left, bounds.Top, bounds.Width, bounds.Height + dy);
                    break;
                case 6: // Bottom-left
                    bounds = new NativeRect(bounds.Left + dx, bounds.Top, bounds.Width - dx, bounds.Height + dy);
                    break;
                case 7: // Left-center
                    bounds = new NativeRect(bounds.Left + dx, bounds.Top, bounds.Width - dx, bounds.Height);
                    break;
            }


            // Ensure minimum size
            if (bounds.Width < 10)
            {
                bounds = bounds.ChangeWidth(10);
            }

            if (bounds.Height < 10)
            {
                bounds = bounds.ChangeHeight(10);
            }

            shape.Bounds = bounds;
        }

        private List<CustomAdorner> GetDefaultAdorners(NativeRect bounds)
        {
            var adorners = new List<CustomAdorner>();
            var color = Color.White;

            // 8 standard adorner positions
            adorners.Add(new CustomAdorner("top-left", new Point(bounds.Left, bounds.Top), color, 8)); // Top-left
            adorners.Add(new CustomAdorner("top-center", new Point(bounds.Left + bounds.Width / 2, bounds.Top), color, 8)); // Top-center
            adorners.Add(new CustomAdorner("top-right", new Point(bounds.Right, bounds.Top), color, 8)); // Top-right
            adorners.Add(new CustomAdorner("middle-right", new Point(bounds.Right, bounds.Top + bounds.Height / 2), color, 8)); // Right-center
            adorners.Add(new CustomAdorner("bottom-right", new Point(bounds.Right, bounds.Bottom), color, 8)); // Bottom-right
            adorners.Add(new CustomAdorner("bottom-center", new Point(bounds.Left + bounds.Width / 2, bounds.Bottom), color, 8)); // Bottom-center
            adorners.Add(new CustomAdorner("bottom-left", new Point(bounds.Left, bounds.Bottom), color, 8)); // Bottom-left
            adorners.Add(new CustomAdorner("middle-left", new Point(bounds.Left, bounds.Top + bounds.Height / 2), color, 8)); // Left-center

            return adorners;
        }
    }
}
