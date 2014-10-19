using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

/**
 * mzTab makes use of CV parameters. As mzTab is expected to be used in several experimental environments
 * where parameters might not yet be available for the generated scores etc. all parameters can either
 * report CV parameters or user parameters that only contain a name and a value.
 * Parameters are always reported as [CV label, accession, name, value].
 * Any field that is not available MUST be left empty.
 *
 */

namespace PluginMzTab.Lib.Model{
    public class Param{
        private const string CV_PARAM = "CV Param";
        private const string USER_PARAM = "User Param";

        protected readonly string cvLabel;
        protected readonly string accession;
        protected readonly string name;
        protected string value;

        protected Param(string cvLabel, string accession, string name, string value){
            if (name == null || name.Trim().Length == 0){
                throw new DataException(CV_PARAM + "'s name can not set empty!");
            }

            this.cvLabel = cvLabel == null ? null : cvLabel.Trim();
            this.accession = accession == null ? null : accession.Trim();
            this.name = name.Trim();
            this.value = value == null ? null : value.Trim();
        }

        protected Param(string name, string value){
            if (name == null || name.Trim().Length == 0){
                throw new DataException(USER_PARAM + "'s name can not set empty!");
            }

            this.name = name;
            this.value = value;
        }

        public string CvLabel { get { return cvLabel; } }

        public string Accession { get { return accession; } }

        public string Name { get { return name; } }

        public string Value { get { return value; } set { this.value = value; } }

        public override bool Equals(Object o){
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            Param param = (Param) o;

            if (accession != null ? !accession.Equals(param.accession) : param.accession != null) return false;

            return true;
        }

        public override int GetHashCode(){
            return accession != null ? accession.GetHashCode() : 0;
        }

        private void printName(string name, StringBuilder sb){
            List<string> charList = new List<string>();
            charList.Add("\"");
            charList.Add(",");
            charList.Add("[");
            charList.Add("]");

            bool containReserveChar = false;
            foreach (string c in charList){
                if (name.Contains(c)){
                    containReserveChar = true;
                    break;
                }
            }

            if (containReserveChar){
                sb.Append("\"").Append(name).Append("\"");
            }
            else{
                sb.Append(name);
            }
        }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            sb.Append("[");

            if (cvLabel != null){
                sb.Append(cvLabel);
            }
            sb.Append(", ");

            if (accession != null){
                sb.Append(accession);
            }
            sb.Append(", ");

            printName(name, sb);
            sb.Append(", ");

            if (value != null){
                sb.Append(value);
            }

            sb.Append("]");

            return sb.ToString();
        }

        public static bool TryParse(string strValue, out Param param){
            try{
                param = Parse(strValue);
            }
            catch (Exception){
                param = null;
                return false;
            }
            return true;
        }

        public static Param Parse(string strValue){
            Regex regex = new Regex(@"\[(.*),(.*),(.*),(.*)\]");
            if (!regex.IsMatch(strValue)){
                throw new Exception("Could not parse Param " + strValue);
            }

            var match = regex.Match(strValue);
            string cvLabel = match.Groups[1].Value;
            string accession = match.Groups[2].Value;
            string name = match.Groups[3].Value;
            string value = match.Groups[4].Value;

            if (string.IsNullOrEmpty(cvLabel)){
                return new UserParam(name, value);
            }
            return new CVParam(cvLabel, accession, name, value);
        }
    }
}