#load "./Dump.fsx"
#r "nuget: Elastic.Clients.Elasticsearch"

open Elastic.Clients.Elasticsearch
open System

let INDEX_NAME = "deputies"
let elasticsearch = ElasticsearchClient()

// Create index if it doesn't exist
if not (elasticsearch.Indices.Exists(INDEX_NAME).Exists) then
    let response = 
        elasticsearch.Indices.CreateAsync(INDEX_NAME)
        |> Async.AwaitTask
        |> Async.Catch
        |> Async.RunSynchronously
    match response with
    | Choice1Of2 _ -> printfn $"Index {INDEX_NAME} created"
    | Choice2Of2 ex -> failwith ex.Message
else
    printfn $"Index {INDEX_NAME} already exists"

//bulk insert expenses
for year in Dump.YEAR_RANGE do
    let expenses = Dump.fetchExpensesForYear year
    printfn $"Inserting expenses for year {year}"
    elasticsearch.BulkAll(expenses, fun bulk ->
        bulk
            .Index(INDEX_NAME)
            .BackOffTime("30s")
            .BackOffRetries(2)
            .RefreshOnCompleted()
            .MaxDegreeOfParallelism(Environment.ProcessorCount)
            .Size(100) |> ignore
    ).Wait(
        TimeSpan.FromMinutes(5), 
        fun next -> 
            printfn $"Inserting page {next.Page}"
            ()
    ) |> ignore