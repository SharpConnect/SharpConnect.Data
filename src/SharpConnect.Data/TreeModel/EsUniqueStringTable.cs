//MIT, 2015-2016 EngineKit and contributors 
using System;
using System.Collections.Generic;

namespace SharpConnect.Data
{

    public class EsUniqueStringTable
    {
        Dictionary<string, int> dic;
        List<string> list;
        public EsUniqueStringTable()
        {
            dic = new Dictionary<string, int>();
            list = new List<string>();
            dic.Add(string.Empty, 0);
            list.Add(string.Empty);
        }

        public int GetStringIndex(string str)
        {

            if (str == null)
            {
                return 0;
            }
            int foundIndex;
            if (dic.TryGetValue(str, out foundIndex))
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
            if (dic.TryGetValue(str, out foundIndex))
            {
                return foundIndex;
            }
            else
            {
                int index = dic.Count;
                dic.Add(str, index);
                list.Add(str);
                return index;
            }
        }
        public bool Contains(string str)
        {
            return dic.ContainsKey(str);
        }
        public int Count
        {
            get
            {
                return dic.Count;
            }
        }
        public string GetString(int index)
        {
            return list[index];
        }
        public IEnumerable<string> WordIter
        {
            get
            {
                foreach (string str in dic.Keys)
                {
                    yield return str;
                }
            }
        }
        public List<string> GetStringList()
        {
            return list;
        }


        public EsUniqueStringTable Clone()
        {
            EsUniqueStringTable newClone = new EsUniqueStringTable();
            Dictionary<string, int> cloneDic = newClone.dic;
            cloneDic.Clear();
            foreach (KeyValuePair<string, int> kp in this.dic)
            {
                cloneDic.Add(kp.Key, kp.Value);
            }
            newClone.list.Clear();
            newClone.list.AddRange(list);

            return newClone;
        }
    }
}