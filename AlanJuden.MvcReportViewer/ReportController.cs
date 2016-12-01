﻿using System.Web;
using System.Web.Mvc;

namespace AlanJuden.MvcReportViewer
{
    public abstract class ReportController : Controller
    {
        protected abstract System.Net.ICredentials NetworkCredentials { get; }
        protected abstract string ReportServerUrl { get; }

        /// <summary>
        /// This indicates whether or not to replace image urls from your report server to image urls on your local site to act as a proxy
        /// *useful if your report server is not accessible publicly*
        /// </summary>
        protected virtual bool UseCustomReportImagePath => false;

        protected virtual string ReportImagePath => "/Report/ReportImage/?originalPath={0}";

        public JsonResult ViewReportPage(string reportPath, int? page = 0)
        {
            var model = this.GetReportViewerModel(Request);
            model.ViewMode = ReportViewModes.View;
            model.ReportPath = reportPath;

            var contentData = ReportServiceHelpers.ExportReportToFormat(model, ReportFormats.Html4_0, page, page);
            var content = System.Text.Encoding.ASCII.GetString(contentData.ReportData);
            if (model.UseCustomReportImagePath && model.ReportImagePath.HasValue())
            {
                content = ReportServiceHelpers.ReplaceImageUrls(model, content);
            }

            var jsonResult = Json(
                new
                {
                    CurrentPage = contentData.CurrentPage,
                    Content = content,
                    TotalPages = contentData.TotalPages
                }
                , JsonRequestBehavior.AllowGet
            );
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }

        public FileResult ExportReport(string reportPath, string format)
        {
            var model = GetReportViewerModel(Request);
            model.ViewMode = ReportViewModes.Export;
            model.ReportPath = reportPath;

            string extension;
            switch (format.ToUpper())
            {
                case "CSV":
                    format = "CSV";
                    extension = ".csv";
                    break;

                case "MHTML":
                    format = "MHTML";
                    extension = ".mht";
                    break;

                case "PDF":
                    format = "PDF";
                    extension = ".pdf";
                    break;

                case "TIFF":
                    format = "IMAGE";
                    extension = ".tif";
                    break;

                case "XML":
                    format = "XML";
                    extension = ".xml";
                    break;

                case "WORDOPENXML":
                    format = "WORDOPENXML";
                    extension = ".docx";
                    break;

                case "EXCELOPENXML":
                default:
                    format = "EXCELOPENXML";
                    extension = ".xlsx";
                    break;
            }

            var contentData = ReportServiceHelpers.ExportReportToFormat(model, format);

            var filename = reportPath;
            if (filename.Contains("/"))
            {
                filename = filename.Substring(filename.LastIndexOf("/"));
                filename = filename.Replace("/", "");
            }

            filename = filename + extension;

            return File(contentData.ReportData, contentData.MimeType, filename);
        }

        public JsonResult FindStringInReport(string reportPath, string searchText, int? startPage = 0)
        {
            var model = GetReportViewerModel(Request);
            model.ViewMode = ReportViewModes.View;
            model.ReportPath = reportPath;

            return Json(ReportServiceHelpers.FindStringInReport(model, searchText, startPage).ToInt32(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult PrintReport(string reportPath)
        {
            var model = GetReportViewerModel(Request);
            model.ViewMode = ReportViewModes.Print;
            model.ReportPath = reportPath;

            var contentData = ReportServiceHelpers.ExportReportToFormat(model, ReportFormats.Html4_0);
            var content = System.Text.Encoding.ASCII.GetString(contentData.ReportData);
            content = ReportServiceHelpers.ReplaceImageUrls(model, content);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("	<body>");
            //sb.AppendLine($"		<img src='data:image/tiff;base64,{Convert.ToBase64String(contentData.ReportData)}' />");
            sb.AppendLine($"		{content}");
            sb.AppendLine("		<script type='text/javascript'>");
            sb.AppendLine("			(function() {");
            /*
			sb.AppendLine("				var beforePrint = function() {");
			sb.AppendLine("					console.log('Functionality to run before printing.');");
			sb.AppendLine("				};");
			*/
            sb.AppendLine("				var afterPrint = function() {");
            sb.AppendLine("					window.onfocus = function() { window.close(); };");
            sb.AppendLine("					window.onmousemove = function() { window.close(); };");
            sb.AppendLine("				};");

            sb.AppendLine("				if (window.matchMedia) {");
            sb.AppendLine("					var mediaQueryList = window.matchMedia('print');");
            sb.AppendLine("					mediaQueryList.addListener(function(mql) {");
            sb.AppendLine("						if (mql.matches) {");
            //sb.AppendLine("							beforePrint();");
            sb.AppendLine("						} else {");
            sb.AppendLine("							afterPrint();");
            sb.AppendLine("						}");
            sb.AppendLine("					});");
            sb.AppendLine("				}");

            //sb.AppendLine("				window.onbeforeprint = beforePrint;");
            sb.AppendLine("				window.onafterprint = afterPrint;");

            sb.AppendLine("			}());");
            sb.AppendLine("			window.print();");
            sb.AppendLine("		</script>");
            sb.AppendLine("	</body>");

            sb.AppendLine("<html>");

            return Content(sb.ToString());
        }

        public FileContentResult ReportImage(string originalPath)
        {
            var rawUrl = Request.RawUrl.UrlDecode();
            var startIndex = rawUrl.IndexOf(originalPath);
            if (startIndex > -1)
            {
                originalPath = rawUrl.Substring(startIndex);
            }

            var client = new System.Net.WebClient { Credentials = NetworkCredentials };
            var imageData = client.DownloadData(originalPath);

            return new FileContentResult(imageData, "image/png");
        }

        protected ReportViewerModel GetReportViewerModel(HttpRequestBase request)
        {
            var model = new ReportViewerModel
            {
                Credentials = NetworkCredentials,
                ServerUrl = ReportServerUrl,
                ReportImagePath = ReportImagePath,
                UseCustomReportImagePath = UseCustomReportImagePath
            };
            model.BuildParameters(Request);

            return model;
        }
    }
}
