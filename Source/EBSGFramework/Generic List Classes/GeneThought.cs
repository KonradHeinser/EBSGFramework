using System.Xml;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class GeneThought
    {
        public GeneDef gene;

        public ThoughtDef thought;

        public GeneThought() { }

        public GeneThought(GeneDef gene, ThoughtDef thought)
        {
            this.gene = gene;
            this.thought = thought;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;

            if (xmlRoot.Name != "li")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "gene", xmlRoot.Name);

            if (num == 1)
                LoadFromSingleNode(xmlRoot.FirstChild);
            else if (num > 1)
                LoadMultipleNodes(xmlRoot);
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thought", xmlText.InnerText);
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
            if (element.Name == "gene")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "gene", element.InnerText);
            else if (element.Name == "thought")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thought", element.InnerText);
        }
    }
}
