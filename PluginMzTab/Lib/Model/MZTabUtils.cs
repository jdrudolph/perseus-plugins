using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
    public static class MZTabUtils{
        /**
         * Pre-process the string object. If object is null, return null; otherwise
         * remove heading and tailing white space.
         */

        public static string ParseString(string target){
            return target == null ? null : target.Trim();
        }

        /**
         * If ratios are included and the denominator is zero, the “INF” value MUST be used.
         * If the result leads to calculation errors (for example 0/0), this MUST be reported
         * as “not a number” (“NaN”).
         *
         * @see #parseDouble(string)
         */

        public static string PrintDouble(double value){
            if (double.IsNaN(value)){
                return MZTabConstants.CALCULATE_ERROR;
            }
            return double.IsPositiveInfinity(value)
                       ? MZTabConstants.INFINITY
                       : value.ToString(CultureInfo.InvariantCulture);
        }


        public static string ParseEmail(string target){
            target = ParseString(target);
            if (target == null){
                return null;
            }

            const string regexp = "^\\s*\\w+(?:\\.{0,1}[\\w-]+)*@[a-zA-Z0-9]+(?:[-.][a-zA-Z0-9]+)*\\.[a-zA-Z]+\\s*$";
            Regex pattern = new Regex(regexp);

            return pattern.IsMatch(target) ? target : null;
        }

        /**
         * Parameters are always reported as [CV label, accession, name, value].
         * Any field that is not available MUST be left empty.
         *
         * Notice: name cell never set null.
         */

        public static Param ParseParam(string target){
            target = ParseString(target);
            if (target == null){
                return null;
            }
            if (target.Contains("\"")){
                return parseComplexParam(target);
            }
            Regex regex = new Regex("\\[([^,]+)?,([^,]+)?,([^,]+),([^,]*)\\]");

            if (regex.IsMatch(target) && regex.Match(target).Length == target.Length){
                string cvLabel = regex.Match(target).Groups[1].Value.Trim();
                string accession = regex.Match(target).Groups[2].Value.Trim();
                string name = regex.Match(target).Groups[3].Value.Trim();
                string value = regex.Match(target).Groups[4].Value.Trim();

                if (string.IsNullOrEmpty(name)){
                    return null;
                }

                if (string.IsNullOrEmpty(cvLabel) && string.IsNullOrEmpty(accession)){
                    return new UserParam(name, value);
                }
                return new CVParam(cvLabel, accession, name, value);
            }
            return null;
        }

        /**
         * If the name of param contains comma, bracket, quotes MUST be added to avoid problems.
         */

        public static Param parseComplexParam(string target){
            const string regexp = "\\[([^,]+)?,([^,]+)?,(.+),([^,]*)\\]";
            Regex pattern = new Regex(regexp);

            Match match = pattern.Match(target);

            if (match.Length == target.Length){
                string cvLabel = match.Groups[1].Value;
                string accession = match.Groups[2].Value;
                string name = match.Groups[3].Value;
                string value = match.Groups[4].Value;

                int start = name.IndexOf("\"", StringComparison.Ordinal) + 1;
                int end = name.LastIndexOf("\"", StringComparison.Ordinal);
                name = name.Substring(start, end - start);
                if (string.IsNullOrEmpty(name)){
                    return null;
                }

                if (string.IsNullOrEmpty(cvLabel) && string.IsNullOrEmpty(accession)){
                    return new UserParam(name, value);
                }
                return new CVParam(cvLabel, accession, name, value);
            }
            return null;
        }

        /**
         * Multiple identifiers MUST be separated by splitChar.
         */

        public static SplitList<string> ParseStringList(char splitChar, string target){
            SplitList<string> list = new SplitList<string>(splitChar);

            target = ParseString(target);
            if (target == null){
                return list;
            }

            // regular express reserved keywords escape            
            IList<char> chars = new List<char>();
            switch (splitChar){
                case '.':
                case '$':
                case '^':
                case '{':
                case '}':
                case '[':
                case ']':
                case '(':
                case ')':
                case '|':
                case '*':
                case '+':
                case '?':
                case '\\':
                    chars.Add(splitChar);
                    break;
                default:
                    chars.Add(splitChar);
                    break;
            }

            string[] items = target.Split(chars.ToArray());
            list.AddRange(items);

            return list;
        }

        public static IndexedElement ParseIndexedElement(string target, MetadataElement element){
            target = ParseString(target);
            if (target == null){
                return null;
            }
            Regex regex = new Regex(element + "\\[(\\d+)\\]");
            if (regex.IsMatch(target)){
                int id = int.Parse(regex.Match(target).Groups[1].Value);
                return new IndexedElement(element, id);
            }
            return null;
        }

        public static List<IndexedElement> ParseIndexedElementList(string target, MetadataElement element){
            SplitList<string> list = ParseStringList(MZTabConstants.COMMA, target);

            List<IndexedElement> indexedElementList = new List<IndexedElement>();
            foreach (string item in list){
                IndexedElement indexedElement = ParseIndexedElement(item, element);
                if (indexedElement == null){
                    indexedElementList.Clear();
                    return indexedElementList;
                }
                indexedElementList.Add(indexedElement);
            }
            return indexedElementList;
        }

        /**
         * A list of '|' separated parameters
         */

        public static SplitList<Param> ParseParamList(string target){
            SplitList<string> list = ParseStringList(MZTabConstants.BAR, target);

            SplitList<Param> paramList = new SplitList<Param>(MZTabConstants.BAR);
            foreach (string item in list){
                Param param = ParseParam(item);
                if (param == null){
                    paramList.Clear();
                    return paramList;
                }
                paramList.Add(param);
            }

            return paramList;
        }

        /**
         * A '|' delimited list of GO accessions
         */

        public static SplitList<string> ParseGoTermList(string target){
            SplitList<string> list = ParseStringList(MZTabConstants.COMMA, target);

            SplitList<string> goList = new SplitList<string>(MZTabConstants.COMMA);
            foreach (string listitem in list){
                var item = ParseString(listitem);
                if (item.StartsWith("GO:")){
                    goList.Add(item);
                }
                else{
                    goList.Clear();
                    break;
                }
            }

            return goList;
        }

        public static Integer ParseInteger(string target){
            target = ParseString(target);
            if (target == null){
                return null;
            }

            Integer integer;

            try{
                integer = Integer.Parse(target);
            }
            catch (FormatException){
                return null;
            }

            return integer;
        }

        public static double ParseDouble(string target){
            target = ParseString(target);
            if (target == null){
                return double.NaN;
            }

            double value;
            try{
                value = double.Parse(target);
            }
            catch (FormatException){
                if (target.Equals(MZTabConstants.CALCULATE_ERROR)){
                    value = double.NaN;
                }
                else if (target.Equals(MZTabConstants.INFINITY)){
                    value = double.PositiveInfinity;
                }
                else{
                    return double.NaN;
                }
            }

            return value;
        }

        public static SplitList<double> ParseDoubleList(string target){
            SplitList<string> list = ParseStringList(MZTabConstants.BAR, target);


            SplitList<double> valueList = new SplitList<double>(MZTabConstants.BAR);
            foreach (string listItem in list){
                double value = ParseDouble(listItem);
                if (value.Equals(double.MinValue)){
                    valueList.Clear();
                    break;
                }
                valueList.Add(value);
            }

            return valueList;
        }

        /**
         * UNIT_IDs MUST only contain the following characters: 'A'-'Z', 'a'-'z', '0'-'9', and '_'.
         * UNIT_IDs SHOULD consist of the resource identifier plus the resources internal unit id.
         * A resource is anything that is generating mzTab files.
         */

        public static string ParseUnitId(string target){
            target = ParseString(target);
            if (target == null){
                return null;
            }
            Regex regex = new Regex("[A-Za-z_][A-Za-z0-9_]*");

            if (regex.IsMatch(target) && regex.Match(target).Index == 0 && regex.Match(target).Length == target.Length){
                return target;
            }
            return null;
        }

        public static Url ParseUrl(string target){
            target = ParseString(target);
            if (target == null){
                return null;
            }

            Url url;

            try{
                url = new Url(target);
            }
            catch (ArgumentNullException){
                url = null;
            }

            return url;
        }

        public static Uri ParseURI(string target){
            target = ParseString(target);
            if (target == null){
                return null;
            }

            Uri uri;

            try{
                uri = new Uri(target);
            }
            catch (UriFormatException){
                uri = null;
            }

            return uri;
        }

        /**
         * A publication on this unit. PubMed ids must be prefixed by “pubmed:”,
         * DOIs by “doi:”. Multiple identifiers MUST be separated by “|”.
         */

        public static SplitList<PublicationItem> ParsePublicationItems(string target){
            SplitList<string> list = ParseStringList(MZTabConstants.BAR, target);

            SplitList<PublicationItem> itemList = new SplitList<PublicationItem>(MZTabConstants.BAR);
            foreach (string pubList in list){
                var pub = ParseString(pubList);
                if (pub == null){
                    itemList.Clear();
                    break;
                }
                string[] items = pub.Split(new[]{"" + MZTabConstants.COLON}, StringSplitOptions.None);
                PublicationType type;
                if (items.Length != 2 || (type = PublicationItem.FindType(items[0].Trim())) == null){
                    itemList.Clear();
                    break;
                }
                string accession = items[1].Trim();
                PublicationItem item = new PublicationItem(type, accession);
                itemList.Add(item);
            }

            return itemList;
        }

        public static SplitList<SpectraRef> ParseSpectraRefList(Metadata metadata, string target){
            SplitList<string> list = ParseStringList(MZTabConstants.BAR, target);
            SplitList<SpectraRef> refList = new SplitList<SpectraRef>(MZTabConstants.BAR);

            Regex regex = new Regex("ms_run\\[(\\d+)\\]:(.*)");

            foreach (string item in list){
                if (regex.IsMatch(item.Trim())){
                    int ms_file_id = int.Parse(regex.Match(item.Trim()).Groups[1].Value);
                    string reference = regex.Match(item.Trim()).Groups[2].Value;

                    MsRun msRun = metadata.MsRunMap[ms_file_id];
                    SpectraRef sref = msRun == null ? null : new SpectraRef(msRun, reference);

                    if (sref == null){
                        refList.Clear();
                        break;
                    }
                    refList.Add(sref);
                }
            }

            return refList;
        }

        private static void parsePosition(string target, Modification modification){
            target = translateTabToComma(target);
            SplitList<string> list = ParseStringList(MZTabConstants.BAR, target);

            Regex regex = new Regex("(\\d+)(\\[([^,]+)?,([^,]+)?,([^,]+),([^,]*)\\])?");

            foreach (string item in list){
                if (regex.IsMatch(item.Trim())){
                    int id = int.Parse(regex.Match(item.Trim()).Groups[1].Value);
                    CVParam param = string.IsNullOrEmpty(regex.Match(item.Trim()).Groups[5].Value)
                                        ? null
                                        : new CVParam(regex.Match(item.Trim()).Groups[3].Value,
                                                      regex.Match(item.Trim()).Groups[4].Value,
                                                      regex.Match(item.Trim()).Groups[5].Value,
                                                      regex.Match(item.Trim()).Groups[6].Value);
                    modification.AddPosition(id, param);
                }
            }
        }

        /**
         *  solve the conflict about minus char between modification position and CHEMMOD charge.
         *  For example: 13-CHEMMOD:-159
         */

        private static string translateMinusToUnicode(string target){
            Regex regex = new Regex("(CHEMMOD:.*)(-)(.*)");

            if (regex.IsMatch(target)){
                StringBuilder sb = new StringBuilder();

                sb.Append(regex.Match(target).Groups[1].Value);
                sb.Append("&minus;");
                sb.Append(regex.Match(target).Groups[3].Value);

                return sb.ToString();
            }
            return target;
        }

        private static string translateUnicodeToMinus(string target){
            Regex regex = new Regex("(.*CHEMMOD:.*)(&minus;)(.*)");

            if (regex.IsMatch(target)){
                StringBuilder sb = new StringBuilder();

                sb.Append(regex.Match(target).Groups[1].Value);
                sb.Append("-");
                sb.Append(regex.Match(target).Groups[3].Value);

                return sb.ToString();
            }
            return target;
        }

        private static Modification ParseModification(Section section, string target){
            target = ParseString(target);
            // no modification
            if (target.Equals("0")){
                return Modification.CreateNoModification(section);
            }
            target = translateMinusToUnicode(target);
            if (target == null){
                return null;
            }

            target = translateTabToComma(target);
            string[] items = target.Split("\\-".ToCharArray());
            string modLabel;
            string positionLabel;
            if (items.Length > 2){
                // error
                return null;
            }
            if (items.Length == 2){
                positionLabel = items[0];
                modLabel = items[1];
            }
            else{
                positionLabel = null;
                modLabel = items[0];
            }

            Modification modification = null;

            modLabel = translateUnicodeToMinus(modLabel);
            Regex regex = new Regex("(MOD|UNIMOD|CHEMMOD|SUBST):([^\\|]+)(\\|\\[([^,]+)?,([^,]+)?,([^,]+),([^,]*)\\])?");

            if (regex.IsMatch(modLabel)){
                Modification.ModificationType type = Modification.FindType(regex.Match(target).Groups[1].Value);
                string accession = regex.Match(target).Groups[2].Value;
                modification = new Modification(section, type, accession);
                if (positionLabel != null){
                    parsePosition(positionLabel, modification);
                }

                CVParam neutralLoss = string.IsNullOrEmpty(regex.Match(target).Groups[6].Value)
                                          ? null
                                          : new CVParam(regex.Match(target).Groups[4].Value,
                                                        regex.Match(target).Groups[5].Value,
                                                        regex.Match(target).Groups[6].Value,
                                                        regex.Match(target).Groups[7].Value);
                modification.NeutralLoss = neutralLoss;
            }

            return modification;
        }

        /**
         * locate param label [label, accession, name, value], translate ',' to '\t'
         * which used to identified
         */

        private static string translateCommaToTab(string target){
            Regex regex = new Regex("\\[([^\\[\\]]+)\\]");


            StringBuilder sb = new StringBuilder();

            int start = 0;
            while (regex.IsMatch(target, start)){
                sb.Append(target.Substring(start, regex.Match(target, start).Length));
                sb.Append(regex.Match(target, start).Groups[1].Value.Replace(",", "\t"));
                start += regex.Match(target, start).Length;
            }
            sb.Append(target.Substring(start, target.Length - start));

            return sb.ToString();
        }

        /**
         * solve the conflict about comma char which used in split modification and split cv param components.
         */

        private static string translateTabToComma(string target){
            Regex regex = new Regex("\\[([^\\[\\]]+)\\]");


            StringBuilder sb = new StringBuilder();

            int start = 0;
            while (regex.IsMatch(target, start)){
                sb.Append(target.Substring(start, regex.Match(target, start).Length));
                sb.Append(regex.Match(target, start).Groups[1].Value.Replace("\t", ","));
                start += regex.Match(target, start).Length;
            }
            sb.Append(target.Substring(start, target.Length - start));

            return sb.ToString();
        }

        public static SplitList<Modification> ParseModificationList(Section section, string target){
            target = ParseString(target);
            if (target == null){
                return null;
            }

            SplitList<Modification> modList = new SplitList<Modification>(MZTabConstants.COMMA);
            if (target.Equals("0")){
                modList.Add(Modification.CreateNoModification(section));
                return modList;
            }

            target = translateCommaToTab(target);

            SplitList<string> list = ParseStringList(MZTabConstants.COMMA, target);

            foreach (string item in list){
                Modification mod = ParseModification(section, item.Trim());
                if (mod == null){
                    modList.Clear();
                    break;
                }
                modList.Add(mod);
            }

            return modList;
        }
    }
}