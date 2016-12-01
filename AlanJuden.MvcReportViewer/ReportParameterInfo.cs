﻿using System.Collections.Generic;

namespace AlanJuden.MvcReportViewer
{
    public class ReportParameterInfo
    {
        public string Name { get; set; }
        public string Prompt { get; set; }
        public bool MultiValue { get; set; }
        public Dictionary<string, string> ValidValues { get; set; }
        public List<string> SelectedValues { get; set; }
        public ReportService.ParameterTypeEnum Type { get; set; }
        public bool PromptUser { get; set; }

        public ReportParameterInfo()
        {
            ValidValues = new Dictionary<string, string>();
            SelectedValues = new List<string>();
        }
    }
}
