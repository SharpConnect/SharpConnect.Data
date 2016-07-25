﻿//MIT 2015, brezza92, EngineKit and contributors

using System;
using System.Collections.Generic;
namespace SharpConnect.Data
{
    /// <summary>
    /// column-based table 
    /// </summary>
    public class EsColumnBasedTable
    {
        //note: this is column-based table

        List<EsDataColumn> _dataColumns = new List<EsDataColumn>();
        Dictionary<string, int> _colNames = new Dictionary<string, int>();
        ColumnNameState _columnNameState = ColumnNameState.Dirty;
        enum ColumnNameState
        {
            Dirty,
            OK
        }


        public int RowCount
        {
            get
            {
                return _dataColumns[0].RowCount;
            }
        }
        public int ColumnCount
        {
            get
            {
                return _dataColumns.Count;
            }
        }
        public void RemoveColumn(int columnIndex)
        {
            //when user remove column or change column name
            //we must update index 
            _dataColumns.RemoveAt(columnIndex);
            _columnNameState = ColumnNameState.Dirty;
        }
        public string GetColumnName(int colIndex)
        {
            return _dataColumns[colIndex].ColumnName;
        }
        public object GetCellData(int row, int column)
        {
            return _dataColumns[column].GetCellData(row);
        }
        public EsDataColumn GetColumn(int index)
        {
            return _dataColumns[index];
        }
        public IEnumerable<EsDataColumn> GetColumnIterForward()
        {
            foreach (EsDataColumn col in _dataColumns)
            {
                yield return col;
            }
        }

        public EsDataColumn CreateDataColumn(string colName)
        {
            if (!_colNames.ContainsKey(colName))
            {
                var dataColumn = new EsDataColumn(this, colName);
                _dataColumns.Add(dataColumn);
                _columnNameState = ColumnNameState.Dirty;
                return dataColumn;
            }
            else
            {
                throw new Exception("duplicate coloumn name " + colName);
            }
        }

        public int GetColumnIndex(string colname)
        {
            if (_columnNameState == ColumnNameState.Dirty)
            {
                //recreate table column names
                ValidateColumnNames();
            }

            int found;
            if (!_colNames.TryGetValue(colname, out found))
            {
                found = -1; //not found
            }
            return found;
        }
        void ValidateColumnNames()
        {
            _colNames.Clear();
            int j = _dataColumns.Count;
            for (int i = 0; i < j; ++i)
            {
                EsDataColumn col = _dataColumns[i];
                _colNames[col.ColumnName] = i;
            }

            _columnNameState = ColumnNameState.OK;
        }
        internal void InvalidateColumnNameState()
        {
            _columnNameState = ColumnNameState.Dirty;
        }
    }

    public class EsDataColumn
    {
        EsColumnBasedTable _ownerTable;
        List<object> _cells = new List<object>();
        string _name;
        internal EsDataColumn(EsColumnBasedTable ownerTable, string name)
        {
            ColumnName = name;
            _ownerTable = ownerTable;
        }
        public int RowCount
        {
            get
            {
                return _cells.Count;
            }
        }
        /// <summary>
        /// TODO: review here when da
        /// </summary>
        internal string ColumnName
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }
        public EsColumnTypeHint TypeHint
        {
            get;
            set;
        }
        public void AddData(object data)
        {
            _cells.Add(data);
        }
        public object GetCellData(int rowIndex)
        {
            return _cells[rowIndex];
        }
    }
    public enum EsColumnTypeHint
    {
        Unknown,
        String,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Double,
        Boolean
    }
}