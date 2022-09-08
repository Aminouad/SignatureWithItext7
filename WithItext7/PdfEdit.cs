using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.License;
using iText.PdfCleanup;
using iText.Signatures;
using WithItext7;
using WithItext7.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iText.Kernel.Pdf.Canvas.Parser.Listener.LocationTextExtractionStrategy;

namespace WithItext7
{
    public class PdfEdit
    { /// <summary>
      /// Find the text and replace in PDF
      /// </summary>
      /// <param name="sourceFile">The source PDF file where text to be searched</param>
      /// <param name="newFile">The new destination PDF file which will be saved with replaced text</param>
      /// <param name="textToBeSearched">The text to be searched in the PDF</param>
        public JokerPagePosition FindAndReplaceFirstJokerWithSignatureFrame(string sourceFile, string newFile, string textToBeSearched)
        {
            return ReplaceFirstJokerWithSignatureFrame(textToBeSearched, newFile, sourceFile);
        }

        private JokerPagePosition ReplaceFirstJokerWithSignatureFrame(string textToBeSearched, string outputFilePath, string inputFilePath)
        {
            //TextLocation position = null;
            JokerPagePosition jokerPagePosition = new JokerPagePosition();

            //LicenseKey.LoadLicenseFile(@"C:\Git\Elise\Neoledge.Elise\Programs and Components\Services\Neoledge.Elise.PdfToolkit\Resources\itextkey1561448586226_0.xml");

            try
            {
                //using (Stream inputPdfStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream inputImageStream = new FileStream(@"C:\signature_client.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream outputPdfStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    PdfReader reader = new PdfReader(inputFilePath);
                    var pdf = new PdfDocument(reader, new PdfWriter(outputPdfStream));
                    for (var i = 1; i <= pdf.GetNumberOfPages(); i++)
                    {
                        //joker search process
                        var strategy = new RegexBasedLocationExtractionStrategy(textToBeSearched);
                        var parser = new PdfCanvasProcessor(strategy);
                        parser.ProcessPageContent(pdf.GetPage(i));
                        var pdfTextLocations = strategy.GetResultantLocations();

                        var sortedPdfTextLocations = pdfTextLocations.OrderByDescending(p => p.GetRectangle().GetY()).ToList();
                        if (pdfTextLocations.Count > 0)
                        {
                            var pdfTextLocationRect = sortedPdfTextLocations.ElementAt(0).GetRectangle();
                            Console.WriteLine("First joker position" + pdfTextLocationRect.GetY());

                            try
                            {
                                //joker delete process       

                                IList<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
                                cleanUpLocations.Add(new PdfCleanUpLocation(i, pdfTextLocationRect));
                                PdfCleanUpTool cleaner = new PdfCleanUpTool(pdf, cleanUpLocations, new CleanUpProperties());
                                cleaner.CleanUp();
                            }
                            catch
                            {
                                Console.WriteLine("error in the delete process of the joker ");
                            }

                            jokerPagePosition.Page = i;
                            jokerPagePosition.Position = pdfTextLocationRect;
                            var rect = jokerPagePosition.Position;
                            //Add Annotation
                            float[] quad = { rect.GetLeft(), rect.GetBottom(), rect.GetRight(), rect.GetBottom()
                                           , rect.GetLeft(), rect.GetTop(), rect.GetRight(), rect.GetTop() };                           
                            PdfAnnotation annotation = PdfTextMarkupAnnotation.CreateHighLight(rect, quad);
                            annotation.SetContents(new PdfString("Cadre de signature"));
                            annotation.SetColor(new DeviceRgb(217, 217, 217));
                            pdf.GetPage(i).AddAnnotation(annotation);

                        }


                    }

                    pdf.Close();


                    return jokerPagePosition;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }



    }
}
