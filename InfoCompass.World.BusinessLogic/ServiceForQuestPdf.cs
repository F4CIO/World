using InfoCompass.BuildingBlocks.Common;
using InfoCompass.World.Common.Entities;
using InfoCompass.BuildingBlocks.Logging;
using InfoCompass.BuildingBlocks.IO;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Settings = InfoCompass.World.Common.Entities.Settings;
using System.Reflection.Metadata;

namespace InfoCompass.World.BusinessLogic
{
	public interface IServiceForQuestPdf
	{
		Task ExportToDocx(Book book, BookSettings bookSettings, string filePath, CustomTraceLog log);
	}

	internal class ServiceForQuestPdf : IServiceForQuestPdf
	{
		private Settings _settings;
		private CustomTraceLog _globalLog;
		private CustomTraceLog log;
		private BookSettings _bookSettings;
		private Book _book;

		public ServiceForQuestPdf(IOptions<Settings> settings, CustomTraceLog globalLog)
		{
			this._settings = settings.Value;
			this._globalLog = globalLog;
		}

		public async Task ExportToDocx(Book book, BookSettings bookSettings, string filePath, CustomTraceLog log)
		{
			this.log = log;
			this._bookSettings = bookSettings;

			using (log.LogScope($"Exporting to {filePath} ..."))
			{
				using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
				{
					log.AddLine("Creating document...");
					MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
					mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
					this.AddStyles(mainPart);
					Body body = mainPart.Document.AppendChild(new Body());

					log.AddLine("Adding start pages...", false);
					this.AddBlankPages(body, this._bookSettings.EndpapersCountAtStart);
					this.AddMainTitle(body, book.Title);
					this.AddParagraph(body, $"-{book.Author}-", JustificationValues.Center);
					this.AddParagraph(body, $"{book.Edition}", JustificationValues.Center);
					this.AddParagraph(body, $"{book.Copyright}");
					this.AddParagraph(body, $"{book.ISBN}", JustificationValues.Left);
					this.AddParagraph(body, $"{book.PublicationDate}", JustificationValues.Left);
					this.AddSection(body, book.LegalNoticesTitle, book.LegalNotices);
					this.AddSection(body, book.DedicationTitle, book.Dedication);
					this.AddSection(body, book.AcknowledgmentsTitle, book.Acknowledgments);


					log.AddLine("Adding table of contents...", false);
					this.AddTableOfContents(body);

					var settingsPart = mainPart.AddNewPart<DocumentSettingsPart>();
					settingsPart.Settings = new DocumentFormat.OpenXml.Wordprocessing.Settings();
					settingsPart.Settings.Append(new BordersDoNotSurroundFooter() { Val = true });
					settingsPart.Settings.Append(new UpdateFieldsOnOpen() { Val = true });

					this.AddSection(body, (this._bookSettings.StartNumberingFromIntroductionInsteadFirstChapter.Value ? "1. " : "") + book.IntroductionTitle, book.Introduction);

					log.AddLine("Adding chapters...", false);
					if (book.Chapters != null && book.Chapters.Count > 0)
					{
						foreach (var chapter in book.Chapters)
						{
							log.AddLine($"Adding chapter {chapter.Order}. {chapter.Title} ...", false);
							string title = (chapter.Order + (this._bookSettings.StartNumberingFromIntroductionInsteadFirstChapter.Value ? 1 : 0))+". " + chapter.Title;
							this.AddSection(body, title, chapter.Body);
						}
					}

					log.AddLine("Adding end pages...", false);
					this.AddSection(body, (this._bookSettings.StartNumberingFromIntroductionInsteadFirstChapter.Value ? (book.Chapters.Count+1)+". " : "") + book.ConclusionTitle, book.Conclusion);
					this.AddSection(body, book.AppendicesTitle, book.Appendices);
					this.AddSection(body, book.EndnotesTitle, book.Endnotes);
					this.AddSection(body, book.BibliographyTitle, book.Bibliography);
					this.AddSection(body, book.IndexTitle, book.Index);
					this.AddSection(body, book.GlossaryTitle, book.Glossary);
					this.AddSection(body, book.AuthorBioTitle, book.AuthorBio);
					this.AddSection(body, book.AboutPublisherTitle, book.AboutPublisher);
					this.AddBlankPages(body, this._bookSettings.EndpapersCountAtEnd);


					log.AddLine("Adding page numbering...", false);
					this.AddPageNumbering(mainPart);


					log.AddLine("Saving file...", false);
					wordDocument.MainDocumentPart.Document.Save();
				}
			}
		}

