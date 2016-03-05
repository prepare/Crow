﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cairo;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Crow
{
    public class TextBox : Label
    {
		#region CTOR
		public TextBox(string _initialValue)
			: base(_initialValue)
		{

		}

		public TextBox()
		{ }
		#endregion

		#region GraphicObject overrides
		[XmlIgnore]public override bool HasFocus   //trigger update when lost focus to errase text beam
        {
            get
            {
                return base.HasFocus;
            }
            set
            {
                base.HasFocus = value;
                RegisterForGraphicUpdate();
            }
        }
		[XmlAttributeAttribute()][DefaultValue(true)]
		public override bool Focusable
		{
			get { return base.Focusable; }
			set { base.Focusable = value; }
		}
		[XmlAttributeAttribute()][DefaultValue("White")]
		public override Fill Background {
			get { return base.Background; }
			set { base.Background = value; }
		}
		[XmlAttributeAttribute()][DefaultValue("Black")]
		public override Fill Foreground {
			get { return base.Foreground; }
			set { base.Foreground = value; }
		}

		protected override void onDraw (Context gr)
		{
			base.onDraw (gr);
			FontExtents fe = gr.FontExtents;
		}
		#endregion

		public event EventHandler<TextChangeEventArgs> TextChanged;

		public virtual void OnTextChanged(Object sender, TextChangeEventArgs e)
		{
			TextChanged.Raise (this, e);
		}
			
        #region Keyboard handling
		public override void onKeyDown (object sender, KeyboardKeyEventArgs e)
		{
			base.onKeyDown (sender, e);

			Key key = e.Key;

			switch (key)
			{
			case Key.Back:
				if (!selectionIsEmpty)
				{
//					Text = Text.Remove(selectionStart, selectionEnd - selectionStart);
//					selReleasePos = -1;
//					currentCol = selBeginPos;
				}
				else 
					this.DeleteChar();
				break;
			case Key.Clear:
				break;
			case Key.Delete:
				if (selectionIsEmpty)
					CurrentColumn++;
				this.DeleteChar ();
				break;
			case Key.Enter:
			case Key.KeypadEnter:
				if (Multiline)
					this.InsertLineBreak ();
				else
					OnTextChanged(this,new TextChangeEventArgs(Text));
				break;
			case Key.Escape:
				Text = "";
				CurrentColumn = 0;
				SelRelease = -1;
				break;
			case Key.Home:
				//TODO
				if (e.Control)
					CurrentLine = 0;
				CurrentColumn = 0;
				break;
			case Key.End:
				if (e.Control)
					CurrentLine = int.MaxValue;
				CurrentColumn = int.MaxValue;
				break;
			case Key.Insert:
				break;
			case Key.Left:				
				CurrentColumn--;
				break;
			case Key.Right:				
				CurrentColumn++;
				break;
			case Key.Up:
				CurrentLine--;
				break;
			case Key.Down:
				CurrentLine++;
				break;
			case Key.Menu:
				break;
			case Key.NumLock:
				break;
			case Key.PageDown:
				break;
			case Key.PageUp:
				break;
			case Key.RWin:
				break;
			case Key.Tab:
				this.Insert("\t");
				break;
			case Key.KeypadDecimal:
				this.Insert (".");					
				break;
			case Key.Space:
				this.Insert(" ");
				break;
			case Key.KeypadDivide:
			case Key.Slash:
				this.Insert("/");
				break;
			case Key.KeypadMultiply:
				this.Insert("*");
				break;
			case Key.KeypadMinus:
			case Key.Minus:
				this.Insert("-");
				break;
			case Key.KeypadPlus:
			case Key.Plus:
				this.Insert("+");
				break;
			case Key.ShiftLeft:
			case Key.ShiftRight:
			case Key.AltLeft:
			case Key.AltRight:
				break;
			case Key.Semicolon:
				this.Insert(";");
				break;
			default:
				if (!selectionIsEmpty)
				{
//					Text = Text.Remove(selectionStart, selectionEnd - selectionStart);
//					currentCol = selBeginPos;
				}

				string k = "?";
				if ((char)key >= 67 && (char)key <= 76)
					k = ((int)key - 67).ToString();
				else if ((char)key >= 109 && (char)key <= 118)
					k = ((int)key - 109).ToString();
				else if (e.Shift)
					k = key.ToString();
				else
					k = key.ToString().ToLower();

				this.Insert (k);

				SelRelease = -1;
				SelBegin = new Point(CurrentColumn, SelBegin.Y);

				break;
			}
			if (Width < 0)
				RegisterForLayouting (LayoutingType.Width);
			if (Height < 0)
				RegisterForLayouting (LayoutingType.Height);
			RegisterForGraphicUpdate();
		}
        #endregion
	} 
}
