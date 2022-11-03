#r "nuget: QuestPDF"
#r "nuget: Microcharts"

open QuestPDF.Fluent
open QuestPDF.Helpers
open QuestPDF.Infrastructure
open Microcharts
open SkiaSharp

let entry value label valueLabel color =
    ChartEntry(
        value,
        Label = label,
        ValueLabel = valueLabel,
        Color = SKColor.Parse(color)
    )

let barChart entries =
     BarChart (
        Entries = entries,
        LabelOrientation = Orientation.Horizontal,
        ValueLabelOrientation = Orientation.Horizontal,
        IsAnimated = false
    )

let preview (title:string) (chart:Chart) =
    Document.Create( fun container -> 
        container.Page( fun page ->
            page.Size(PageSizes.A4)
            page.Margin(2.0F, Unit.Centimetre)
            page.DefaultTextStyle(fun style -> style.FontSize(20.0F))
            
            let drawOnCanvas canvas (size:Size) =
                let height = size.Height |> Convert.ToInt32
                let width = size.Width |> Convert.ToInt32
                chart.DrawContent(canvas, height, width)

            page.Content()
                .PaddingVertical(1.0F, Unit.Centimetre)
                .Column(fun col ->
                    col
                        .Item()
                        .PaddingBottom(10.0F)
                        .Text(title) |> ignore

                    col
                        .Item()
                        .Border(1.0F)
                        .ExtendHorizontal()
                        .Height(600.0F)
                        .Canvas(drawOnCanvas)
                )
            ) |> ignore)