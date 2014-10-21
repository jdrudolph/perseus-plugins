namespace PluginMzTab.Lib.Model{
    /**
     * Comment lines can be placed anywhere in an mzTab file. These lines must start with
     * the three-letter code COM and are ignored by most parsers.
     */

    public class Comment{
        private readonly string msg;

        public Comment(string msg){
            this.msg = msg ?? "";
        }

        public override string ToString(){
            return Section.Comment.Prefix + MZTabConstants.TAB + msg;
        }
    }
}