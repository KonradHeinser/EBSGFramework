using RimWorld;
using System.Xml;
using Verse;

namespace EBSGFramework
{
    public class ConditionDuration
    {
        public GameConditionDef condition;

        public int ticks = 60000;

        public ConditionDuration() { }

        public ConditionDuration(GameConditionDef condition, int ticks)
        {
            this.condition = condition;
            this.ticks = ticks;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;

            if (xmlRoot.Name != "li")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "condition", xmlRoot.Name);

            if (num == 1)
                LoadFromSingleNode(xmlRoot.FirstChild);
            else if (num > 1)
                LoadMultipleNodes(xmlRoot);
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
                ticks = ParseHelper.FromString<int>(xmlText.InnerText);
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
            if (element.Name == "condition")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "condition", element.InnerText);
            else if (element.Name == "ticks")
                ticks = ParseHelper.FromString<int>(element.InnerText);
        }
    }
}
