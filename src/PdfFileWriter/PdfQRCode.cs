/////////////////////////////////////////////////////////////////////
//
//	PdfFileWriter
//	PDF File Write C# Class Library.
//
//	PdfQRCode
//	Display QR Code as image resource.
//
//	Uzi Granot
//	Version: 1.0
//	Date: April 1, 2013
//	Copyright (C) 2013-2018 Uzi Granot. All Rights Reserved
//
//	PdfFileWriter C# class library and TestPdfFileWriter test/demo
//  application are free software.
//	They is distributed under the Code Project Open License (CPOL).
//	The document PdfFileWriterReadmeAndLicense.pdf contained within
//	the distribution specify the license agreement and other
//	conditions and notes. You must read this document and agree
//	with the conditions specified in order to use this software.
//
//	For version history please refer to PdfDocument.cs
//
/////////////////////////////////////////////////////////////////////

using System;

namespace PdfFileWriter
{
/// <summary>
/// QR Code error correction code enumeration
/// </summary>
public enum ErrorCorrection
	{
	/// <summary>
	/// Low
	/// </summary>
	L,

	/// <summary>
	/// Medium
	/// </summary>
	M,

	/// <summary>
	/// Medium-high
	/// </summary>
	Q,

	/// <summary>
	/// High
	/// </summary>
	H
	};

/// <summary>
/// PDF QR Code resource class
/// </summary>
/// <remarks>
/// <para>
/// The QR Code object is a PDF Image object.
/// </para>
/// <para>
/// For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#QRCodeSupport">2.8 QR Code Support</a>
/// </para>
/// </remarks>
public class PdfQRCode : PdfImage
	{
	/// <summary>
	/// Gets matrix dimension.
	/// </summary>
	public Int32 MatrixDimension {get; private set;}

	/// <summary>
	/// Segment marker
	/// </summary>
	public const char SegmentMarker = (char) 256;

	////////////////////////////////////////////////////////////////////
	/// <summary>
	/// PDF QR Code constructor
	/// </summary>
	/// <param name="Document">Parent PDF document.</param>
	/// <param name="DataString">Data string to encode.</param>
	/// <param name="ErrorCorrection">Error correction code.</param>
	/// <param name="QuietZone">Quiet zone</param>
	/// <param name="ModuleSize">Module size</param>
	////////////////////////////////////////////////////////////////////
	public PdfQRCode
			(
			PdfDocument		Document,
			string			DataString,
			ErrorCorrection	ErrorCorrection,
			int				QuietZone = 4,
			int				ModuleSize = 1
			) : base(Document)
		{
		// create QR Code object
		QREncoder Encoder = new QREncoder();
		Encoder.EncodeQRCode(DataString, ErrorCorrection);

		// PdfQRCode constructor helper
		ConstructorHelper(Encoder, QuietZone, ModuleSize);
		return;
		}

	////////////////////////////////////////////////////////////////////
	/// <summary>
	/// PDF QR Code constructor
	/// </summary>
	/// <param name="Document">Parent PDF document.</param>
	/// <param name="SegDataString">Data string array to encode.</param>
	/// <param name="ErrorCorrection">Error correction code.</param>
	/// <param name="QuietZone">Quiet zone</param>
	/// <param name="ModuleSize">Module size</param>
	/// <remarks>
	/// The program will calculate the best encoding mode for each segment.
	/// </remarks>
	////////////////////////////////////////////////////////////////////
	public PdfQRCode
			(
			PdfDocument		Document,
			string[]		SegDataString,
			ErrorCorrection	ErrorCorrection,
			int				QuietZone = 4,
			int				ModuleSize = 1
			) : base(Document)
		{
		// create QR Code object
		QREncoder Encoder = new QREncoder();
		Encoder.EncodeQRCode(SegDataString, ErrorCorrection);

		// PdfQRCode constructor helper
		ConstructorHelper(Encoder, QuietZone, ModuleSize);
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Write object to PDF file
	////////////////////////////////////////////////////////////////////

	internal void ConstructorHelper
			(
			QREncoder Encoder,
			int QuietZone,
			int ModuleSize
			)
		{
		// test arguments
		if(QuietZone < 0) throw new ApplicationException("QR Code quiet zone must be >= 0");
		if(ModuleSize < 1) throw new ApplicationException("QR Code module size must be >= 1");

		// image width and height in pixels
		MatrixDimension = Encoder.MatrixDimension;
		WidthPix = 2 * QuietZone + ModuleSize * MatrixDimension;
		HeightPix = WidthPix;

		// quiet module size > 1
		if(ModuleSize > 1)
			{
			// output matrix size in pixels
			BWImage = new Boolean[WidthPix, HeightPix];

			// convert result matrix to output matrix
			for(int Row = 0; Row < MatrixDimension; Row++)
				{
				int YOffset = QuietZone + ModuleSize * Row;
				for(int Col = 0; Col < MatrixDimension; Col++)
					{
					if(!Encoder.OutputMatrix[Row, Col]) continue;
					int XOffset = QuietZone + ModuleSize * Col;
					for(int Y = 0; Y < ModuleSize; Y++)
						for(int X = 0; X < ModuleSize; X++)
							BWImage[YOffset + Y, XOffset + X] = true;
					}
				}
			}

		// quiet zone == 0 and module size == 1
		else if(QuietZone == 0)
			{
			// output matrix
			// NOTE: Black=true, White=false
			BWImage = Encoder.OutputMatrix;
			}

		// quiet zone > 0 and module size == 1
		else
			{
			// output matrix size in pixels
			BWImage = new Boolean[WidthPix, HeightPix];

			// convert result matrix to output matrix
			for(int Row = 0; Row < MatrixDimension; Row++)
				{
				for(int Col = 0; Col < MatrixDimension; Col++) BWImage[QuietZone + Row, QuietZone + Col] = Encoder.OutputMatrix[Row, Col];
				}
			}

		// image control for QR code
		ImageControl = new PdfImageControl();
		ImageControl.ReverseBW = true;
		ImageControl.SaveAs = SaveImageAs.BWImage;

		// write stream object
		WriteObjectToPdfFile();
		return;
		}
	}
}
