namespace mrz_scanner_mobile_maui;

public partial class ResultPage : ContentPage
{
    public ResultPage(Dictionary<String, String> labelMap)
    {
        InitializeComponent();
        if (labelMap.Count > 0)
        {
            LabelName.Text = labelMap["Name"];
            if (!labelMap["Sex"].Equals("Unknown"))
            {
                string sex = labelMap["Sex"];
                LabelSex.Text = sex.First().ToString().ToUpper() + sex.Substring(1) + ", Age: " + labelMap["Age"];
                VerticalLayout.Add(ChildView("Document Type:", labelMap["Document Type"]));
                VerticalLayout.Add(ChildView("Document Number:", labelMap["Document Number"]));
                VerticalLayout.Add(ChildView("Issuing State:", labelMap["Issuing State"]));
                VerticalLayout.Add(ChildView("Nationality:", labelMap["Nationality"]));
                VerticalLayout.Add(ChildView("Date of Birth(YYYY-MM-DD):", labelMap["Date of Birth(YY-MM-DD)"]));
                VerticalLayout.Add(ChildView("Date of Expiry(YYYY-MM-DD):", labelMap["Date of Expiry(YY-MM-DD)"]));
                VerticalLayout.Add(new Label
                {
                    Text = "Powered by Dynamsoft",
                    FontSize = 16,
                    TextColor = Color.FromArgb("#999999"),
                    Padding = new Thickness(0, 86, 0, 0),
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                });
            }

        }

        IView ChildView(string label, string text)
        {
            return new VerticalStackLayout
            {
                new Label
                {
                    Text = label,
                    TextColor = Color.FromArgb("AAAAAA"),
                    FontSize = 16,
                    Padding = new Thickness(0, 20, 0, 0),
                },
                new Label
                {
                    Text = text,
                    TextColor = Colors.White,
                    FontSize = 16,

                }
            };

        }
    }
}
