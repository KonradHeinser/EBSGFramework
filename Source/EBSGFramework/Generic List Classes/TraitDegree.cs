using RimWorld;
using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class TraitDegree
    {
        public TraitDef def;
        
        public int degree;

        public TraitDegree(){}
        
        public TraitDegree(TraitDef def, int degree)
        {
            this.def = def;
            this.degree = degree;
        }
        
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name);

            if (num == 0)
                degree = 0;
            else if (num == 1)
                LoadFromSingleNode(xmlRoot.FirstChild);
            else if (num > 1)
                LoadMultipleNodes(xmlRoot);
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
                degree = ParseHelper.FromString<int>(xmlText.InnerText);
            else if (node is XmlElement element)
                ParseXmlElement(element);
        }

        private void LoadMultipleNodes(XmlNode xmlRoot)
        {
            foreach (object childNode in xmlRoot.ChildNodes)
                ParseXmlElement(childNode as XmlElement);
        }

        private void ParseXmlElement(XmlElement element)
        {
            if (element.Name == "degree")
                degree = ParseHelper.FromString<int>(element.InnerText);
            else if (element.Name == "def")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", element.InnerText);
        }
    }
}