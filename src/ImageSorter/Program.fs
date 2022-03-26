module ImageSorter.Program

open CommandLineArguments
open CreationDateHelpers
open System.IO

let setCultureInfo () =
    let invCult = System.Globalization.CultureInfo.InvariantCulture
    System.Globalization.CultureInfo.CurrentCulture <- invCult
    System.Globalization.CultureInfo.CurrentUICulture <- invCult
    System.Globalization.CultureInfo.DefaultThreadCurrentCulture <- invCult
    System.Globalization.CultureInfo.DefaultThreadCurrentUICulture <- invCult

let enumerateSourceFiles (args: CArgs) =
    let isRecursive = args.Contains Recursive

    let rec enumFiles path =
        match Directory.Exists path with
        | true when isRecursive -> Seq.collect enumFiles <| Directory.EnumerateFiles (path, "*", SearchOption.AllDirectories)
        | true -> Seq.empty
        | false ->
            match File.Exists path with
            | true -> Seq.singleton path
            | false ->
                eprintf $"Bad path \"%s{path}\". Ignoring."
                Seq.empty

    Seq.ofList (args.GetResult Paths)
    |> Seq.collect enumFiles

let processFile (args: CArgs) sourceFile =
    let destFile =
        match tryGetCreationDate sourceFile with
        | Some date ->
            FileHelpers.dateTimeToFilePath date (Path.GetExtension sourceFile)
        | None ->
            Path.Combine ("UnknownCreationDate", Path.GetFileName sourceFile)

    let destFile = Path.Combine (getDestinationDir args, destFile)
    FileHelpers.safeCopyFile sourceFile destFile

[<EntryPoint>]
let main argv =
    setCultureInfo ()
    let argv = parseArguments argv
    printfn $"Got %d{List.length (argv.GetResult Paths)} files to process."
    printfn $"Destination dir is \"%s{getDestinationDir argv}\"."

    enumerateSourceFiles argv
    |> Seq.iter (processFile argv)

    printfn "Done."
    0
