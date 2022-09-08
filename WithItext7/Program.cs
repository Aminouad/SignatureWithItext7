using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using iText.Kernel.Pdf;
using iText.Signatures;
using iText.IO.Image;
using static iText.Signatures.PdfSigner;
using static iText.Signatures.PdfSignatureAppearance;

//Signature done don't forget to reorder the class jokerPagePosition
using WithItext7;
using WithItext7.Model;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Extgstate;
using iText.IO.Font;
using iText.IO.Font.Constants;

Queue<JokerPagePosition> queueSignatureJoker = new Queue<JokerPagePosition>();
Queue<JokerPagePosition> queueDateJoker = new Queue<JokerPagePosition>();

string sourceFile = @"C:\oldFile.pdf";
string descFile = @"C:\oldFileWithoutFirstSignatureJoker.pdf";

string sourceFile1 = @"C:\oldFileWithoutFirstSignatureJoker.pdf";
string descFile1 = @"C:\oldFileWithoutFirstDateJoker.pdf";

string sourceFile2 = @"C:\oldFileWithoutFirstDateJoker.pdf";
string descFile2 = @"C:\oldFileWithoutSecondSignatureJoker.pdf";

string sourceFile3 = @"C:\oldFileWithoutSecondSignatureJoker.pdf";
string descFile3 = @"C:\oldFileWithoutSecondDateJoker.pdf";
string signatureJoker = "#SIGNATURE#";
string dateJoker = "#DATE_SIGNATURE#";
string signatureFrame = "Cadre de signature";
string dateFrame = "Cadre de date de signature";

PdfEdit pdfObj = new PdfEdit();
queueSignatureJoker.Enqueue(pdfObj.FindAndReplaceFirstJokerWithSignatureFrame(sourceFile, descFile, signatureJoker, signatureFrame));
queueDateJoker.Enqueue(pdfObj.FindAndReplaceFirstJokerWithSignatureFrame(sourceFile1, descFile1, dateJoker, dateFrame));

queueSignatureJoker.Enqueue(pdfObj.FindAndReplaceFirstJokerWithSignatureFrame(sourceFile2, descFile2, signatureJoker, signatureFrame));
queueDateJoker.Enqueue(pdfObj.FindAndReplaceFirstJokerWithSignatureFrame(sourceFile3, descFile3, dateJoker, dateFrame));
Queue<JokerPagePosition> s = queueSignatureJoker;
Queue<JokerPagePosition> d = queueDateJoker;

static void signPdfFile(string sourceDocument, string destinationPath, Stream privateKeyStream,
                        string keyPassword, string reason, string location, JokerPagePosition jokerSignaturePagePosition,
                        JokerPagePosition jokerDatePagePosition, bool visibleSignature, string signatureImagePath,
                        string signatureNameField)

