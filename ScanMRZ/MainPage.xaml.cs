using System.Collections.ObjectModel;
using Dynamsoft.MRZScannerBundle.Maui;

namespace ScanMRZ;

public partial class MainPage : ContentPage
{
    public ObservableCollection<TableItem> TableItems { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnScanMRZ(object sender, EventArgs e)
    {
        var config = new MRZScannerConfig("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9");
        var result = await MRZScanner.Start(config);

        TableItems.Clear();

        if (result.ResultStatus == EnumResultStatus.Finished && result.Data is not null)
        {
            var data = result.Data;

            TableItems.Add(new TableItem { Key = "Name", Value = $"{data.FirstName} {data.LastName}" });
            TableItems.Add(new TableItem { Key = "Sex", Value = data.Sex.ToUpperInvariant() });
            TableItems.Add(new TableItem { Key = "Age", Value = data.Age.ToString() });
            TableItems.Add(new TableItem { Key = "Document Type", Value = data.DocumentType });
            TableItems.Add(new TableItem { Key = "Document Number", Value = data.DocumentNumber });
            TableItems.Add(new TableItem { Key = "Issuing State", Value = data.IssuingState });
            TableItems.Add(new TableItem { Key = "Nationality", Value = data.Nationality });
            TableItems.Add(new TableItem { Key = "Date Of Birth (YYYY-MM-DD)", Value = data.DateOfBirth });
            TableItems.Add(new TableItem { Key = "Date Of Expire (YYYY-MM-DD)", Value = data.DateOfExpire });
        }
        else
        {
            var msg = result.ResultStatus == EnumResultStatus.Canceled
                ? "Scanning canceled"
                : result.ErrorString ?? "Unknown error";

            TableItems.Add(new TableItem { Key = "Result", Value = msg });
        }
    }
}

public class TableItem
{
    public string Key { get; set; }
    public string Value { get; set; }
}
