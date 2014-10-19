using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginMzTab.Lib.Model{
    public class PublicationType{
        public static PublicationType PUBMED = new PublicationType("pubmed");
        public static PublicationType DOI = new PublicationType("doi");
        public static IList<PublicationType> All = new[]{PUBMED, DOI};

        private readonly string name;

        private PublicationType(string name){
            this.name = name;
        }

        public string Name { get { return name; } }

        public static PublicationType FindType(string name){
            if (name == null){
                return null;
            }

            PublicationType publicationType;
            try{
                publicationType = All.First(x => x.Name.Equals(name));
            }
            catch (Exception){
                publicationType = null;
            }

            return publicationType;
        }
    }

    public class PublicationItem{
        private readonly PublicationType publicationType;
        private readonly string accession;

        public PublicationItem(PublicationType publicationType, string accession){
            if (publicationType == null){
                throw new NullReferenceException("Publication PublicationType can not set null!");
            }
            if (accession == null){
                throw new NullReferenceException("Publication accession can not empty!");
            }

            this.publicationType = publicationType;
            this.accession = accession;
        }

        public PublicationType PublicationType { get { return publicationType; } }

        public string Accession { get { return accession; } }

        public override string ToString(){
            return string.Format("{0}:{1}", publicationType.Name, accession);
        }

        public static PublicationType FindType(string name){
            return PublicationType.FindType(name);
        }
    }
}