		private void AddBlankPages(Body body, int? endpapersCountAtStart)
		{
			for (int i = 0; i < endpapersCountAtStart.Value; i++)
			{
				this.AddPageBreak(body);
			}
		}

		/// <summary>
		/// Source: https://stackoverflow.com/questions/25056927/unable-to-use-existing-paragraph-styles-in-open-xml
		/// </summary>
		/// <param name="mainPart"></param>
		private void AddStyles(MainDocumentPart mainPart)
		{
			StyleDefinitionsPart stylesPart = mainPart.StyleDefinitionsPart;
			if (stylesPart == null)
			{
				//// Create empty style definitions part.
				var styleDefinitionsPart = mainPart.AddNewPart<StyleDefinitionsPart>();
				styleDefinitionsPart.Styles = new Styles();
				stylesPart = mainPart.StyleDefinitionsPart;
			}

			NumberingDefinitionsPart numberingPart = mainPart.NumberingDefinitionsPart;
			if (numberingPart == null)
			{
				// Create empty numbering definitions part.
				var numberingDefinitionsPart = mainPart.AddNewPart<NumberingDefinitionsPart>();
				numberingDefinitionsPart.Numbering = new Numbering();
			}

			//Import styles from word default template:
			//I'm looping the styles and adding them here
			//you could clone the whole StyleDefinitionsPart
			//but then you'd lose custom styles in your source doc
			using (WordprocessingDocument wordTemplate = WordprocessingDocument.Open(this._settings.DefaultDotxFilePathAbsolute, false))
			{
				foreach (var templateStyle in wordTemplate.MainDocumentPart.StyleDefinitionsPart.Styles)
				{
					stylesPart.Styles.Append(templateStyle.CloneNode(true));
				}
			}			

			// Define custom "MyCustomTitle" style
			Style titleStyle = new Style()
			{
				Type = StyleValues.Paragraph,
				StyleId = "Title",
				CustomStyle = true,
				StyleName = new StyleName() { Val = "MyCustomTitle" },
				BasedOn = new BasedOn() { Val = "Normal" },
				NextParagraphStyle = new NextParagraphStyle() { Val = "Normal" },
				StyleParagraphProperties = new StyleParagraphProperties(new Justification() { Val = JustificationValues.Center }),
				StyleRunProperties = new StyleRunProperties(new Bold(), new FontSize() { Val = (28 * 2).ToString() }) // Set font size to 28 for this example
			};

			// Add the style to the Styles part
			stylesPart.Styles.Append(titleStyle);
			stylesPart.Styles.Save();
		}

		private void AddMainTitle(Body body, string mainTitle)
		{
			if (mainTitle.IsNOTNullOrWhiteSpace())
			{
				// Create a paragraph for the main title
				Paragraph titleParagraph = new Paragraph();
							
				// Paragraph properties - TODO: title style is not set while Heading1 later works
				ParagraphProperties titleParaProps = new ParagraphProperties(new ParagraphStyleId() { Val = "Title" }, new Justification() { Val = JustificationValues.Center });
				titleParagraph.Append(titleParaProps);

				// Create run with properties and text
				Run titleRun = new Run();
				RunProperties titleRunProperties = new RunProperties();
				titleRunProperties.Append(new Bold(), new FontSize() { Val = (25 * 2).ToString() });
				titleRun.Append(titleRunProperties);
				titleRun.Append(new Text(mainTitle));
				titleParagraph.Append(titleRun);	

				body.Append(titleParagraph);
			}
		}

		private void AddSection(Body body, string sectionTitle, string sectionBody, bool skipEmptyParagraphs = true)
		{
			if (sectionBody.IsPrintable())
			{
				this.AddPageBreak(body);
			}

			if (sectionBody.IsPrintable() && sectionTitle.IsPrintable())
			{
				if(this._bookSettings.CapitalizeTitles.Value){
					sectionTitle = sectionTitle.ToUpper();
				}
				// Add section title
				Paragraph titleParagraph = new Paragraph();

				// Apply "Heading 1" style to the title
				ParagraphProperties titleParagraphProperties = new ParagraphProperties();
				titleParagraphProperties.Append(new ParagraphStyleId() { Val = "Heading1" }, new Justification() { Val = JustificationValues.Center });
				titleParagraph.Append(titleParagraphProperties);

				Run titleRun = new Run();
				titleRun.Append(new Text(sectionTitle));
				titleParagraph.Append(titleRun);

				// Apply a larger font size to the title run
				RunProperties titleRunProperties = new RunProperties();
				FontSize titleFontSize = new FontSize() { Val = (15*2).ToString() }; 
				titleRunProperties.Append(titleFontSize);
				titleRun.PrependChild(titleRunProperties);

				body.Append(titleParagraph);
			}
			
			if (sectionBody.IsPrintable())
			{
				List<string> paragraphs = sectionBody.Split("\n").ToList();
				// Add section body with multiple paragraphs
				foreach (string paragraphText in paragraphs)
				{
					if (skipEmptyParagraphs && paragraphText.IsNullOrNonPrintable())
					{
						//skip
					}
					else
					{
						this.AddParagraph(body, paragraphText.TrimNonPrintableChars());
					}
				}
			}
		}

