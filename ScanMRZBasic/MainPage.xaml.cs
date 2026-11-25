using Dynamsoft.CodeParser.Maui;
using Dynamsoft.MRZScannerBundle.Maui;

namespace ScanMRZBasic;

public partial class MainPage : ContentPage
{
	// Android can rebuild this page while the scanner is open, so the text outlives it.
	private static string _resultText = "";

	public MainPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ResultLabel.Text = _resultText;
	}

	private async void OnScanMRZ(object? sender, EventArgs e)
	{
		// A trial license, so it needs a network connection. Request your own at
		// https://www.dynamsoft.com/customer/license/trialLicense/?product=mrz&utm_source=samples&package=maui
		var result = await MRZScanner.Start(new MRZScannerConfig("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9"));

		_resultText = result.ResultStatus switch
		{
			EnumResultStatus.Finished when result.Data is { } data => Describe(data),
			EnumResultStatus.Canceled => "Scan canceled",
			EnumResultStatus.Exception => result.ErrorString ?? "",
			_ => ""
		};
		ResultLabel.Text = _resultText;
	}

	private static string Describe(MRZData data)
	{
		// The fields the scanner checks against the MRZ check digits.
		string[] validatedFields = ["firstName", "lastName", "sex", "issuingState", "nationality", "dateOfBirth",
			"dateOfExpire", "documentNumber", "mrzText", "optionalData1", "optionalData2", "personalNumber"];
		var invalidFields = validatedFields.Where(f => data.GetFieldValidationStatus(f) == EnumValidationStatus.VS_FAILED);

		return $"""
			Name: {data.FirstName} {data.LastName}
			Document Number: {data.DocumentNumber}
			Nationality: {data.Nationality}
			Date of Birth: {data.DateOfBirth}
			Date of Expiry: {data.DateOfExpire}
			Document Type: {data.DocumentType}
			Invalid Fields: [{string.Join(", ", invalidFields)}]
			MRZ Text:
			{data.MrzText}
			""";
	}
}
