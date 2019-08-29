//MIT, 2016-2017, EngineKit
using System.Text;
namespace SharpConnect.Data
{
    class IndentTextWriter
    {
        StringBuilder _stBuilder;

        const string TAB = "\t";


        public IndentTextWriter(StringBuilder stBuilder)
        {
            this._stBuilder = stBuilder;
            NewLine = "\r\n";
        }

        public StringBuilder InnterStringBuilder => _stBuilder;

        public void Append(string str)
        {
            _stBuilder.Append(str);
        }
        public void Append(char c)
        {
            _stBuilder.Append(c);
        }
        public void OutputTabs()
        {
            for (int i = 0; i < IndentLevel; i++)
            {
                _stBuilder.Append(TAB);
            }
        }

        public void CloseLine()
        {
            _stBuilder.Append(NewLine);
            OutputTabs();
        }

        public void CloseLine(string str)
        {

            _stBuilder.Append(str);
            _stBuilder.Append(NewLine);
            OutputTabs();
        }

        public void CloseLineFinal(string str)
        {
            _stBuilder.Append(str);

        }
        public void CloseLine(char c)
        {

            _stBuilder.Append(c);
            _stBuilder.Append(NewLine);
            OutputTabs();
        }

        public void CloseLineNoTab(string str)
        {
            _stBuilder.Append(str);
            _stBuilder.Append(NewLine);
        }
        public void CloseLineNoTab()
        {
            _stBuilder.Append(NewLine);
        }

        public void CloseLineNoTab(char c)
        {
            _stBuilder.Append(c);
            _stBuilder.Append(NewLine);
        }


        public string NewLine { get; set; }
        public int IndentLevel { get; set; }


    }

}