		private void AddPageBreak(Body body)
		{
			// Add a page break
			Paragraph pageBreakParagraph = new Paragraph();
			Run pageBreakRun = new Run(new Break() { Type = BreakValues.Page });
			pageBreakParagraph.Append(pageBreakRun);
			body.Append(pageBreakParagraph);
		}

		private void AddParagraph(Body body, string paragraphText, JustificationValues justification = JustificationValues.Both)
		{
			Paragraph paragraph = new Paragraph();
			paragraph.Append(new ParagraphProperties(new ParagraphStyleId() { Val = "Normal" }, new Justification() { Val = justification }));
			Run run = new Run();
			Text text = new Text(paragraphText);
			run.Append(text);
			paragraph.Append(run);
			body.Append(paragraph);
		}

		private void AddPageNumbering(MainDocumentPart mainPart)
		{
			FooterPart footerPart = mainPart.AddNewPart<FooterPart>("rIdFooter1");
			GeneratePageNumberPart1Content(footerPart);

			SectionProperties sectionProperties = mainPart.Document.Descendants<SectionProperties>().FirstOrDefault();
			if (sectionProperties == null)
			{
				sectionProperties = new SectionProperties();
				mainPart.Document.Body.InsertAt(sectionProperties, 0);
			}

			FooterReference footerReference = new FooterReference() { Type = HeaderFooterValues.Default, Id = "rIdFooter1" };
			sectionProperties.InsertAt(footerReference, 0);
		}

		private void GeneratePageNumberPart1Content(FooterPart footerPart)
		{
			Footer footer = new Footer();
			Paragraph paragraph = new Paragraph();
			Run run = new Run();
			FieldChar fieldCharBegin = new FieldChar() { FieldCharType = FieldCharValues.Begin };
			FieldCode fieldCode = new FieldCode("PAGE");
			FieldChar fieldCharSeparate = new FieldChar() { FieldCharType = FieldCharValues.Separate };
			RunProperties runProperties = new RunProperties();
			NoProof noProof = new NoProof();
			runProperties.Append(noProof);
			Text text = new Text() { Space = SpaceProcessingModeValues.Preserve };
			FieldChar fieldCharEnd = new FieldChar() { FieldCharType = FieldCharValues.End };
			paragraph.Append(new ParagraphProperties(new Justification() { Val = JustificationValues.Right }));
			run.Append(fieldCharBegin, fieldCode, fieldCharSeparate, runProperties, text, fieldCharEnd);
			paragraph.Append(run);
			footer.Append(paragraph);
			footerPart.Footer = footer;
		}

		/// <summary>
		/// Source: https://stackoverflow.com/questions/9762684/how-to-generate-table-of-contents-using-openxml-sdk-2-0
		/// </summary>
		/// <param name="body"></param>
		private void AddTableOfContents(Body body)
		{
			this.AddPageBreak(body);

			string title = this._bookSettings.TableofContentsTitle;
			if(this._bookSettings.CapitalizeTitles.Value){
				title = title.ToUpper();
			}

			var sdtBlock = new SdtBlock();
			sdtBlock.InnerXml = GetTOC(title, 15);
			body.AppendChild(sdtBlock);			
		}

