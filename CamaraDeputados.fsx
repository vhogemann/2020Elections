#r "nuget: GraphQL.Client"
#r "nuget: GraphQL.Client.Serializer.Newtonsoft"

open GraphQL
open GraphQL.Client.Http
open GraphQL.Client.Serializer.Newtonsoft
open System

let graphQLClient = new GraphQLHttpClient("http://localhost:5078/graphql", new NewtonsoftJsonSerializer());

let deputyRequest party state year = new GraphQLRequest (
    $"""
    query {{
        deputies (party: "{party}", state: "{state}") {{
            name
            party
            expenses (year: "{year}") {{
                netValue
                documentDate
            }}
        }}
    }}
    """
)

let expenseRequest name year = new GraphQLRequest (
    $"""
    query {{
        deputies (name: "{name}") {{
            name
            party
            state
            expenses (year: "{year}") {{
                netValue
                documentDate
                supplierCnpjOrCpf
                supplierName
            }}
        }}
    }}
    """
)

type DeputyResponse = {
    deputies: Deputy list
} 
and Deputy = {
    name: string
    party: string
    expenses: Expense[]
}
and Expense = {
    netValue: decimal
    supplierName: string
    documentDate: Nullable<DateTime>
}

let GetDeputies party state year =
    let response =
        graphQLClient.SendQueryAsync<DeputyResponse>(deputyRequest party state year)
        |> Async.AwaitTask
        |> Async.RunSynchronously
    response.Data.deputies

