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
using System.Data;
using System.Data.Odbc;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ict.Common.Data
{
    /// <summary>
    /// This is the base class for the typed datatables.
    /// </summary>
    [Serializable()]
    public abstract class TTypedDataTable : DataTable
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">name of table</param>
        public TTypedDataTable(string name) : base(name)
        {
            this.InitClass();
            this.InitVars();
        }

        /// <summary>
        /// on purpose, this constructor does not call InitClass or InitVars;
        /// used for serialization
        /// </summary>
        /// <param name="tab">table for copying the table name</param>
        public TTypedDataTable(DataTable tab) : base(tab.TableName)
        {
            // System.Console.WriteLine('TTypedDataTable constructor tab:DataTable');
        }

        /// <summary>
        /// default constructor
        /// not needed, but for clarity
        /// </summary>
        public TTypedDataTable() : base()
        {
            this.InitClass();
            this.InitVars();
        }

        /// <summary>
        /// serialization constructor
        /// </summary>
        /// <param name="info">required for serialization</param>
        /// <param name="context">required for serialization</param>
        public TTypedDataTable(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            // Console.WriteLine('TTypeddatatable.create serialization');
            this.InitVars();
        }

        /// <summary>
        /// abstract method to be implemented by generated code
        /// </summary>
        protected abstract void InitClass();

        /// <summary>
        /// abstract method to be implemented by generated code
        /// </summary>
        public abstract void InitVars();

        /// <summary>
        /// abstract method to be implemented by generated code
        /// </summary>
        public abstract OdbcParameter CreateOdbcParameter(Int32 AColNumber);

        /// <summary>
        /// make sure that we use GetChangesType instead of GetChanges
        /// </summary>
        /// <returns>throws an exception</returns>
        public new DataTable GetChanges()
        {
            throw new Exception("Note to the developer: use GetChangesTyped instead of DataTable.GetChanges");

            // return null;
        }

        /// <summary>
        /// our own version of GetChanges
        /// </summary>
        /// <returns>returns a typed table with the changes</returns>
        public DataTable GetChangesTypedInternal()
        {
            DataTable ReturnValue;

            ReturnValue = base.GetChanges();

            if (ReturnValue != null)
            {
                // might not be necessary. The casting in the derived class might already call the contructor?
                ((TTypedDataTable)ReturnValue).InitVars();
            }

            return ReturnValue;
        }

        /// <summary>
        /// the number of rows in the current table
        /// </summary>
        public int Count
        {
            get
            {
                return this.Rows.Count;
            }
        }

        /// <summary>
        /// remove columns that are not needed
        /// </summary>
        /// <param name="ATableTemplate">this table only contains the columns that should be kept</param>
        public void RemoveColumnsNotInTableTemplate(DataTable ATableTemplate)
        {
            DataUtilities.RemoveColumnsNotInTableTemplate(this, ATableTemplate);
        }

        /// <summary>
        /// stores information about typed tables
        /// </summary>
        protected static SortedList <short, TTypedTableInfo>TableInfo = new SortedList <short, TTypedTableInfo>();

        /// will be filled by generated code
        public class TTypedColumnInfo
        {
            /// identification of the column, by order
            public short orderNumber;

            /// nice name of column (CamelCase)
            public string name;

            /// name of the column as it is in the SQL database
            public string dbname;

            /// odbc type of the column
            public System.Data.Odbc.OdbcType odbctype;

            /// if this type has a length, here it is
            public Int32 length;

            /// can the column never be NULL
            public bool bNotNull;

            /// constructor
            public TTypedColumnInfo(short AOrderNumber,
                string AName,
                string ADBName,
                System.Data.Odbc.OdbcType AOdbcType,
                Int32 ALength,
                bool ANotNull)
            {
                orderNumber = AOrderNumber;
                name = AName;
                dbname = ADBName;
                odbctype = AOdbcType;
                length = ALength;
                bNotNull = ANotNull;
            }
        }

        /// will be filled by generated code
        public class TTypedTableInfo
        {
            /// identification of the table, by order
            public short id;

            /// nice name of table (CamelCase)
            public string name;

            /// name of the table as it is in the SQL database
            public string dbname;

            /// the names of the columns that are part of the primary key
            public string[] PrimaryKeyColumns;

            /// the columns of this table
            public TTypedColumnInfo[] columns;

            /// constructor
            public TTypedTableInfo(short AId, string AName, string ADBName, TTypedColumnInfo[] AColumns, string[] APrimaryKeyColumns)
            {
                id = AId;
                name = AName;
                dbname = ADBName;
                columns = AColumns;
                PrimaryKeyColumns = APrimaryKeyColumns;
            }
        }

        /// the table name as it is in the SQL database
        public static string GetTableNameSQL(short ATableNumber)
        {
            return TableInfo[ATableNumber].dbname;
        }

        /// the table name in CamelCase
        public static string GetTableName(short ATableNumber)
        {
            return TableInfo[ATableNumber].name;
        }

        /// get the names of the columns that are part of the primary key
        public static string[] GetPrimaryKeyColumnStringList(short ATableNumber)
        {
            return TableInfo[ATableNumber].PrimaryKeyColumns;
        }

        /// get the names of the columns in this table
        public static string[] GetColumnStringList(short ATableNumber)
        {
            string[] ReturnValue = new string[TableInfo[ATableNumber].columns.Length];
            short counter = 0;

            foreach (TTypedColumnInfo col in TableInfo[ATableNumber].columns)
            {
                ReturnValue[counter++] = col.dbname;
            }

            return ReturnValue;
        }

        /// get the details of a column
        private static TTypedColumnInfo GetColumn(short ATableNumber, string colname)
        {
            foreach (TTypedColumnInfo col in TableInfo[ATableNumber].columns)
            {
                if ((col.name == colname) || (col.dbname == colname))
                {
                    return col;
                }
            }

            throw new Exception("TTypedDataTable::GetColumn cannot find column " + colname);
        }

        /// create an odbc parameter for the given column
        public static OdbcParameter CreateOdbcParameter(short ATableNumber, TSearchCriteria ASearchCriteria)
        {
            TTypedColumnInfo columnInfo = GetColumn(ATableNumber, ASearchCriteria.fieldname);

            if (columnInfo.odbctype == OdbcType.VarChar)
            {
                return new System.Data.Odbc.OdbcParameter("", columnInfo.odbctype, columnInfo.length);
            }

            return new System.Data.Odbc.OdbcParameter("", columnInfo.odbctype);
        }

        /// create an odbc parameter for the given column
        public static OdbcParameter CreateOdbcParameter(short ATableNumber, Int32 AColumnNr)
        {
            TTypedColumnInfo columnInfo = TableInfo[ATableNumber].columns[AColumnNr];

            if (columnInfo.odbctype == OdbcType.VarChar)
            {
                return new System.Data.Odbc.OdbcParameter("", columnInfo.odbctype, columnInfo.length);
            }

            return new System.Data.Odbc.OdbcParameter("", columnInfo.odbctype);
        }
    }
}