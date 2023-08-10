#load "./Dump.fsx"
#r "nuget: Elastic.Clients.Elasticsearch, 7.13.0"

open Elastic.Clients.Elasticsearch

let INDEX_NAME = "deputies"
let elasticsearch = ElasticsearchClient()

let lazyCreateIndex (index:string) =
    // Create index if it doesn't exist
    if not (elasticsearch.Indices.Exists(index).Exists) then
        elasticsearch.Indices.CreateAsync(index)
        |> Async.AwaitTask
        |> Async.Catch
        |> Async.RunSynchronously
        |> function
        | Choice1Of2 response -> 
            if response.Acknowledged then
                printfn $"Index {index} created"
            else
                failwith response.DebugInformation
        | Choice2Of2 ex -> failwith ex.Message
    else
        printfn $"Index {index} already exists"

let year = 2021
let expenses = Dump.fetchExpensesForYear year
let indexName = $"{INDEX_NAME}-{year}"
lazyCreateIndex indexName
printfn $"Inserting expenses for year {year}"
for expense in expenses do
    printfn $"Inserting expense {expense.id}"
    let response = elasticsearch.Index(expense, indexName)
    if response.IsValidResponse then
        printfn $"Inserted expense {expense.id}"
    else
        printfn "%s" response.DebugInformation
        