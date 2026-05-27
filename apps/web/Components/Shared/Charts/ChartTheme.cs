using ApexCharts;

namespace Web.DermaImage.Components.Shared.Charts;

public static class ChartTheme
{
    private static readonly string[] Palette =
    [
        "#530c73",
        "#894c5a",
        "#291d8f",
        "#8d2b5b",
        "#3c5d7a",
        "#95710f",
        "#2a7f62",
        "#7060b3",
        "#b85c2f",
        "#4a7c3f"
    ];

    public static List<string> BuildSeriesColors(int count)
    {
        var colors = new List<string>(Math.Max(count, 1));
        if (count <= 0)
        {
            colors.Add(Palette[0]);
            return colors;
        }

        for (var i = 0; i < count; i++)
        {
            colors.Add(Palette[i % Palette.Length]);
        }

        return colors;
    }

    public static string ResolveHistogramColor(string colorClass)
        => colorClass == "histogram-fill--secondary"
            ? "#6750a4"
            : "#006492";

    public static ApexChartOptions<TItem> BuildBarOptions<TItem>(
        IReadOnlyList<string> colors,
        IReadOnlyList<string>? categories,
        bool showLegend)
        where TItem : class
    {
        var options = new ApexChartOptions<TItem>
        {
            Chart = new Chart
            {
                Toolbar = new Toolbar { Show = false },
                Zoom = new Zoom { Enabled = false },
                Type = ChartType.Bar
            },
            PlotOptions = new PlotOptions
            {
                Bar = new PlotOptionsBar
                {
                    ColumnWidth = "55%",
                    BorderRadius = 6,
                    Horizontal = false
                }
            },
            DataLabels = new DataLabels { Enabled = false },
            Xaxis = new XAxis
            {
                Categories = categories?.ToList(),
                AxisBorder = new AxisBorder { Show = false },
                AxisTicks = new AxisTicks { Show = false },
                Labels = new XAxisLabels { Trim = true, Rotate = -35 }
            },
            Yaxis = new List<YAxis>
            {
                new YAxis
                {
                    Min = 0,
                    AxisBorder = new AxisBorder { Show = false },
                    AxisTicks = new AxisTicks { Show = false }
                }
            },
            Colors = colors.ToList(),
            Tooltip = new Tooltip { Theme = Mode.Light },
            Legend = showLegend
                ? new Legend
                {
                    Show = true,
                    Position = LegendPosition.Top,
                    HorizontalAlign = Align.Center,
                    FontSize = "12px"
                }
                : new Legend { Show = false }
        };

        return options;
    }

    public static ApexChartOptions<TItem> BuildDonutOptions<TItem>(
        IReadOnlyList<string> colors,
        IReadOnlyList<string> labels,
        string totalLabel)
        where TItem : class
    {
        return new ApexChartOptions<TItem>
        {
            Chart = new Chart
            {
                Toolbar = new Toolbar { Show = false },
                Zoom = new Zoom { Enabled = false },
                Type = ChartType.Donut
            },
            Labels = labels.ToList(),
            PlotOptions = new PlotOptions
            {
                Pie = new PlotOptionsPie
                {
                    Donut = new PlotOptionsDonut
                    {
                        Size = "58%",
                        Labels = new DonutLabels
                        {
                            Show = true,
                            Name = new DonutLabelName { Show = false },
                            Value = new DonutLabelValue { Show = false },
                            Total = new DonutLabelTotal
                            {
                                Show = true,
                                Label = totalLabel,
                                FontFamily = "var(--font-display)",
                                FontSize = "16px",
                                Formatter = "function (w) { return w.globals.seriesTotals.reduce((a, b) => a + b, 0).toFixed(0); }"
                            }
                        }
                    }
                }
            },
            DataLabels = new DataLabels { Enabled = false },
            Colors = colors.ToList(),
            Legend = new Legend
            {
                Show = true,
                Position = LegendPosition.Bottom,
                HorizontalAlign = Align.Center,
                FontSize = "12px",
                ItemMargin = new LegendItemMargin { Horizontal = 10, Vertical = 4 }
            },
            Tooltip = new Tooltip { Theme = Mode.Light }
        };
    }
}
