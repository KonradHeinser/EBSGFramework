using System.Xml;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class SkillXP
    {
        public SkillDef skill;

        public int experience;

        public SkillXP() { }

        public SkillXP(SkillDef skill, int experience)
        {
            this.skill = skill;
            this.experience = experience;
        }
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;

            if (xmlRoot.Name != "Random" && xmlRoot.Name != "li")
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
            else
                skill = null;

            if (num == 1)
                LoadFromSingleNode(xmlRoot.FirstChild);
            else if (num > 1)
                LoadMultipleNodes(xmlRoot);
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
                experience = ParseHelper.FromString<int>(xmlText.InnerText);
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
            else if (element.Name == "experience")
                experience = ParseHelper.FromString<int>(element.InnerText);
        }
    }
}
