﻿using Catfish.Core.Models;
using Catfish.Core.Models.Forms;
using Catfish.Core.Services;
using Catfish.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catfish.Areas.Manager.Services;
using System.Configuration;

namespace Catfish.Tests.Services
{
    [TestClass]
    public class PageServiceTest
    {
        private DatabaseHelper mDh { get; set; }
        private PageService pageService;

        [TestInitialize]
        public void InitializeTesting()
        {
            mDh = new DatabaseHelper(true);
            pageService = new PageService();
            var p = mDh.PDb;
        }

        [TestMethod]
        public void CanGetPageByGuid()
        {
            Piranha.Models.Page page = pageService.GetAPage("7849b6d6-dc43-43f6-8b5a-5770ab89fbcf"); 
             Assert.AreEqual("Start", page.Title);
        }

        [TestMethod]
        public void CanGetPageByPermalink()
        {
            Piranha.Models.Page page = pageService.GetPageByPermalink("start");
            Assert.AreEqual("Start", page.Title);
        }
    }
}