		private static string GetTOC(string title, int titleFontSize)
		{
			return $@"<w:sdt>
     <w:sdtPr>
        <w:id w:val=""-493258456"" />
        <w:docPartObj>
           <w:docPartGallery w:val=""Table of Contents"" />
           <w:docPartUnique />
        </w:docPartObj>
     </w:sdtPr>
     <w:sdtEndPr>
        <w:rPr>
           <w:rFonts w:asciiTheme=""minorHAnsi"" w:eastAsiaTheme=""minorHAnsi"" w:hAnsiTheme=""minorHAnsi"" w:cstheme=""minorBidi"" />
           <w:b />
           <w:bCs />
           <w:noProof />
           <w:color w:val=""auto"" />
           <w:sz w:val=""22"" />
           <w:szCs w:val=""22"" />
        </w:rPr>
     </w:sdtEndPr>
     <w:sdtContent>
        <w:p w:rsidR=""00095C65"" w:rsidRDefault=""00095C65"">
           <w:pPr>
              <w:pStyle w:val=""TOCHeading"" />
              <w:jc w:val=""center"" /> 
           </w:pPr>
           <w:r>
                <w:rPr>
                  <w:b /> 
                  <w:color w:val=""2E74B5"" w:themeColor=""accent1"" w:themeShade=""BF"" /> 
                  <w:sz w:val=""{titleFontSize * 2}"" /> 
                  <w:szCs w:val=""{titleFontSize * 2}"" /> 
              </w:rPr>
              <w:t>{title}</w:t>
           </w:r>
        </w:p>
        <w:p w:rsidR=""00095C65"" w:rsidRDefault=""00095C65"">
           <w:r>
              <w:rPr>
                 <w:b />
                 <w:bCs />
                 <w:noProof />
              </w:rPr>
              <w:fldChar w:fldCharType=""begin"" />
           </w:r>
           <w:r>
              <w:rPr>
                 <w:b />
                 <w:bCs />
                 <w:noProof />
              </w:rPr>
              <w:instrText xml:space=""preserve""> TOC \o ""1-3"" \h \z \u </w:instrText>
           </w:r>
           <w:r>
              <w:rPr>
                 <w:b />
                 <w:bCs />
                 <w:noProof />
              </w:rPr>
              <w:fldChar w:fldCharType=""separate"" />
           </w:r>
           <w:r>
              <w:rPr>
                 <w:noProof />
              </w:rPr>
              <w:t>No table of contents entries found.</w:t>
           </w:r>
           <w:r>
              <w:rPr>
                 <w:b />
                 <w:bCs />
                 <w:noProof />
              </w:rPr>
              <w:fldChar w:fldCharType=""end"" />
           </w:r>
        </w:p>
     </w:sdtContent>
  </w:sdt>";

		}

		//private void CreateTableOfContents(MainDocumentPart mainPart)
		//{
		//	SdtBlock sdtBlock = new SdtBlock(
		//		new SdtProperties(
		//			new SdtStyle() { Val = "TOC" },
		//			new SdtContentDocPartObject()
		//		),
		//		new SdtContentBlock(
		//			new SdtProperties(),
		//			new Run(
		//				new RunProperties(
		//					new Bold(),
		//					new Color() { Val = "365F91", ThemeColor = ThemeColorValues.Hyperlink },
		//					new RunFonts() { Ascii = "Cambria", HighAnsi = "Cambria" },
		//					new FontSize() { Val = "32" }
		//				),
		//				new Text("Table of Contents")
		//			),
		//			new Paragraph(
		//				new ParagraphProperties(
		//					new ParagraphStyleId() { Val = "TOCHeading" }
		//				),
		//				new Run(
		//					new RunProperties(
		//						new RunFonts() { Ascii = "Cambria", HighAnsi = "Cambria" },
		//						new FontSize() { Val = "26" },
		//						new Bold()
		//					),
		//					new Text("1. Section 1")
		//				),
		//				new Run(
		//					new RunProperties(
		//						new RunFonts() { Ascii = "Cambria", HighAnsi = "Cambria" },
		//						new FontSize() { Val = "26" }
		//					),
		//					new Text("...1")
		//				),
		//				new Run(
		//					new RunProperties(
		//						new RunFonts() { Ascii = "Cambria", HighAnsi = "Cambria" },
		//						new FontSize() { Val = "26" },
		//						new Bold()
		//					),
		//					new Text("2. Section 2")
		//				),
		//				new Run(
		//					new RunProperties(
		//						new RunFonts() { Ascii = "Cambria", HighAnsi = "Cambria" },
		//						new FontSize() { Val = "26" }
		//					),
		//					new Text("...2")
		//				)
		//			)
		//		)
		//	);

		//}		
	}

	internal class MockedServiceForQuestPdf : IServiceForQuestPdf
	{
		public Task ExportToDocx(Book book, BookSettings bookSettings, string filePath, CustomTraceLog log)
		{
			throw new NotImplementedException();
		}
	}
}