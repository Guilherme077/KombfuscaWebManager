using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.CupModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace KombfuscaWebManager.Controllers
{
    public class DocsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public DocsController(IWebHostEnvironment env, ApplicationDbContext context)
        {
            _env = env;
            _context = context;
        }

        [Authorize(Roles = Roles.Admin)]
        public IActionResult GenerateScoreSheets(int cupId, string footer)
        {
            var cup = _context.Cups.Where(c => c.Id == cupId).Include(c => c.Periods).FirstOrDefault();

            int nPages = cup.Periods.Count;
            string gameName = cup.Name;
            string[] periodsDesc = cup.Periods.Select(p => p.Description).ToArray();

            string modelPath = Path.Combine(_env.ContentRootPath, "Services", "PdfModels", "scoreSheetModel.pdf");
            string coverModelPath = Path.Combine(_env.ContentRootPath, "Services", "PdfModels", "coverScoreSheetModel.pdf");

            if (!System.IO.File.Exists(modelPath))
            {
                return NotFound("ScoreSheet Model not found.");
            }
            if (!System.IO.File.Exists(coverModelPath))
            {
                return NotFound("Cover Model not found.");
            }

            using (MemoryStream streamFinal = new MemoryStream())
            {
                PdfWriter writerFinal = new PdfWriter(streamFinal);
                PdfDocument pdfDest = new PdfDocument(writerFinal);


                using (MemoryStream streamTemp = new MemoryStream())
                {
                    PdfReader readerModel = new PdfReader(coverModelPath);
                    PdfWriter writerTemp = new PdfWriter(streamTemp);
                    PdfDocument pdfTemp = new PdfDocument(readerModel, writerTemp);

                    Document docLayout = new Document(pdfTemp);
                    PdfFont fontObj = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    Paragraph text = new Paragraph(gameName).SetFont(fontObj).SetFontSize(16).SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    float posX = 0;
                    float posY = 420;

                    text.SetFixedPosition(posX, posY, 595);

                    docLayout.Add(text);

                    docLayout.Close();
                    pdfTemp.Close();


                    using (MemoryStream streamLeituraTemp = new MemoryStream(streamTemp.ToArray()))
                    {
                        PdfReader readerPaginaPronta = new PdfReader(streamLeituraTemp);
                        PdfDocument pdfOriginalReady = new PdfDocument(readerPaginaPronta);

                        pdfOriginalReady.CopyPagesTo(1, 1, pdfDest);

                        pdfOriginalReady.Close();
                    }
                }


                for (int i = 1; i <= nPages; i++)
                {
                    using (MemoryStream streamTemp = new MemoryStream())
                    {
                        PdfReader readerModel = new PdfReader(modelPath);
                        PdfWriter writerTemp = new PdfWriter(streamTemp);
                        PdfDocument pdfTemp = new PdfDocument(readerModel, writerTemp);

                        Document docLayout = new Document(pdfTemp);
                        PdfFont fontObj = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                        //Page Number and period desc

                        Paragraph numberText = new Paragraph(i.ToString() + " | " + periodsDesc[i-1]).SetFont(fontObj).SetFontSize(10).SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);

                        float posX = 480;
                        float posY = 775;

                        numberText.SetFixedPosition(posX, posY, 75); // 75 is the max length

                        docLayout.Add(numberText);

                        //Footer
                        Paragraph footerText = new Paragraph(footer).SetFont(fontObj).SetFontSize(10).SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                        posX = 0;
                        posY = 35;

                        footerText.SetFixedPosition(posX, posY, 595); // 595 is the length of A4 paper

                        docLayout.Add(footerText);

                        //Game Name
                        fontObj = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        Paragraph gameNameText = new Paragraph(gameName).SetFont(fontObj).SetFontSize(14).SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                        posX = 80;
                        posY = 775;

                        gameNameText.SetFixedPosition(posX, posY, 450); // 450 is the max length (530-80)

                        docLayout.Add(gameNameText);

                        docLayout.Close();
                        pdfTemp.Close();

                        
                        using (MemoryStream streamLeituraTemp = new MemoryStream(streamTemp.ToArray()))
                        {
                            PdfReader readerPaginaPronta = new PdfReader(streamLeituraTemp);
                            PdfDocument pdfOriginalReady = new PdfDocument(readerPaginaPronta);

                            pdfOriginalReady.CopyPagesTo(1, 1, pdfDest);

                            pdfOriginalReady.Close();
                        }
                    }
                }

                pdfDest.Close();

                byte[] bytesPdf = streamFinal.ToArray();
                return File(bytesPdf, "application/pdf", $"Folha_Pontuacao_{gameName}.pdf");
            }
        }
    }
}
