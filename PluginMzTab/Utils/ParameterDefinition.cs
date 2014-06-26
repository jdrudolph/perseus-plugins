using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PluginMzTab.utils
{
    public class ParameterDefinition
    {
        private static readonly Regex Regex = new Regex(@"([a-zA-Z]+)[\[0-9\-n\]]*\-([a-zA-Z]+)");

        private string Identifier { get; set; }
        private string Description { get; set; }
        private string Type { get; set; }
        private string Multiplicity { get; set; }
        private string Example { get; set; }
        private string Section { get; set; }

        public override string ToString()
        {
            return Identifier + "|" + Section;
        }

        public static ParameterDefinition Parse(StreamReader reader)
        {
            string line;
            ParameterDefinition parameterDefinition = new ParameterDefinition();
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line))
                {
                    return parameterDefinition;
                }
                int index = line.IndexOf(":", StringComparison.Ordinal);
                string item = line.Substring(index + 1).Trim();

                if (line.StartsWith("name"))
                {
                    parameterDefinition.Identifier = item;
                }
                else if (line.StartsWith("def"))
                {
                    parameterDefinition.Description = item;
                }
                else if (line.StartsWith("type"))
                {
                    parameterDefinition.Type = item;
                }
                else if (line.StartsWith("section"))
                {
                    parameterDefinition.Section = item;
                }
                else if (line.StartsWith("multiplicity"))
                {
                    parameterDefinition.Multiplicity = item;
                }
                else if (line.StartsWith("example"))
                {
                    if (string.IsNullOrEmpty(parameterDefinition.Example))
                    {
                        parameterDefinition.Example = item;
                    }
                    else
                    {
                        parameterDefinition.Example += "\n" + item;
                    }
                }
            }

            return null;
        }

        public bool Match(string section, string groupName, string name)
        {
            if (!Section.Equals(section))
            {
                return false;
            }
            if (!Regex.IsMatch(Identifier) && groupName.Equals(name))
            {
                return name.Equals(Identifier);
            }
            if (!groupName.StartsWith(Regex.Match(Identifier).Groups[1].Value))
            {
                return false;
            }
            if (!Regex.Match(Identifier).Groups[2].Value.Equals(name))
            {
                return false;
            }

            return true;
        }
    }
}
