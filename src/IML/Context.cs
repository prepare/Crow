﻿//
//  Context.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2016 jp
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;

namespace Crow.IML
{
	/// <summary>
	/// Context while parsing IML
	/// </summary>
	public class Context
	{
		public XmlTextReader reader = null;
		public Type RootType = null;
		public Node CurrentNode = null;
		public DynamicMethod dm = null;
		public ILGenerator il = null;
		//public SubNodeType curSubNodeType;
		public Stack<Node> nodesStack = new Stack<Node> ();

		public Dictionary<string, string> Names = new Dictionary<string, string> ();
		public Dictionary<string, Dictionary<string, MemberAddress>> PropertyBindings = new Dictionary<string, Dictionary<string, MemberAddress>> ();

		public Context (Type rootType)
		{
			RootType = rootType;
			dm = new DynamicMethod ("dyn_instantiator",
					MethodAttributes.Family | MethodAttributes.FamANDAssem | MethodAttributes.NewSlot,
					CallingConventions.Standard,
					typeof (void), new Type [] { typeof (object), typeof (Interface) }, RootType, true);
			il = dm.GetILGenerator (256);

			initILGen ();
		}
		void initILGen ()
		{
			il.DeclareLocal (typeof (GraphicObject));
			il.Emit (OpCodes.Nop);
			//set local GraphicObject to root object passed as 1st argument
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Stloc_0);
			CompilerServices.emitSetCurInterface (il);
		}

	}
}