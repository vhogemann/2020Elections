#load "./Camara.fsx"
#r "System.Security.Cryptography"

open System
open System.Text
open System.Security.Cryptography

let YEAR_RANGE = seq { for i in 2023 .. 2023
 do yield i }


open System

let baseUrl = "https://camaradosdeputadosgql-qboe6n5gda-uc.a.run.app/graphql"
let camara = Camara.Client(baseUrl)

type DeputyDocument = {
    legislatureId: string
    legislatureStart: DateTime
    legislatureEnd: DateTime
    deputyId: int
    deputyName: string
    deputyParty: string
    deputyState: string
    deputyPicture: string
    expenseNetValue: decimal
    expenseDocumentDate: DateTime
    expenseSupplierCnpjOrCpf: string
    expenseSupplierName: string
    expenseType: string
} with
    member this.id
        with get() = 
            let md5 = MD5.Create()
            let bytes = Encoding.UTF8.GetBytes($"{this.deputyId}-{this.legislatureId}-{this.expenseType}-{this.expenseSupplierCnpjOrCpf}-{this.expenseDocumentDate}")
            Guid(md5.ComputeHash(bytes)).ToString()
    static member Create (legislature:Model.Legislature) (deputy:Model.Deputy) (expense:Model.Expense) =
        {
            legislatureId = legislature.id
            legislatureStart = legislature.start
            legislatureEnd = legislature.``end``
            deputyId = deputy.id
            deputyName = deputy.name
            deputyParty = deputy.party
            deputyState = deputy.state
            deputyPicture = deputy.picture
            expenseNetValue = expense.netValue
            expenseDocumentDate = expense.documentDate.Value
            expenseSupplierCnpjOrCpf = expense.supplierCnpjOrCpf
            expenseSupplierName = expense.supplierName
            expenseType = expense.expenseType
        }

let fetchDeputyExpenses (deputyId:int) (year:int) (month:Nullable<int>) =
    match camara.GetDeputyExpenses(deputyId, year, month).deputies with
    | [||] -> None
    | deputies -> 
        match deputies.[0].expenses with
        | [||] -> None
        | _ -> Some deputies.[0]

let fetchExpensesForYear (year:int) =
    let datetime = DateTime(year, 1, 1)
    camara.GetLegislature(datetime).legislatures
    |> Seq.map (fun legislature ->
        printfn "Fetching deputies for legislature %s" legislature.id
        legislature.deputies
        |> Seq.map(fun deputy -> 
            printfn $"Fetching expenses for { deputy.name } {deputy.id}"
            fetchDeputyExpenses deputy.id year (Nullable())
        )
        |> Seq.choose id
        |> Seq.filter (fun deputy -> deputy.expenses <> null && deputy.expenses.Length > 0)
        |> Seq.map (fun deputy -> 
            deputy.expenses
            |> Seq.filter (fun expense -> expense.documentDate.HasValue)
            |> Seq.map (fun expense -> DeputyDocument.Create legislature deputy expense)
        )
        |> Seq.concat
    )
    |> Seq.concat

// Export to CSV
// let header = "id,legislatureId,legislatureStart,legislatureEnd,deputyId,deputyName,deputyParty,deputyState,deputyPicture,expenseNetValue,expenseDocumentDate,expenseSupplierCnpjOrCpf,expenseSupplierName,expenseType"
// let file: IO.StreamWriter = System.IO.File.CreateText("expenses.csv")
// file.WriteLine(header)
// for year in YEAR_RANGE do
//     for expense in fetchExpensesForYear(year) do
//         file.WriteLine($"{expense.id},{expense.legislatureId},{expense.legislatureStart},{expense.legislatureEnd},{expense.deputyId},{expense.deputyName},{expense.deputyParty},{expense.deputyState},{expense.deputyPicture},{expense.expenseNetValue},{expense.expenseDocumentDate},{expense.expenseSupplierCnpjOrCpf},{expense.expenseSupplierName},{expense.expenseType}")
//         file.Flush()
// file.Close()