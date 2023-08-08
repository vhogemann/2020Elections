open System

type Expense = {
    netValue: decimal
    documentDate: Nullable<DateTime>
    supplierCnpjOrCpf: string
    supplierName: string
    expenseType: string
}
type Deputy = {
    id: int
    name: string
    party: string
    state: string
    picture: string
    expenses: Expense[]
}
type DeputyResponse = {
    deputies: Deputy[]
}
type Legislature = {
    id: string
    start: DateTime
    ``end``: DateTime
    deputies: Deputy[]
}
type LegislatureResponse = {
    legislatures: Legislature[]
}