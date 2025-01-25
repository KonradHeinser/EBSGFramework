﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class GenderByAge
    {
        public Gender gender = Gender.None;

        public FloatRange range = new FloatRange(0f, 99999f);

        public FloatRange defaultRange = new FloatRange(0f, 99999f);

        public GenderByAge() { }

        public GenderByAge(Gender gender, FloatRange range)
        {
            this.gender = gender;
            if (range == null)
                this.range = defaultRange;
            else
                this.range = range;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            int num = xmlRoot.ChildNodes.Count;

            switch (xmlRoot.Name)
            {
                case "Male":
                    gender = Gender.Male;
                    break;
                case "Female":
                    gender = Gender.Female;
                    break;
                case "None":
                    gender = Gender.None;
                    break;
                default:
                    Log.Error($"Tried to convert {xmlRoot.Name} into a gender, but couldn't.");
                    break;
            }

            // DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "gender", xmlRoot.Name);
            if (num == 0)
                range = defaultRange;
            else if (num == 1)
                LoadFromSingleNode(xmlRoot.FirstChild);
            else if (num > 1)
                LoadMultipleNodes(xmlRoot);
            Log.Message($"A) {gender} {range}");
        }

        private void LoadFromSingleNode(XmlNode node)
        {
            if (node is XmlText xmlText)
            {
                range = ParseHelper.FromString<FloatRange>(xmlText.InnerText);
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
            if (element.Name == "range")
            {
                range = ParseHelper.FromString<FloatRange>(element.InnerText);
            }
        }
    }
}
