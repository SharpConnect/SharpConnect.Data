//MIT, 2015-2016 EngineKit and contributors 
using System;
using System.Collections.Generic;

namespace SharpConnect.Data
{

    public class EsUniqueStringTable
    {
        Dictionary<string, int> _dic;
        List<string> _list;

        public EsUniqueStringTable()
        {
            _dic = new Dictionary<string, int>();
            _list = new List<string>();
            _dic.Add("", 0);
            _list.Add("");
        }

        public int GetStringIndex(string str)
        {

            if (str == null)
            {
                return 0;
            }
            int foundIndex;
            if (_dic.TryGetValue(str, out foundIndex))
            {
                return foundIndex;
            }
            else
            {
                return -1;
            }
        }

        public int AddStringIfNotExist(string str)
        {

            if (str == null)
            {
                return 0;
            }
            //---------------------------------------
            int foundIndex;
            if (_dic.TryGetValue(str, out foundIndex))
            {
                return foundIndex;
            }
            else
            {
                int index = _dic.Count;
                _dic.Add(str, index);
                _list.Add(str);
                return index;
            }
        }

        public bool Contains(string str) => _dic.ContainsKey(str);

        public int Count => _dic.Count;

        public string GetString(int index) => _list[index];

        public IEnumerable<string> WordIter
        {
            get
            {
                foreach (string str in _dic.Keys)
                {
                    yield return str;
                }
            }
        }

        public List<string> GetStringList() => _list;

        public EsUniqueStringTable Clone()
        {
            EsUniqueStringTable newClone = new EsUniqueStringTable();
            Dictionary<string, int> cloneDic = newClone._dic;
            cloneDic.Clear();
            foreach (KeyValuePair<string, int> kp in _dic)
            {
                cloneDic.Add(kp.Key, kp.Value);
            }
            newClone._list.Clear();
            newClone._list.AddRange(_list);

            return newClone;
        }
    }
}