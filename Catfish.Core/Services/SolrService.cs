﻿using Catfish.Core.Models;
using SolrNet;
using SolrNet.Attributes;
using SolrNet.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catfish.Core.Services
{
    public class SolrService
    {
        public static bool IsInitialized { get; private set; }

        private static SolrConnection mSolr { get; set; }

        public static void Init(string server)
        {
            IsInitialized = false;
            if (!string.IsNullOrEmpty(server))
            {
                mSolr = new SolrConnection(server);
                Startup.Init<SolrIndex>(mSolr);

                //TODO: Should we update the database here or have it in an external cron job

                IsInitialized = true;
            }
            else
            {
                throw new InvalidOperationException("The app parameter Solr Server string has not been defined.");
            }
        }

        public static string EscapeQueryString(string searchString)
        {
            string result = searchString.Replace("\"", "\\\"")
                .Replace(":", "\\:");

            return new SolrQuery(result).Query;
        }

        public SolrService()
        {
        }
    }

    public class SolrIndex
    {
        [SolrUniqueKey("id")]
        public string SolrId { get; set; }

        [SolrField("id_s")]
        public int Id { get; set; }
    }
}
