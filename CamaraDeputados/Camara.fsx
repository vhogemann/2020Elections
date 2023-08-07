#r "nuget: GraphQL.Client"
#r "nuget: GraphQL.Client.Serializer.Newtonsoft"
#r "nuget: Plotly.NET"
#r "nuget: Plotly.NET.ImageExport"
#load "./Model.fsx"

open GraphQL
open GraphQL.Client.Http
open GraphQL.Client.Serializer.Newtonsoft
open System
open Model

type Client(baseUrl:string) =
    let graphQLClient = new GraphQLHttpClient(baseUrl, new NewtonsoftJsonSerializer())

    let performQuery (query:string) (variables: 'V):'T =
        let request = GraphQLRequest(query, variables)
        graphQLClient.SendQueryAsync<'T>(request)
        |> Async.AwaitTask
        |> Async.Catch
        |> Async.RunSynchronously
        |> function
            | Choice1Of2 response -> response.Data
            | Choice2Of2 ex -> failwith ex.Message

    member _.GetLegislature(date:DateTime):LegislatureResponse =
        let query = """
            query ($date: DateTime) {
            legislatures(date: $date) {
                id
                start
                end
                deputies {
                id
                name
                party
                state
                picture
                }
            }
            }
        """
        //Date format 2023-04-22
        let formattedDate = date.ToString("yyyy-MM-dd")
        performQuery query {| date = formattedDate |}

    member _.GetDeputyExpenses(deputyId:int, year: int, month: int): DeputyResponse =
        let query = """
            query($deputyId: Int! $month: Int! $year: Int!){
                deputies(id: $deputyId) {
                    id
                    name
                    party
                    state
                    picture
                    expenses(month: $month year: $year) {
                        netValue
                        documentDate
                        supplierCnpjOrCpf
                        supplierName
                        expenseType
                    }
                }
            }
        """
        let variables = {| 
            deputyId = deputyId 
            month = month
            year = year
        |}
        performQuery query variables

module Graph =
    open Plotly.NET
    open Plotly.NET.ImageExport

    let stackedArea (deputy:Model.Deputy) =
        deputy.expenses
        |> Seq.groupBy (fun e -> e.expenseType)
        |> Seq.map(fun (expenseType, expenses) -> 
            let dates, values =
                expenses
                |> Seq.filter(fun it -> it.documentDate.HasValue)
                |> Seq.sortBy(fun it -> it.documentDate.Value)
                |> Seq.map(fun it -> it.documentDate.Value, it.netValue)
                |> Seq.toArray
                |> Array.unzip
            Chart.Line(dates, values, Name=expenseType)
        )
        |> Chart.combine
        |> Chart.withLegendStyle(Orientation = StyleParam.Orientation.Horizontal)
        |> Chart.withLayoutStyle(Width = 1000, Height = 400)

    let asSVG graph =
        graph
        |> Chart.toSVGString( Width = 800, Height = 600)
