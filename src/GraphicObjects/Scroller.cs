﻿//
// Scroller.cs
//
// Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
// Copyright (c) 2013-2017 Jean-Philippe Bruyère
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Diagnostics;
using Cairo;

namespace Crow
{
	public class Scroller : Container
	{
		bool _verticalScrolling;
		bool _horizontalScrolling;
		bool _scrollbarVisible;
		double _scrollX = 0.0;
		double _scrollY = 0.0;
		int scrollSpeed;

		public event EventHandler<ScrollingEventArgs> Scrolled;

		#region public properties
		[XmlAttributeAttribute][DefaultValue(true)]
		public bool VerticalScrolling {
			get { return _verticalScrolling; }
			set { _verticalScrolling = value; }
		}

		[XmlAttributeAttribute][DefaultValue(false)]
        public bool HorizontalScrolling {
			get { return _horizontalScrolling; }
			set { _horizontalScrolling = value; }
		}
		[XmlAttributeAttribute][DefaultValue(true)]
		public bool ScrollbarVisible {
			get { return _scrollbarVisible; }
			set { _scrollbarVisible = value; }
		}
		[XmlAttributeAttribute][DefaultValue(0.0)]
		public double ScrollX {
			get {
				return _scrollX;
			}
			set {
				if (_scrollX == value)
					return;
				if (value < 0.0)
					_scrollX = 0.0;
				else if (value > Child.Slot.Width - ClientRectangle.Width)
					_scrollX = Math.Max(0.0, Child.Slot.Width - ClientRectangle.Width);
				else
					_scrollX = value;
				NotifyValueChanged("ScrollX", _scrollX);
				RegisterForRedraw ();
				Scrolled.Raise (this, new ScrollingEventArgs (Orientation.Horizontal));
			}
		}
		[XmlAttributeAttribute][DefaultValue(0.0)]
		public double ScrollY {
			get {
				return _scrollY;
			}
			set {
				if (_scrollY == value)
					return;
				if (value < 0.0)
					_scrollY = 0.0;
				else if (value > Child.Slot.Height - ClientRectangle.Height)
					_scrollY = Math.Max(0.0,Child.Slot.Height - ClientRectangle.Height);
				else
					_scrollY = value;
				NotifyValueChanged("ScrollY", _scrollY);
				RegisterForRedraw ();
				Scrolled.Raise (this, new ScrollingEventArgs (Orientation.Vertical));
			}
		}

		[XmlIgnore]
		public int MaximumScroll {
			get {
				try {
					return VerticalScrolling ?
						Math.Max(Child.Slot.Height - ClientRectangle.Height,0) :
						Math.Max(Child.Slot.Width - ClientRectangle.Width,0);
				} catch {
					return 0;
				}
			}
		}

		[XmlAttributeAttribute][DefaultValue(30)]
		public int ScrollSpeed {
			get { return scrollSpeed; }
			set {
				scrollSpeed = value;
				NotifyValueChanged("ScrollSpeed", scrollSpeed);
			}
		}
		#endregion

        public Scroller()
            : base(){}

		#region GraphicObject Overrides
		public override void OnLayoutChanges (LayoutingType layoutType)
		{
			base.OnLayoutChanges (layoutType);

			NotifyValueChanged("MaximumScroll", MaximumScroll);
		}
		void OnChildLayoutChanges (object sender, LayoutingEventArgs arg)
		{
			//Debug.WriteLine ("scroller childLayoutChanges");
			int maxScroll = MaximumScroll;
			//Debug.WriteLine ("maxscroll={0}", maxScroll);
			if (_verticalScrolling) {
				if (arg.LayoutType == LayoutingType.Height) {
					if (maxScroll < ScrollY) {
						//Debug.WriteLine ("scrolly={0} maxscroll={1}", ScrollY, maxScroll);
						ScrollY = maxScroll;
					}
					NotifyValueChanged("MaximumScroll", maxScroll);
				}
			} else if (arg.LayoutType == LayoutingType.Width) {
				if (maxScroll < ScrollX) {
					//Debug.WriteLine ("scrolly={0} maxscroll={1}", ScrollY, maxScroll);
					ScrollX = maxScroll;
				}
				NotifyValueChanged("MaximumScroll", maxScroll);
			}
		}
		void onChildListCleared(object sender, EventArgs e){
			ScrollY = 0;
			ScrollX = 0;
		}
		public override void SetChild (GraphicObject _child)
		{
			GraphicObject c = child as GraphicObject;
			Group g = child as Group;
			if (c != null) {
				c.LayoutChanged -= OnChildLayoutChanges;
				if (g != null)
					g.ChildrenCleared -= onChildListCleared;
			}
			c = _child as GraphicObject;
			g = _child as Group;
			if (c != null) {
				c.LayoutChanged += OnChildLayoutChanges;
				if (g != null)
					g.ChildrenCleared += onChildListCleared;
			}
			base.SetChild (_child);
		}
		public override Rectangle ScreenCoordinates (Rectangle r)
		{
			return base.ScreenCoordinates (r) - new Point((int)ScrollX,(int)ScrollY);
		}
		protected override void onDraw (Context gr)
		{
			Rectangle rBack = new Rectangle (Slot.Size);

			Background.SetAsSource (gr, rBack);
			CairoHelpers.CairoRectangle(gr,rBack, CornerRadius);
			gr.Fill ();

			gr.Save ();
			if (ClipToClientRect) {
				//clip to scrolled client zone
				CairoHelpers.CairoRectangle (gr, ClientRectangle, CornerRadius);
				gr.Clip ();
			}

			gr.Translate (-ScrollX, -ScrollY);
			if (child != null)
				child.Paint (ref gr);
			gr.Restore ();
		}

		#region Mouse handling
		internal Point savedMousePos;
		public override bool MouseIsIn (Point m)
		{
			return Visible ? base.ScreenCoordinates(Slot).ContainsOrIsEqual (m) : false;
		}
		public override void checkHoverWidget (MouseMoveEventArgs e)
		{
			savedMousePos = e.Position;
			Point m = e.Position - new Point ((int)ScrollX, (int)ScrollY);
			base.checkHoverWidget (new MouseMoveEventArgs(m.X,m.Y,e.XDelta,e.YDelta));
		}
		public override void onMouseWheel (object sender, MouseWheelEventArgs e)
		{
			if (Child == null)
				return;

			if (VerticalScrolling )
				ScrollY -= e.Delta * ScrollSpeed;
			if (HorizontalScrolling )
				ScrollX -= e.Delta * ScrollSpeed;
		}
		public override void onMouseMove (object sender, MouseMoveEventArgs e)
		{
			savedMousePos.X += e.XDelta;
			savedMousePos.Y += e.YDelta;
			base.onMouseMove (sender, new MouseMoveEventArgs(savedMousePos.X,savedMousePos.Y,e.XDelta,e.YDelta));
		}
		public override void RegisterClip (Rectangle clip)
		{
			base.RegisterClip (clip - new Point((int)ScrollX,(int)ScrollY));
		}
		#endregion

		#endregion
    }
}
