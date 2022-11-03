#r "nuget: GraphQL.Client"
#r "nuget: GraphQL.Client.Serializer.Newtonsoft"

open GraphQL
open GraphQL.Client.Http
open GraphQL.Client.Serializer.Newtonsoft
open System

type ExpenseRequestParams =
    { date: string
      month: int
      year: int
      limit: int
      offset: int }

let expenseRequest (variables: ExpenseRequestParams) =
    new GraphQLRequest(
        $"""
    query {{
        legislatures(date: "{variables.date}") {{
            deputies(limit: {variables.limit}, offset: {variables.offset}) {{
            name
            state
            party
            picture
            expenses(month: "{variables.month}", year: "{variables.year}") {{
                supplierName
                documentValue
                documentDate
                expenseType
            }}
            }}
        }}
    }}
    """,
        Variables = variables
    )

type LegislatureResponse = { legislatures: Legislature list }
and Legislature = { deputies: Deputy list }

and Deputy =
    { name: string
      party: string
      expenses: Expense [] }

and Expense =
    { netValue: decimal
      supplierName: string
      documentDate: Nullable<DateTime> }

type ApiError =
    { errors: GraphQLError array option
      ex: Exception option }

let getExpenses (host: string) (date: DateTime) limit offset =
    let graphQLClient = new GraphQLHttpClient(host, new NewtonsoftJsonSerializer())

    let variables =
        { date = date.ToString("yyyy-MM-dd")
          month = date.Month
          year = date.Year
          limit = limit
          offset = offset }

    try
        let response: GraphQLResponse<LegislatureResponse> =
            graphQLClient.SendQueryAsync<LegislatureResponse>(expenseRequest variables)
            |> Async.AwaitTask
            |> Async.RunSynchronously

        if response.Errors <> null
           && response.Errors.Length > 0 then
            { errors = Some(response.Errors)
              ex = None }
            |> Error
        else
            Ok(response.Data)

    with
    | e -> { errors = None; ex = Some e } |> Error
