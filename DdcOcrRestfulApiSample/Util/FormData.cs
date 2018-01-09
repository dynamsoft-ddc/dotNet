using System.Collections.Generic;

namespace DdcOcrRestfulApiSample.Util
{
    public class FormData
    {
        private readonly List<KeyValuePair<string, KeyValuePair<object, string>>> _listFormData;

        public FormData()
        {
            _listFormData = new List<KeyValuePair<string, KeyValuePair<object, string>>>();
        }

        public void Append(string strKey, object value)
        {
            Append(strKey, value, null);
        }

        public void Append(string strKey, object value, string strFileName)
        {
            var dataItem = new KeyValuePair<string, KeyValuePair<object, string>>(strKey,
                new KeyValuePair<object, string>(value, strFileName));

            _listFormData.Add(dataItem);
        }

        public void Clear()
        {
            _listFormData.Clear();
        }

        public bool IsValid()
        {
            return _listFormData != null;
        }

        public List<KeyValuePair<string, KeyValuePair<object, string>>> GetAll()
        {
            return _listFormData;
        }
    }
}
