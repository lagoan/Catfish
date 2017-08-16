﻿using System;
using Catfish.Core.Models.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using Catfish.Core.Models;
using System.Collections.Generic;

namespace Catfish.Tests
{
    [TestClass]
    public class XmlModelTest
    {
        public static string GetSampleDataFilePathName(string fileName)
        {
            var dir = Directory.GetCurrentDirectory();
            dir = dir.Substring(0, dir.IndexOf("Catfish.Test"));
            dir = Path.Combine(dir, "XmlTemplates");
            return Path.Combine(dir, fileName);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var option_filed_types = typeof(OptionsField).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(OptionsField))).ToList();

            var n = option_filed_types.Count;
        }

        [TestMethod]
        public void XPathTest()
        {
            string path = GetSampleDataFilePathName("MetadataSet.xml");
            Assert.IsTrue(File.Exists(path));

            XElement root = XElement.Load(path);
            Assert.IsNotNull(root);

            XmlModel model = new XmlModel(root);

            var name = model.GetName();
            Assert.AreEqual("Dublin Core", name);

            var desc = model.GetDescription("fr");
            Assert.AreEqual("Il sagit du jeu de métadonnées Dublin Core.", desc);

            var vals = model.GetValues();

            var x = 0;
        }

        [TestMethod]
        public void EditTest()
        {
            string path = GetSampleDataFilePathName("MetadataSet.xml");
            Assert.IsTrue(File.Exists(path));

            XElement root = XElement.Load(path);
            Assert.IsNotNull(root);

            XmlModel model = new XmlModel(root);

            var name = "Test Name 123";
            model.SetName(name);
            var name2 = model.GetName();
            Assert.AreEqual(name, name2);

            model.SetValues(new List<string>() { "a", "b", "c" }); //Default lang "en"

            List<string> evals1 = new List<string>() { "value 1", "value 2", "value 3" };
            model.SetValues(evals1); ////Default lang "en". This should replace the first set of values "a", "b" and "c"

            List<string> fvals1 = new List<string>() { "valeur 1", "valeur 2", "valeur 3" };
            model.SetValues(fvals1, "fr");//This should not replace evals1 because this is not the default lang "en"

            IEnumerable<string> evals2 = model.GetValues("en");
            foreach (var v in evals2)
                Assert.IsTrue(evals1.Contains(v));
            foreach (var v in evals1)
                Assert.IsTrue(evals2.Contains(v));

            IEnumerable<string> fvals2 = model.GetValues("fr");
            foreach (var v in fvals2)
                Assert.IsTrue(fvals1.Contains(v));
            foreach (var v in fvals1)
                Assert.IsTrue(fvals2.Contains(v));

        }

        [TestMethod]
        public void MetadataFieldsTest()
        {
            string path = GetSampleDataFilePathName("MetadataSet.xml");
            Assert.IsTrue(File.Exists(path));

            XElement root = XElement.Load(path);
            Assert.IsNotNull(root);

            MetadataSet model = XmlModel.Parse(root) as MetadataSet;
            Assert.IsNotNull(model);

            List<MetadataField> fields = model.Fields;
            Assert.AreEqual(3, fields.Count);

            var field = fields.Where(f => f is TextField).FirstOrDefault();
            Assert.IsNotNull(field);
            Assert.AreEqual("Subject", field.GetName("en"));
            Assert.AreEqual("Sujet", field.GetName("fr"));
            Assert.AreEqual("Enter the subject here.", field.GetDescription("en"));

            field = fields.Where(f => f is TextArea).FirstOrDefault();
            Assert.IsNotNull(field);
            Assert.AreEqual("Description", field.GetName("en"));
            Assert.AreEqual("La description", field.GetName("fr"));
            Assert.AreEqual("Enter the description here.", field.GetDescription("en"));

            field = fields.Where(f => f is RadioButtonSet).FirstOrDefault();
            Assert.IsNotNull(field);
            Assert.AreEqual("Radio button set 1", field.GetName("en"));
            Assert.AreEqual("Réglage du bouton radio 1", field.GetName("fr"));
            Assert.AreEqual("This is a radio button set.", field.GetDescription("en"));

            var options = (field as RadioButtonSet).GetOptions("fr");
            Assert.AreEqual("Première option\nDeuxième option\nTroisième option", options);
            Assert.AreEqual("First option\nSecond option\nThird option", (field as RadioButtonSet).Options);

        }

        [TestMethod]
        public void ItemTest()
        {
            string path = GetSampleDataFilePathName("Item.xml");
            Assert.IsTrue(File.Exists(path));

            Item model = XmlModel.Load(path) as Item;
            Assert.IsNotNull(model);

            List<MetadataSet> metadatasets = model.MetadataSets.ToList();
            Assert.AreEqual(2, metadatasets.Count());

        }
    }
}