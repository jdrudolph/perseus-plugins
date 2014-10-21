namespace PluginMzTab.Lib.Model{
/**
 * User: Qingwei
 * Date: 13/02/13
 */

    public class ModificationList{
        private readonly SplitList<Modification> modList = new SplitList<Modification>(MZTabConstants.COMMA);

        public void Add(Modification modification){
            modList.Add(modification);
        }

        public override string ToString(){
            return modList.ToString();
        }
    }
}