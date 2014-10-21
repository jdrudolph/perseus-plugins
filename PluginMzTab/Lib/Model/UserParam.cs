namespace PluginMzTab.Lib.Model{
/**
 * user parameters that only contain a name and a value.
 */

    public class UserParam : Param{
        public UserParam(string name, string value)
            : base(name, value){}

        public new string Name { get { return name; } }

        public new string Value { get { return value; } }
    }
}