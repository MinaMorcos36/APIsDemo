using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;
using System.Text;
using UglyToad.PdfPig;

namespace APIsDemo.Services;

public class FileParsingService
{
    public string Parse(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLower();

        return ext switch
        {
            ".txt" => ParseTxt(file),
            ".pdf" => ParsePdf(file),
            ".docx" => ParseDocx(file),
            _ => throw new NotSupportedException("File type not supported")
        };
    }

    private string ParseTxt(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        return reader.ReadToEnd();
    }

    private string ParsePdf(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var pdf = PdfDocument.Open(stream);
        var sb = new StringBuilder();
        foreach (var page in pdf.GetPages())
        {
            sb.AppendLine(page.Text);
        }
        return sb.ToString();
    }

    private string ParseDocx(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart.Document.Body;
        return body.InnerText;
    }
}
