open System.IO

let load () =
    let envFile = Path.Combine(__SOURCE_DIRECTORY__, ".env")
    if File.Exists(envFile) then
        File.ReadAllLines(envFile)
        |> Array.map (fun line -> line.Split('='))
        |> Array.map (fun parts -> parts.[0], parts.[1])
        |> Map
    else
        Map []