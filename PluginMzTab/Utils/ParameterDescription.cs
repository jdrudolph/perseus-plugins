using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BaseLib.Util;
using MzTabLibrary.model;

namespace PluginMzTab.utils{
    internal class FieldType{
        private readonly IList<MzTabType> _types = new List<MzTabType>();
        private readonly IList<MzTabMode> _modes = new List<MzTabMode>();
        private readonly IList<string> _results = new List<string>();

        public void Add(MzTabType type, MzTabMode mode, string result){
            _types.Add(type);
            _modes.Add(mode);
            _results.Add(result);
        }

        public string GetFieldType(MzTabType type, MzTabMode mode)
        {
            int[] i = ArrayUtils.IndicesOf(_types, type);
            if (i.Length == 0)
            {
                return null;
            }

            var temp1 = ArrayUtils.SubArray(_modes, i);
            var temp2 = ArrayUtils.SubArray(_results, i);

            i = ArrayUtils.IndicesOf(temp1, mode);
            if (i.Length == 0)
            {
                return null;
            }

            temp2 = ArrayUtils.SubArray(temp2, i);

            return temp2.FirstOrDefault();
        }

        public bool IsMandatory(MzTabType type, MzTabMode mode){
            return GetFieldType(type, mode).Equals("mandatory", StringComparison.CurrentCultureIgnoreCase);
        }
    }

    public class ParameterDescription{
        public string Name { get; private set; }
        public Section Section { get; private set; }
        public string Definition { get; private set; }
        public string Multiplicity { get; private set; }
        public IList<string> Example { get; private set; }
        private readonly FieldType _fieldType;
        
        private string _short;

        public string GetFieldType(MzTabType type, MzTabMode mode){
            return _fieldType.GetFieldType(type, mode);
        }

        private ParameterDescription(string name, Section section, string definition, string multiplicity,
                                     IList<string> example, FieldType fieldType){
            Name = name;
            Section = section;
            Definition = definition;
            Multiplicity = multiplicity;
            Example = example;
            _fieldType = fieldType;
        }

        private string Short{
            get{
                if (string.IsNullOrEmpty(_short)){
                    _short = Shorten(Name);
                }
                return _short;
            }
        }

        public bool Match(string name){
            return Short.Equals(Shorten(name));
        }

        private void ToHtml(StringBuilder builder){
            builder.Append(string.Format("<h2>name: <b>{0}</b></h2>", Name));
            builder.Append(string.Format("<p>definition: {0}</p>", Definition));
            builder.Append(string.Format("<p>example: <i><font face=\"Courier New\">{0}</font></i></p>", StringUtils.Concat("<br>", Example)));
            builder.Append("<br>");
            builder.Append("<br>");
        }

        private void ToPlainText(StringBuilder builder){
            builder.Append("name:\t" + Name + "\n");
            builder.Append("definition:\t" + Definition + "\n");
            builder.Append("example:\t" + StringUtils.Concat("\n", Example));
            builder.Append("\n");
            builder.Append("\n");
        }

        private void ToRichText(StringBuilder builder){
            builder.Append(@"\ul name:\ul0 \t \b " + Name + "\b0\n");
            builder.Append(string.Format(@"\ul name:\ul0 \t \b {0}\0\n", Name));
            builder.Append(string.Format(@"\ul definition:\ul0 \t {0}\n", Definition));
            builder.Append(string.Format(@"\ul example:\ul0 \t \i {0}\i0", StringUtils.Concat("\n", Example)));
            builder.Append("\n");
            builder.Append("\n");
            builder.Append("}");
        }

        public static string Shorten(string name){
            string shortName = name;
            if (shortName.Contains("[")){
                shortName = shortName.Substring(0, shortName.IndexOf("[", StringComparison.Ordinal));
            }
            return shortName;
        }

        public static IList<ParameterDescription> Read(){
            const string filename = "definition.txt";
            try{
                string configPath = Path.Combine(FileUtils.GetConfigPath(), "mztab");
                if (Directory.Exists(configPath)){
                    string file = Path.Combine(configPath, filename);
                    if (File.Exists(file)){
                        return Read(file);
                    }
                }
            }
            catch (Exception){
                Console.Error.WriteLine("Can not read " + filename);
            }
            return null;
        }

        private static IList<ParameterDescription> Read(string filename){
            StreamReader reader = new StreamReader(File.OpenRead(filename));
            return Read(reader);
        }

        private static IList<ParameterDescription> Read(StreamReader reader){
            string line;
            IList<ParameterDescription> result = new List<ParameterDescription>();

            string name = null;
            Section section = null;
            string definition = null;
            string multiplicity = null;
            FieldType fieldType = new FieldType();
            List<string> example = new List<string>();
            string key = null;
            while ((line = reader.ReadLine()) != null){
                if (string.IsNullOrEmpty(line)){
                    if (!string.IsNullOrEmpty(name) && section != null && !string.IsNullOrEmpty(definition)){
                        ParameterDescription parameterDescription = new ParameterDescription(name, section, definition,
                                                                                             multiplicity, example,
                                                                                             fieldType);
                        result.Add(parameterDescription);
                    }
                    name = null;
                    section = null;
                    definition = null;
                    multiplicity = null;
                    example = new List<string>();
                    fieldType = new FieldType();
                    continue;
                }
                string value;
                if (line.Contains(":")){
                    key = line.Substring(0, line.IndexOf(':'));
                    value = line.Substring(line.IndexOf(':') + 1).Trim();
                }
                else{
                    value = line;
                }
                if (key == null){
                    continue;
                }

                if (key.StartsWith("name")){
                    name += value.Trim();
                }
                else if (key.StartsWith("section")){
                    section = Section.findSection(value.Trim());
                }
                else if (key.StartsWith("def")){
                    definition += value.Trim();
                }
                else if (key.StartsWith("multiplicity")){
                    multiplicity += value.Trim();
                }
                else if (key.StartsWith("example")){
                    example.Add(value.Trim());
                }
                else if (key.StartsWith("quantification")){
                    string[] split = value.Split('|');
                    if (split.Length == 2){
                        fieldType.Add(MzTabType.Quantification, MzTabMode.Summary, split[0]);
                        fieldType.Add(MzTabType.Quantification, MzTabMode.Complete, split[1]);
                    }
                }
                else if (key.StartsWith("identification")){
                    string[] split = value.Split('|');
                    if (split.Length == 2){
                        fieldType.Add(MzTabType.Identification, MzTabMode.Summary, split[0]);
                        fieldType.Add(MzTabType.Identification, MzTabMode.Complete, split[1]);
                    }
                }
            }

            return result;
        }

        public static string GetText(DocumentType helpType, Section section){
            IList<ParameterDescription> list = Read();
            if (list == null){
                return null;
            }
            list = list.Where(x => x.Section.Name == section.Name).ToList();

            StringBuilder builder = new StringBuilder();
            if (helpType == DocumentType.Rtf){
                builder.Append("{\rtf1\ansi");
            }
            foreach (ParameterDescription descripition in list){
                switch (helpType){
                    case DocumentType.Html:
                        descripition.ToHtml(builder);
                        break;
                    case DocumentType.PlainText:
                        descripition.ToPlainText(builder);
                        break;
                    case DocumentType.Rtf:
                        descripition.ToRichText(builder);
                        break;
                }
            }
            if (helpType == DocumentType.Rtf){
                builder.Append("}");
            }

            return builder.ToString();
        }
    }
}