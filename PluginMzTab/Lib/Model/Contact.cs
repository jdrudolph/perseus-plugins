using System.Text;

namespace PluginMzTab.Lib.Model{
/**
 * User: Qingwei
 * Date: 23/05/13
 */

    public class Contact : IndexedElement{
        private string name;
        private string email;
        private string affiliation;

        public Contact(int id) : base(MetadataElement.CONTACT, id){}

        public string Name { get { return name; } set { name = value; } }

        public string Email { get { return email; } set { email = value; } }

        public string Affiliation { get { return affiliation; } set { affiliation = value; } }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            if (name != null){
                sb.Append(printProperty(MetadataProperty.CONTACT_NAME, name)).Append(MZTabConstants.NEW_LINE);
            }
            if (affiliation != null){
                sb.Append(printProperty(MetadataProperty.CONTACT_AFFILIATION, affiliation))
                  .Append(MZTabConstants.NEW_LINE);
            }
            if (email != null){
                sb.Append(printProperty(MetadataProperty.CONTACT_EMAIL, email)).Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}