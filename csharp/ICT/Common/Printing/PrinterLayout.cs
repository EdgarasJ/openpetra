﻿/*************************************************************************
 *
 * DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * @Authors:
 *       timop
 *
 * Copyright 2004-2009 by OM International
 *
 * This file is part of OpenPetra.org.
 *
 * OpenPetra.org is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * OpenPetra.org is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
 *
 ************************************************************************/
using System;
using System.Xml;
using System.Collections.Generic;

namespace Ict.Common.Printing
{
    /// A helper class, to enable the GfxPrinter to call the functions that will be
    /// provided by TReportPrinter, which is derived from this class.
    public abstract class TPrinterLayout
    {
        /// <summary>todoComment</summary>
        public abstract void StartPrintDocument();

        /// <summary>todoComment</summary>
        public abstract void PrintPageHeader();

        /// <summary>todoComment</summary>
        public abstract void PrintPageBody();

        /// <summary>todoComment</summary>
        public abstract void PrintPageFooter();

        /// <summary>
        /// this should be overwritten e.g. by a function that knows to interpret HTML etc
        /// </summary>
        /// <param name="AXPos">the X position to start the content</param>
        /// <param name="AWidthAvailable">AWidthAvailable</param>
        /// <param name="content"></param>
        /// <returns>s the height of the content</returns>
        public virtual float RenderContent(float AXPos, float AWidthAvailable, ref XmlNode content)
        {
            return -1;
        }
    }

    /// <summary>todoComment</summary>
    public class TTableCellGfx
    {
        /// <summary>todoComment</summary>
        public float columnWidthInPercentage;

        /// <summary>can be html code</summary>
        public XmlNode content;

        /// needed for table headers (th)
        public bool bold;

        /// align the content of the cell
        public eAlignment align = eAlignment.eLeft;

        /// <summary>todoComment</summary>
        public Int32 borderWidth;

        /// <summary>this height has been calculated while printing the content</summary>
        public float contentHeight;

        /// <summary>todoComment</summary>
        public float contentWidth;
    }

    /// <summary>todoComment</summary>
    public class TTableRowGfx
    {
        /// <summary>todoComment</summary>
        public List <TTableCellGfx>cells;

        /// <summary>todoComment</summary>
        public float contentHeight;
    }
}