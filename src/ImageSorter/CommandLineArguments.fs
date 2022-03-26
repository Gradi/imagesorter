module ImageSorter.CommandLineArguments

open Argu

type CommandArgs =
    | [<AltCommandLine("-r")>] Recursive
    | DestinationDir of destinationDir: string
    | [<MainCommand; ExactlyOnce>] Paths of paths: string list

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Recursive -> "If path is non empty directory process it recursively."
            | DestinationDir _ -> "Destination directory to put sorted files in otherwise it is current working directory."
            | Paths _ -> "One or more paths to files which to be sorted."

type CArgs = ParseResults<CommandArgs>

let parseArguments argv : CArgs = ArgumentParser.Create<CommandArgs>().Parse argv

let getDestinationDir (args: CArgs) =
    match args.TryGetResult DestinationDir with
    | Some dir -> dir
    | None -> System.IO.Directory.GetCurrentDirectory ()

