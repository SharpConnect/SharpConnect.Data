﻿//MIT, 2015-present, brezza92, EngineKit and contributors

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

        List<EsTableColumn> _dataColumns = new List<EsTableColumn>();
        Dictionary<string, int> _colNames = new Dictionary<string, int>();
        ColumnNameState _columnNameState = ColumnNameState.Dirty;
        enum ColumnNameState
        {
            Dirty,
            OK
        }

        public int RowCount => _dataColumns[0].RowCount;

        public int ColumnCount => _dataColumns.Count;

        public void RemoveColumn(int columnIndex)
        {
            //when user remove column or change column name
            //we must update index 
            _dataColumns.RemoveAt(columnIndex);
            _columnNameState = ColumnNameState.Dirty;
        }
        public string GetColumnName(int colIndex) => _dataColumns[colIndex].ColumnName;

        public object GetCellData(int row, int column) => _dataColumns[column].GetCellData(row);

        public EsTableColumn GetColumn(int index) => _dataColumns[index];

        public IEnumerable<EsTableColumn> GetColumnIterForward()
        {
            foreach (EsTableColumn col in _dataColumns)
            {
                yield return col;
            }
        }

        public EsTableColumn CreateDataColumn(string colName)
        {
            if (!_colNames.ContainsKey(colName))
            {
                var dataColumn = new EsTableColumn(this, colName);
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
                EsTableColumn col = _dataColumns[i];
                _colNames[col.ColumnName] = i;
            }

            _columnNameState = ColumnNameState.OK;
        }
        internal void InvalidateColumnNameState()
        {
            _columnNameState = ColumnNameState.Dirty;
        }

        public void AppendNewRow(object[] cells)
        {
            //append data to each column

            if (cells.Length != _dataColumns.Count)
            {
                throw new NotSupportedException("row count not match!");
            }

            for (int i = 0; i < cells.Length; ++i)
            {
                _dataColumns[i].AppendData(cells[i]);
            }
        }

    }

    public class EsTableColumn
    {
        EsColumnBasedTable _ownerTable;
        List<object> _cells = new List<object>();

        internal EsTableColumn(EsColumnBasedTable ownerTable, string name)
        {
            ColumnName = name;
            _ownerTable = ownerTable;
        }
        public int RowCount => _cells.Count;
        /// <summary>
        /// TODO: review here  ***
        /// </summary>
        public string ColumnName { get; set; }
        public void NewBlankRows(int rowCount, object initData)
        {
            for (int i = rowCount - 1; i >= 0; --i)
            {
                _cells.Add(initData);
            }
        }
        public EsColumnTypeHint TypeHint { get; set; }

        public void AppendData(object data)
        {
            _cells.Add(data);
        }
        public object GetCellData(int rowIndex)
        {
            return _cells[rowIndex];
        }
        public void SetCellData(int rowIndex, object data)
        {
            _cells[rowIndex] = data;
        }
        public int FindRow(string data)
        {
            int j = _cells.Count;
            for (int i = 0; i < j; ++i)
            {
                if ((string)_cells[i] == data)
                {
                    return i;
                }
            }
            return -1;
        }
#if DEBUG
        public override string ToString() => ColumnName;
#endif
        public static void CloneAllCells(EsTableColumn origin, EsTableColumn target)
        {
            target._cells.AddRange(origin._cells);
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