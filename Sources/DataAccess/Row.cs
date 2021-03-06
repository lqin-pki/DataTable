﻿using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace DataAccess
{
    /// <summary>
    /// Represents a row within a <see cref="DataTable"/>
    /// The Row may or may not be mutable, depending on whether the table is mutable.
    /// </summary>
    public abstract class Row
    {
        /// <summary>
        /// ordered collection of values for this row.
        /// The ordering matches the column ordering. 
        /// </summary>
        public abstract IList<string> Values { get ; }

        /// <summary>
        /// Column names for the table containing this row. This is a parallel collection to <see cref="Values"/>
        /// </summary>
        public abstract IEnumerable<string> ColumnNames { get; }
        
        // Write this single row to a CSV file
        internal void WriteCsv(TextWriter tw)
        {
            CsvWriter.RawWriteLine(this.Values, tw);
        }

        /// <summary>
        /// Return a fast compiled function that parses rows into a strongly typed object. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnNames">column names that will correspond to the row.</param>
        /// <returns>the parsing function</returns>
        public static Func<Row, T> BuildMethod<T>(IEnumerable<string> columnNames)
        {
            return StrongTypeBinder.BuildMethod<T>(columnNames);
        }

        /// <summary>
        /// Debug helper to show all values.
        /// </summary>
        public string[] DebugValues
        {
            get
            {
                string[] columnnNames = ColumnNames.ToArray();

                int numColumns = columnnNames.Length;
                var result = new string[numColumns];
                for (int i = 0; i < numColumns; i++)
                {
                    string name = columnnNames[i];
                    string value = this.Values[i];

                    result[i] = string.Format("{0}={1}", name, value);
                }
                return result;
            }
        }

        /// <summary>
        /// Lookup value by column name. Throws if column name is not valid.
        /// </summary>
        /// <param name="columnName">column name</param>
        /// <returns>the value in the given column</returns>    
        public virtual string this[string columnName]
        {
            get
            {
                int idx = GetColumnIndex(columnName);
                if (idx == -1)
                {
                    throw new ArgumentException("column is not found", columnName);
                }
                return Values[idx];
            }
            set
            {
                int idx = GetColumnIndex(columnName);
                if (idx == -1)
                {
                    throw new ArgumentException("column is not found", columnName);
                }
                Values[idx] = value; // this will check mutability.
            }
        }

        // $$$ Should be in base table.
        // -1 on not found
        protected int GetColumnIndex(string columnName)
        {
            int i = 0;
            foreach (string x in this.ColumnNames)
            {
                if (string.Compare(x, columnName, true) == 0)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        
        /// <summary>
        /// Lookup value by column name. Returns emtpy string if column name is not valid.
        /// </summary>
        /// <param name="columnName">name of column</param>
        /// <returns>value or empty string</returns>
        public string GetValueOrEmpty(string columnName)
        {
            int idx = GetColumnIndex(columnName);
            if (idx == -1)            
            {
                return string.Empty;
            }
            return Values[idx];
        }

        /// <summary>
        /// Plural version of <see cref="GetValueOrEmpty"/>
        /// </summary>
        /// <param name="columnName">enumeration of column names</param>
        /// <returns>enumeration of corresponding values</returns>
        public IEnumerable<string> GetValuesOrEmpty(IEnumerable<string> columnName)
        {
            foreach (var c in columnName)
            {
                string val = this.GetValueOrEmpty(c);
                yield return val;
            }
        }
    }
}