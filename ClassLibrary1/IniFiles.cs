using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umpo.Common.IniFileLib
{
    public class IniFiles
    {


        #region приватные переменные

        //private Section[] sections;

        #endregion



        #region публичные свойства

        public Dictionary<string, Section> Sections { get; set; }
        public string IniFile { get; set; }
        #endregion


        #region приватные методы

        private void ReadFile(string file)
        {
            Sections = new Dictionary<string, Section>();

            // прочитать текстовый файл file

            var lines = File.ReadAllLines(file);
            var lastSectionName = string.Empty;

            foreach (var line in lines)
            {

                //если строка не пустая
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var sectionName = string.Empty;


                    if (isLineSection(line, out sectionName))
                    {
                        //видимо это секция

                        var newSection = new Section() { Name = sectionName, Keys = new List<SectionKey>() };
                        Sections.Add(sectionName, newSection);

                        lastSectionName = sectionName;
                    }
                    else
                    {
                        //видимо эта строка не секция, возможно ключ
                        var k = string.Empty;
                        var v = string.Empty;

                        getKeyAndValue(line, out k, out v);

                        var newKey = new SectionKey() { Name = k, Value = v };

                        Sections[lastSectionName].Keys.Add(newKey);
                    }
                }
            }
        }

        public void AddSection(string sectionname)
        {
            if (!Sections.ContainsKey(sectionname.ToLower())) {

                var newSection = new Section() { Name = sectionname.ToLower() };

                Sections.Add(sectionname.ToLower(), newSection);
            }


        }

        public void AddSection(string sectionname, string key, string value)
        {
            if (!Sections.ContainsKey(sectionname.ToLower()))
            {
                var keys = new List<SectionKey>();
                keys.Add(new SectionKey() { Name = key, Value = value });
                var newSection = new Section() { Name = sectionname.ToLower(), Keys = keys };

                Sections.Add(sectionname.ToLower(), newSection);
            }


        }

        public bool Save()
        {
            var sb = new StringBuilder();

            foreach(var sec in Sections){
                sb.AppendFormat("[{0}]", sec.Key);

                if (sec.Value.Keys != null) {
                    foreach (var key in sec.Value.Keys) {
                        sb.AppendFormat("{0}={1}\n\r", key.Name, key.Value);
                    }
                }
            }

            var ret = false;
            try {
                File.WriteAllText(IniFile, sb.ToString());
                ret = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ret;


            
        }

        private bool isLineSection(string line, out string sectionName)
        {
            sectionName = string.Empty;
            var s = line.Trim();

            if (s.StartsWith("[") && s.EndsWith("]")){
                sectionName = s.Substring(1, s.Length - 2);
                return true;
            };

            return false;
        }

        private void getKeyAndValue(string line, out string k, out string v)
        {
            var s = line.Trim();

            var dump = s.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

            k = dump[0].Trim();
            if (dump.Length == 1)
            {
                v = string.Empty;
            }
            else {
                v = dump[1].Trim();
            }


        }

        public void Load()
        {
            ReadFile(IniFile);
        }


        #endregion


        //конструкторы класса
        #region конструкторы класса

        public IniFiles(){
        }


        public IniFiles(string file)
        {
            //запоминаем имя файла
            IniFile = file;
            ReadFile(file);
        }






        #endregion





        //публичные методы
        #region публичные методы
        public bool isSectionExists(string section) {
            //найти есть ли в Sections элемент с именем секции section


            //1 способ
            var item = GetSection(section.ToLower());
         
            return item != null;

            //2 способ
            //foreach (var item in sections)//зависит от регистра
            //{
            //    if (item.Name == section) {
            //        return true;
            //    }
            //}

            //3 способ Linq
            //var item Sections.FirstOrDefault(x => x.Name == section)

            //if (item != null) {
            //    return true;
            //}
        }

        public Section GetSection(string section) {

            if (Sections.ContainsKey(section.ToLower()))
            {
                var item = Sections[section.ToLower()];

                return item;
            }

            return null;
        }

        #endregion



    }
}
