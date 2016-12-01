﻿using System.Collections.Generic;
using System.Web;

namespace AlanJuden.MvcReportViewer
{
    public class ReportViewerModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ReportPath { get; set; }

        /// <summary>
        /// This indicates whether or not to replace image urls from your report server to image urls on your local site to act as a proxy
        /// *useful if your report server is not accessible publicly*
        /// </summary>
        public bool UseCustomReportImagePath { get; set; }

        /// <summary>
        /// This is the local URL on your website that will handle returning images for you, be sure to use the replacement variable {0} in your string to represent the original image URL that came from your report server.
        /// </summary>
        public string ReportImagePath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public System.Net.ICredentials Credentials { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string[]> Parameters { get; set; }

        public bool ShowHiddenParameters { get; set; }

        public ReportViewModes ViewMode { get; set; }

        public ReportViewerModel()
        {
            Parameters = new Dictionary<string, string[]>();
            ViewMode = ReportViewModes.View;
        }

        public void AddParameter(string name, string value)
        {
            AddParameter(name, new string[1] { value });
        }

        public void AddParameter(string name, string[] values)
        {
            if (Parameters.ContainsKey(name))
            {
                Parameters[name] = values;
            }
            else
            {
                Parameters.Add(name, values);
            }
        }

        public void BuildParameters(HttpRequestBase request)
        {
            foreach (var key in request.QueryString.AllKeys)
            {
                AddParameter(key, request.QueryString[key].ToSafeString().ToStringList().ToArray());
            }

            foreach (var key in request.Form.AllKeys)
            {
                AddParameter(key, request.Form[key].ToSafeString().ToStringList().ToArray());
            }
        }
    }
}
