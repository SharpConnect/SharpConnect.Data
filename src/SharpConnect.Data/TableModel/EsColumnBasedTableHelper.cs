//MIT, 2015-2019, brezza92, EngineKit and contributors

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace SharpConnect.Data
{

    public static class EsColumnBasedTableHelper
    {
        public static Encoding s_defaultEncoding = Encoding.UTF8;
        public static EsColumnBasedTable CreateColumnBaseTableFromCsv(Stream stream, Encoding enc, bool firstRowIsColumns, char sep = ',')
        {
            var table = new EsColumnBasedTable();
            using (var reader = new StreamReader(stream, enc))
            {
                int line_id = 0;
                int col_count = 0;
                EsTableColumn[] columns = null;
                string firstline = reader.ReadLine();
                if (!firstRowIsColumns)
                {
                    //when first line is not column 
                    string[] cells = ParseCsvLine(firstline, sep);
                    col_count = cells.Length;
                    columns = new EsTableColumn[col_count];
                    for (int i = 0; i < col_count; ++i)
                    {
                        columns[i] = table.CreateDataColumn("col_" + i);
                    }
                    for (int i = 0; i < col_count; ++i)
                    {
                        columns[i].AppendData(cells[i]);
                    }
                }
                else
                {

                    string[] col_names = ParseCsvLine(firstline, sep);
                    col_count = col_names.Length;
                    columns = new EsTableColumn[col_count];
                    for (int i = 0; i < col_count; ++i)
                    {
                        columns[i] = table.CreateDataColumn(col_names[i]);
                    }
                }
                line_id++;
                string line = reader.ReadLine();
                while (line != null)
                {
                    string[] cells = ParseCsvLine(line,sep);
                    if (cells.Length != col_count)
                    {
                        throw new NotSupportedException("column count not match!");
                    }
                    for (int i = 0; i < col_count; ++i)
                    {
                        columns[i].AppendData(cells[i]);
                    }

                    line_id++;
                    line = reader.ReadLine();
                }
                reader.Close();
            }
            return table;
        }
        public static EsColumnBasedTable CreateColumnBaseTableFromCsv(string file, Encoding enc, bool firstRowIsColumns)
        {
            using (var fs = new FileStream(file, FileMode.Open))
            {
                return CreateColumnBaseTableFromCsv(fs, enc, firstRowIsColumns);
            }
        }

        static string[] ParseCsvLine(string csvline, char sep)
        {
            char[] buffer = csvline.ToCharArray();
            List<string> output = new List<string>();
            int j = buffer.Length;
            int state = 0;
            //TODO: optimize currentBuffer
            List<char> currentBuffer = new List<char>();
            for (int i = 0; i < j; ++i)
            {
                char c = buffer[i];
                switch (state)
                {
                    case 0: //init
                        {
                            if (c == '"')
                            {
                                state = 1;
                            }
                            else if (c == sep)
                            {
                                output.Add(new string(currentBuffer.ToArray()));
                                currentBuffer.Clear();
                            }
                            else
                            {
                                state = 2;
                                currentBuffer.Add(c);
                            }
                        }
                        break;
                    case 1:  //string escape
                        {
                            if (c == '"')
                            {
                                state = 2;
                            }
                            else
                            {
                                currentBuffer.Add(c);
                            }
                        }
                        break;
                    case 2:
                        {
                            if (c == sep)
                            {
                                output.Add(new string(currentBuffer.ToArray()));
                                currentBuffer.Clear();
                            }
                            else
                            {
                                if (c == '"')
                                {
                                    state = 1;
                                }
                                else
                                {
                                    currentBuffer.Add(c);
                                }
                            }
                        }
                        break;
                }
            }
            if (currentBuffer.Count > 0)
            {

                output.Add(new string(currentBuffer.ToArray()));
            }
            else
            {
                if (state == 2)
                {
                    output.Add(new string(currentBuffer.ToArray()));
                }
            }
            return output.ToArray();
        }

        public static void AddColumns(this EsColumnBasedTable table, params string[] columnNames)
        {
            int j = columnNames.Length;
            for (int i = 0; i < j; ++i)
            {
                table.CreateDataColumn(columnNames[i]);
            }
        }
        public static void SaveAsCsvFile(this EsColumnBasedTable table, string filename, Encoding enc = null)
        {
            if (enc == null)
            {
                enc = s_defaultEncoding;
            }
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            using (StreamWriter w = new StreamWriter(fs, enc))
            {
                //1. table column 
                int colCount = table.ColumnCount;
                for (int i = 0; i < colCount; ++i)
                {
                    if (i > 0)
                    {
                        w.Write(',');
                    }
                    var col = table.GetColumn(i);
                    w.Write('"');
                    w.Write(col.ColumnName);
                    w.Write('"');
                }

                //2. rows
                int rowCount = table.RowCount;
                for (int r = 0; r < rowCount; ++r)
                {
                    w.WriteLine();
                    for (int c = 0; c < colCount; ++c)
                    {
                        if (c > 0)
                        {
                            w.Write(',');
                        }
                        object cell = table.GetCellData(r, c);
                        w.Write('"');
                        w.Write(cell.ToString());
                        w.Write('"');
                    }
                }

                w.Close();
                fs.Close();
            }
        }
        public static EsColumnBasedTable Clone(this EsColumnBasedTable table, EsTableColumn[] selectedColumns = null)
        {
            int allColCount = 0;
            if (selectedColumns == null)
            {
                //=> select all
                allColCount = table.ColumnCount;
                selectedColumns = new EsTableColumn[allColCount];
                for (int i = 0; i < allColCount; ++i)
                {
                    selectedColumns[i] = table.GetColumn(i);
                }
            }
            else
            {
                allColCount = selectedColumns.Length;
            }
            //else clone only selected columns

            EsColumnBasedTable newTable = new EsColumnBasedTable();
            //1. create new columns
            for (int n = 0; n < allColCount; ++n)
            {
                EsTableColumn orgColumn = selectedColumns[n];
                EsTableColumn newColumn = newTable.CreateDataColumn(orgColumn.ColumnName);
                EsTableColumn.CloneAllCells(orgColumn, newColumn);
            }
            return newTable;
        }

        public static void SaveAsJsArrayFile(this EsColumnBasedTable table, string filename, Encoding enc = null)
        {
            if (enc == null)
            {
                enc = s_defaultEncoding;
            }

            using (FileStream fs = new FileStream(filename, FileMode.Create))
            using (StreamWriter w = new StreamWriter(fs, enc))
            {
                //1. table column 
                int colCount = table.ColumnCount;
                w.Write('[');
                {
                    w.Write('[');
                    for (int i = 0; i < colCount; ++i)
                    {
                        if (i > 0)
                        {
                            w.Write(',');
                        }
                        var col = table.GetColumn(i);
                        w.Write('"');
                        w.Write(col.ColumnName);
                        w.Write('"');
                    }
                    w.Write(']');

                    //2. rows
                    int rowCount = table.RowCount;
                    for (int r = 0; r < rowCount; ++r)
                    {
                        w.Write(',');
                        w.WriteLine();
                        w.Write('[');
                        for (int c = 0; c < colCount; ++c)
                        {
                            if (c > 0)
                            {
                                w.Write(',');
                            }
                            object cell = table.GetCellData(r, c);
                            w.Write('"');
                            w.Write(cell.ToString());
                            w.Write('"');
                        }
                        w.Write(']');
                    }
                }
                w.Write(']');
                w.Close();
                fs.Close();
            }
        }


        public static EsColumnBasedTable CreateNewTable(this EsColumnBasedTable srcTable, string[] selectedColumns, RowEvaluator rowEval = null)
        {
            int[] selected_colIndexs = new int[selectedColumns.Length];
            for (int i = 0; i < selectedColumns.Length; ++i)
            {
                selected_colIndexs[i] = srcTable.GetColumnIndex(selectedColumns[i]);
            }

            EsColumnBasedTable newTable = new EsColumnBasedTable();
            newTable.AddColumns(selectedColumns);
            int rowCount = srcTable.RowCount;

            int colCount = selectedColumns.Length;
            object[] newRow = new object[colCount];

            if (rowEval != null)
            {
                RowVisitor rowVisitor = new RowVisitor();
                rowVisitor._cells = newRow;
                for (int r = 0; r < rowCount && !rowVisitor._stop; ++r)
                {
                    rowVisitor._skipThisRow = false;//reset
                    for (int c = 0; c < newRow.Length; ++c)
                    {
                        newRow[c] = srcTable.GetCellData(r, selected_colIndexs[c]);
                    }
                    //
                    rowEval(rowVisitor);
                    //
                    if (rowVisitor.SkipThisRow)
                    {
                        continue;
                    }
                    newTable.AppendNewRow(newRow);
                }
            }
            else
            {
                for (int r = 0; r < rowCount; ++r)
                {
                    for (int c = 0; c < newRow.Length; ++c)
                    {
                        newRow[c] = srcTable.GetCellData(r, c);
                    }
                    newTable.AppendNewRow(newRow);
                }
            }

            return newTable;
        }



        public delegate void RowEvaluator(RowVisitor vis);

        public class RowVisitor
        {
            internal object[] _cells;
            internal bool _stop;
            internal bool _skipThisRow;

            public object[] Cells => _cells;
            public bool SkipThisRow
            {
                get => _skipThisRow;
                set => _skipThisRow = value;
            }
            public void Stop() { _stop = true; }
            public int RowCount { get; set; }
        }

    }
}