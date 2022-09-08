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

Queue<JokersPagesPositions> queue = new Queue<JokersPagesPositions>();
string sourceFile = @"C:\oldFile.pdf";
string descFile = @"C:\oldFileWithoutFirstJoker.pdf";

string sourceFile1 = @"C:\oldFileWithoutFirstJoker.pdf";
string descFile1 = @"C:\oldFileWithoutSecondJoker.pdf";

//string sourceFile2 = @"C:\oldFileWithoutSecondJoker.pdf";
//string descFile2 = @"C:\oldFileWithoutThirdJoker.pdf";
string signatureJoker = "#SIGNATURE #";

PdfEdit pdfObj = new PdfEdit();
queue.Enqueue(pdfObj.FindAndReplaceFirstJokerWithSignatureFrame(sourceFile, descFile, signatureJoker));
queue.Enqueue(pdfObj.FindAndReplaceFirstJokerWithSignatureFrame(sourceFile1, descFile1, signatureJoker));

static void signPdfFile(string sourceDocument, string destinationPath, Stream privateKeyStream,
                        string keyPassword, string reason, string location, JokerPagePosition jokerPagePosition,
                        bool visibleSignature, string signatureImagePath, string signatureNameField)
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
        PdfPage p = pdf.GetPage(jokerPagePosition.Page);
        var listeAnnotations = p.GetAnnotations();
        listeAnnotations.ElementAt(0);
        p.RemoveAnnotation(listeAnnotations.ElementAt(0));


        PdfSignatureAppearance appearance = signer.GetSignatureAppearance();
        PdfSignatureAppearance appearance1 = signer.GetSignatureAppearance();
        if (visibleSignature)
        {
            signer.SetFieldName(signatureNameField);
            //Rectangle position = jokerPagePosition.Position;
            var widthRectangle = 85;
            var heightRectangle = 30;

            //Rectangle rectangle = new Rectangle(0, 0, 0, 0);

            var X= jokerPagePosition.Position.GetX() + 40;
            var Y= jokerPagePosition.Position.GetY() - 8;
            
            jokerPagePosition.Position.SetBbox(X, Y, X + widthRectangle, Y + heightRectangle);

            appearance.SetPageNumber(jokerPagePosition.Page);
            //jokerPagePosition.Position.SetWidth(widthRectangle);
            //jokerPagePosition.Position.SetHeight(heightRectangle);

            //jokerPagePosition.Position.IncreaseWidth(50);
            //jokerPagePosition.Position.IncreaseHeight(10);
            appearance.SetPageRect(jokerPagePosition.Position);

            

            //appearance.SetVisibleSignature(rectangle, jokerPagePosition.Page, signatureNameField);
            Uri path = new Uri(signatureImagePath);
            ImageData image = ImageDataFactory.CreatePng(path);

            //appearance.SetImage(image);
            appearance.SetRenderingMode(RenderingMode.GRAPHIC);
            appearance.SetSignatureGraphic(image);
        }
        // digital signature
        IExternalSignature es = new PrivateKeySignature(pk, "SHA256");
        CryptoStandard sigtype = CryptoStandard.CMS;
        signer.SignDetached(es, new X509Certificate[] { pk12.GetCertificate(alias).Certificate }, null, null, null, 0, sigtype);

    }
}
using (Stream MyCert = new FileStream(@"C:\public_privatekey.pfx", FileMode.Open, FileAccess.Read, FileShare.Read))
{
    signPdfFile(descFile1, @"C:\FirstSignature.pdf", MyCert, "/*twayf*/", "", "", queue.Dequeue(), true, @"C:\signature_client.png", "SignatureClient");

}
Thread.Sleep(1000);
using (Stream MyCert = new FileStream(@"C:\cert.pfx", FileMode.Open, FileAccess.Read, FileShare.Read))
{
    signPdfFile(@"C:\FirstSignature.pdf", @"C:\SecondSignature.pdf", MyCert, "root", "", "", queue.Dequeue(), true, @"C:\signature_société.png", "SignatureCompany");
}

