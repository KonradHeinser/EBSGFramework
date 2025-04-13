using Verse;
using RimWorld;
using System.Xml;

namespace EBSGFramework
{
    public class GeneEvent
    {
        public GeneDef gene;

        public HistoryEventDef propagateEvent;

        public GeneEvent() { }

        public GeneEvent(GeneDef gene, HistoryEventDef propagateEvent)
        {
            this.gene = gene;
            this.propagateEvent = propagateEvent;
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
                propagateEvent = ParseHelper.FromString<HistoryEventDef>(xmlText.InnerText);
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
            else if (element.Name == "propagateEvent")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "propagateEvent", element.InnerText);
        }
    }
}
