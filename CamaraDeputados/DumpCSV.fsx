#load "Export.fsx"

open System

let YEAR_RANGE = seq { for i in 2000 .. 2023
 do yield i }

// Export to CSV
let header = "id,legislatureId,legislatureStart,legislatureEnd,deputyId,deputyName,deputyParty,deputyState,deputyPicture,expenseNetValue,expenseDocumentDate,expenseSupplierCnpjOrCpf,expenseSupplierName,expenseType"
for year in YEAR_RANGE do
    let file: IO.StreamWriter = System.IO.File.CreateText($"csv/expenses-{year}.csv")
    file.WriteLine(header)
    for expense in Export.fetchExpensesForYear(year) do
        file.WriteLine($"{expense.id},{expense.legislatureId},{expense.legislatureStart},{expense.legislatureEnd},{expense.deputyId},{expense.deputyName},{expense.deputyParty},{expense.deputyState},{expense.deputyPicture},{expense.expenseNetValue},{expense.expenseDocumentDate},{expense.expenseSupplierCnpjOrCpf},{expense.expenseSupplierName},{expense.expenseType}")
        file.Flush()
    file.Close()