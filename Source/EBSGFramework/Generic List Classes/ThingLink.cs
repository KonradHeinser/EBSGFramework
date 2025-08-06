using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class ThingLink
    {
        public ThingDef thing;

        public int amount = 1;

        public ThingLink()
        {
        }

        public ThingLink(ThingDef thing, int amount)
        {
            this.thing = thing;
            this.amount = amount;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;
            if (xmlRoot.Name != "li")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", xmlRoot.Name);

            if (num == 0)
                amount = 1;
            else if (num == 1)
                LoadFromSingleNode(xmlRoot.FirstChild);
            else if (num > 1)
                LoadMultipleNodes(xmlRoot);
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
            {
                amount = ParseHelper.FromString<int>(xmlText.InnerText);
            }
            else if (node is XmlElement element)
            {
                ParseXmlElement(element);
            }
        }

        private void LoadMultipleNodes(XmlNode xmlRoot)
        {
            foreach (object childNode in xmlRoot.ChildNodes)
            {
                ParseXmlElement(childNode as XmlElement);
            }
        }

        private void ParseXmlElement(XmlElement element)
        {
            if (element.Name == "amount")
                amount = ParseHelper.FromString<int>(element.InnerText);
            else if (element.Name == "thing")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", element.InnerText);
        }

        public override string ToString()
        {
            return thing.LabelCap + " x" + amount;
        }
    }
}
