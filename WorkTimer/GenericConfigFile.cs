using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WorkTimer
{
    class GenericConfigFile
    {
        private Dictionary<string, string> _keyValues;

        private GenericConfigFile()
        {
            _keyValues = new Dictionary<string, string>();
        }

        public bool TryGetString(string key, out string s)
        {
            s = "";
            if (_keyValues.ContainsKey(key))
            {
                s = _keyValues[key];
                return true;
            }
            return false;
        }

        public bool TryGetInt(string key, out int i)
        {
            i = 0;
            if (_keyValues.ContainsKey(key))
            {
                return int.TryParse(_keyValues[key], out i);
            }
            return false;
        }

        public bool TryGetFloat(string key, out float f)
        {
            f = 0.0f;
            if (_keyValues.ContainsKey(key))
            {   
                return float.TryParse(_keyValues[key], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out f);
            }
            return false;
        }

        private void Add(string key, string value)
        {
            _keyValues.Add(key, value);
        }

        public static GenericConfigFile Load(string path)
        {
            if(File.Exists(path))
            {
                char[] split = { '=' };
                GenericConfigFile file = new GenericConfigFile();
                var lines = File.ReadAllLines(path);

                for (int i = 0; i < lines.Length; i++)
                {
                    var curLine = lines[i].Trim();
                    var keyAndValue = curLine.Split(split);

                    var key = keyAndValue[0].Trim();
                    var value = keyAndValue[1].Trim();

                    file.Add(key, value);
                }

                return file;
            }

            return null;
        }
    }
}
