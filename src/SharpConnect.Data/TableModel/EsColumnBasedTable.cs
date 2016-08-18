//MIT, 2015-2016, brezza92, EngineKit and contributors

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
        public EsTableColumn GetColumn(int index)
        {
            return _dataColumns[index];
        }
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
            int j = cells.Length;
            if (j != _dataColumns.Count)
            {
                throw new NotSupportedException("row count not match!");
            }
            for (int i = j - 1; i >= 0; --i)
            {
                _dataColumns[i].AppendData(cells[i]);
            }
        }

    }

    public class EsTableColumn
    {
        EsColumnBasedTable _ownerTable;
        List<object> _cells = new List<object>();
        string _name;
        internal EsTableColumn(EsColumnBasedTable ownerTable, string name)
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
        /// TODO: review here  ***
        /// </summary>
        public string ColumnName
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

        public void AppendData(object data)
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