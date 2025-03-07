using System.Xml;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class SkillLevel
    {
        public SkillDef skill;

        public int minLevel = 0; // Kept for compatibility

        public int maxLevel = 20; // Kept for compatibility

        public IntRange range = new IntRange(0, 20);

        public IntRange defaultRange = new IntRange(0, 20);

        public SkillLevel() { }

        public SkillLevel(SkillDef skill, IntRange range)
        {
            this.skill = skill;
            this.range = range;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;

            if (xmlRoot.Name != "li")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);

            if (num == 0)
                range = defaultRange;
            else if (num == 1)
                LoadFromSingleNode(xmlRoot.FirstChild);
            else if (num > 1)
                LoadMultipleNodes(xmlRoot);
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
                range = ParseHelper.FromString<IntRange>(xmlText.InnerText);
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
            if (element.Name == "skill")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", element.InnerText);
            if (element.Name == "range")
                range = ParseHelper.FromString<IntRange>(element.InnerText);
            else if (element.Name == "minLevel")
                range.min = ParseHelper.FromString<int>(element.InnerText);
            else if (element.Name == "maxLevel")
                range.max = ParseHelper.FromString<int>(element.InnerText);
        }
    }
}