{
    Pkcs12Store pk12 = new Pkcs12Store(privateKeyStream, keyPassword.ToCharArray());
    privateKeyStream.Dispose();

    //then Iterate throught certificate entries to find the private key entry
    string alias = null;
    foreach (string tAlias in pk12.Aliases)
    {
        if (pk12.IsKeyEntry(tAlias))
        {
            alias = tAlias;
            break;
        }
    }
    var pk = pk12.GetKey(alias).Key;

    // reader and stamper    
    using (FileStream fout = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite))

    {

        PdfReader reader = new PdfReader(sourceDocument);
        //PdfDocument pdf = new PdfDocument(reader, new PdfWriter(fout));


        PdfSigner signer = new PdfSigner(reader, fout, new StampingProperties().UseAppendMode());
        var pdf = signer.GetDocument();
        PdfPage p = pdf.GetPage(jokerSignaturePagePosition.Page);
        ////delete Annotations
        var listeAnnotations = p.GetAnnotations();
       
            p.RemoveAnnotation(listeAnnotations.ElementAt(0));
            p.RemoveAnnotation(listeAnnotations.ElementAt(1));



        PdfSignatureAppearance appearance = signer.GetSignatureAppearance();

        var widthRectangle = 85;
        var heightRectangle = 30;

        //Rectangle rectangle = new Rectangle(0, 0, 0, 0);

        var X = jokerSignaturePagePosition.Position.GetX() ;
        var Y = jokerSignaturePagePosition.Position.GetY() ;
        // PdfSignatureAppearance appearance1 = signer.GetSignatureAppearance();
        if (visibleSignature)
        {
            signer.SetFieldName(signatureNameField);
            //Rectangle position = jokerPagePosition.Position;


            jokerSignaturePagePosition.Position.SetBbox(X, Y, X + widthRectangle, Y + heightRectangle);

            appearance.SetPageNumber(jokerSignaturePagePosition.Page);
            //jokerPagePosition.Position.SetWidth(widthRectangle);
            //jokerPagePosition.Position.SetHeight(heightRectangle);

            //jokerPagePosition.Position.IncreaseWidth(50);
            //jokerPagePosition.Position.IncreaseHeight(10);
            appearance.SetPageRect(jokerSignaturePagePosition.Position);

            

            //appearance.SetVisibleSignature(rectangle, jokerPagePosition.Page, signatureNameField);
            Uri path = new Uri(signatureImagePath);
            ImageData image = ImageDataFactory.CreatePng(path);

            //appearance.SetImage(image);
            appearance.SetRenderingMode(RenderingMode.GRAPHIC);
            appearance.SetSignatureGraphic(image);

            //try date

            //try canvas 1
            //PdfFormXObject layer2 = appearance.GetLayer2();
            //Rectangle rectangle = layer2.GetBBox().ToRectangle();
            //PdfCanvas canvas = new PdfCanvas(layer2, signer.GetDocument());
            //canvas.Rectangle(X + 90, Y + 90, X + widthRectangle, Y + heightRectangle);

            //canvas.SetFontAndSize(PdfFontFactory.CreateFont(), 10);
            //canvas.ShowText("datedate");




            //try canvas 1
            //PdfFreeTextAnnotation text = new PdfFreeTextAnnotation(new Rectangle(X + 90, Y + 90, X + widthRectangle, Y + heightRectangle), new PdfString(signer.GetSignDate().ToString()));
            //PdfTextAnnotation date = new PdfTextAnnotation(new Rectangle(X + 90, Y + 90, X + widthRectangle, Y + heightRectangle));
            //PdfAnnotationAppearance k = new PdfAnnotationAppearance();
            //text.SetNormalAppearance(k);
            float watermarkTrimmingRectangleWidth = 75;
            float watermarkTrimmingRectangleHeight = 250;
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            var page = pdf.GetPage(jokerDatePagePosition.Page);
            Rectangle ps = page.GetPageSize();
            float prodBottomLeftX = -20;
            float prodBottomLeftY = ps.GetHeight() / 2;

            Rectangle prodWatermarkTrimmingRectangle = new Rectangle(prodBottomLeftX, prodBottomLeftY, watermarkTrimmingRectangleWidth, watermarkTrimmingRectangleHeight);

            var X1 = jokerDatePagePosition.Position.GetX() ;
            var Y2 = jokerDatePagePosition.Position.GetY() ;
            //jokerDatePagePosition.Position.SetBbox(X, Y, X + widthRectangle, Y + heightRectangle);

            PdfWatermarkAnnotation watermark = new PdfWatermarkAnnotation(new Rectangle(X1,Y2+5,85,30));

            //Apply linear algebra rotation math
            //Create identity matrix
            AffineTransform transform = new AffineTransform();//No-args constructor creates the identity transform
            //Apply translation
            //transform.Translate(xTranslation, yTranslation);
            //Apply rotation
            //transform.Rotate(rotationInRads);

            PdfFixedPrint fixedPrint = new PdfFixedPrint();
            watermark.SetFixedPrint(fixedPrint);
            //Create appearance
            float formWidth = 80;
            float formHeight = 20;
            float formXOffset = 0;
            float formYOffset = 0;
            jokerDatePagePosition.Position.SetBbox(X, Y, 500, 500);
            Rectangle formRectangle = new Rectangle(formXOffset, formYOffset, formWidth, formHeight);

            //Observation: font XObject will be resized to fit inside the watermark rectangle
            PdfFormXObject form = new PdfFormXObject(formRectangle);
            PdfExtGState gs1 = new PdfExtGState().SetFillOpacity(0.6f);
            PdfCanvas canvas = new PdfCanvas(form, pdf);



            float fontSize = 6;
            float[] transformValues = new float[6];
           // transform.GetMatrix(transformValues);
            canvas.SaveState()
                .BeginText().SetColor(new DeviceRgb(0, 0, 0), true)
                .SetFontAndSize(font, fontSize)
                .ShowText(signer.GetSignDate().ToString())
                .EndText()
                .RestoreState();

            canvas.Release();

            watermark.SetAppearance(PdfName.N, new PdfAnnotationAppearance(form.GetPdfObject()));
            watermark.SetFlags(PdfAnnotation.PRINT);

            pdf.GetPage(jokerDatePagePosition.Page).AddAnnotation(watermark);

        }
        // digital signature


        IExternalSignature es = new PrivateKeySignature(pk, "SHA256");
        CryptoStandard sigtype = CryptoStandard.CMS;
        signer.SignDetached(es, new X509Certificate[] { pk12.GetCertificate(alias).Certificate }, null, null, null, 0, sigtype);

           
    }
}
using (Stream MyCert = new FileStream(@"C:\public_privatekey.pfx", FileMode.Open, FileAccess.Read, FileShare.Read))
{
    signPdfFile(descFile3, @"C:\FirstSignature.pdf", MyCert, "/*twayf*/", "", "", queueSignatureJoker.Dequeue(), queueDateJoker.Dequeue(),true, @"C:\signature_client.png", "SignatureClient");

}
Thread.Sleep(1000);
using (Stream MyCert = new FileStream(@"C:\cert.pfx", FileMode.Open, FileAccess.Read, FileShare.Read))
{
    signPdfFile(@"C:\FirstSignature.pdf", @"C:\SecondSignature.pdf", MyCert, "root", "", "", queueSignatureJoker.Dequeue(), queueDateJoker.Dequeue(), true, @"C:\signature_société.png", "SignatureCompany");
}

