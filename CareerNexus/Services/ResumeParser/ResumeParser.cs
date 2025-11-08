using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;


namespace CareerNexus.Services.ResumeParser
{
    public class ResumeParser:IResumeParser
    {
        //public async Task<string> ExtractTextFromFileAsync(IFormFile file)
        //{
        //    using var stream = new MemoryStream();
        //    await file.CopyToAsync(stream);
        //    var extractor = new TextExtractor();
        //    var text = extractor.Extract(stream).Text;
        //    return text ?? string.Empty;
        //}

        public async Task<string> ExtractTextFromFileAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => await ExtractTextFromPdfAsync(file),
                ".docx" => await ExtractTextFromWordAsync(file),
                ".doc" => await ExtractTextFromDocFallbackAsync(file), // optional
                _ => throw new NotSupportedException($"File type {extension} is not supported")
            };
        }

        private async Task<string> ExtractTextFromPdfAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            var sb = new StringBuilder();
            using var document = UglyToad.PdfPig.PdfDocument.Open(ms);
            foreach (Page page in document.GetPages())
            {
                sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }

        private async Task<string> ExtractTextFromWordAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            using var doc = WordprocessingDocument.Open(ms, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            return body?.InnerText ?? string.Empty;
        }

        // Optional fallback for .doc (legacy) — uses NPOI if you add that package
        private async Task<string> ExtractTextFromDocFallbackAsync(IFormFile file)
        {
            // implement if needed, or tell user to upload .docx/pdf only
            return string.Empty;
        }
        /////FILHAL YE NH CHAL RHA ONLY PDF WORKING ... 

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="file"></param>
        ///// <returns></returns>
        //private async Task<string> ExtractTextFromWordAsync(IFormFile file)
        //{
        //    using var memoryStream = new MemoryStream();
        //    await file.CopyToAsync(memoryStream);
        //    memoryStream.Position = 0;

        //    using var document = WordprocessingDocument.Open(memoryStream, false);
        //    var body = document.MainDocumentPart?.Document?.Body;

        //    if (body == null) return string.Empty;

        //    var text = new System.Text.StringBuilder();
        //    foreach (var paragraph in body.Elements<Paragraph>())
        //    {
        //        text.AppendLine(paragraph.InnerText);
        //    }

        //    return text.ToString();
        //}

        //private async Task<string> ExtractTextFromPdfAsync(IFormFile file)
        //{
        //    using var memoryStream = new MemoryStream();
        //    await file.CopyToAsync(memoryStream);
        //    memoryStream.Position = 0;

        //    using var pdfReader = new PdfReader(memoryStream);
        //    var text = new System.Text.StringBuilder();

        //    for (int page = 1; page <= pdfReader.NumberOfPages; page++)
        //    {
        //        text.AppendLine(PdfTextExtractor.GetTextFromPage(pdfReader, page));
        //    }

        //    return text.ToString();
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="file"></param>
        ///// <returns></returns>

    }
}
