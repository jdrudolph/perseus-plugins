using System.Collections.Generic;

namespace PluginMzTab.Lib.Model{
/**
 * A publication on this unit. PubMed ids must be prefixed by “pubmed:”, DOIs by “doi:”.
 */

    public class Publication : IndexedElement{
        private SplitList<PublicationItem> itemList = new SplitList<PublicationItem>(MZTabConstants.BAR);

        public Publication(int id) : base(MetadataElement.PUBLICATION, id){}

        public void AddPublicationItem(PublicationItem item){
            itemList.Add(item);
        }

        public void AddPublicationItems(IEnumerable<PublicationItem> items){
            itemList.AddRange(items);
        }

        public int Size { get { return itemList.Count; } }

        public SplitList<PublicationItem> Items { get { return itemList; } }

        public override string ToString(){
            return string.Format("{0}{1}", printElement(itemList), MZTabConstants.NEW_LINE);
        }
    }
}