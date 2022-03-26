module ImageSorter.FileHelpers

open System
open System.IO
open System.Security.Cryptography

let dateTimeToFilePath (dt: DateTime) fileExtension =
    let day = dt.Day.ToString "00"
    let hour = dt.Hour.ToString "00"
    let minute = dt.Minute.ToString "00"
    let second = dt.Second.ToString "00"
    let filename = sprintf $"%s{day}d_%s{hour}h_%s{minute}m_%s{second}s%s{fileExtension}"

    let year = dt.Year.ToString ()
    let month = dt.ToString "MM MMMM"

    Path.Combine (year, month, filename)

let computeFileHashAsync file =
    async {
        use hasher = SHA512.Create ()
        use stream = File.OpenRead file
        return! hasher.ComputeHashAsync stream |> Async.AwaitTask
    }

let areFilesEqual file1 file2 =
    let info1 = FileInfo file1
    let info2 = FileInfo file2

    if info1.Length <> info2.Length then false
    else
        let hashes =
            [
                computeFileHashAsync file1
                computeFileHashAsync file2
            ]
            |> Async.Parallel
            |> Async.RunSynchronously

        hashes[0] = hashes[1]

let getUniqueName (file: string) =
    let extension = Path.GetExtension file
    let fileWithoutExt = Path.Combine (Path.GetDirectoryName file, Path.GetFileNameWithoutExtension file)

    let rec producePaths (suffix: int) =
        seq {
            let suffixS = suffix.ToString "00"

            yield sprintf $"%s{fileWithoutExt}_%s{suffixS}%s{extension}"
            yield! producePaths (suffix + 1)
        }

    producePaths 0
    |> Seq.find (fun f -> not (File.Exists f))

let safeCopyFile (sourceFile: string) (destFile: string) =
    let destinationDir = Path.GetDirectoryName destFile

    Directory.CreateDirectory destinationDir |> ignore

    match File.Exists destFile with
    | false -> File.Copy (sourceFile, destFile)
    | true ->
        match areFilesEqual sourceFile destFile with
        | true -> ()
        | false ->
            let destFile = getUniqueName destFile
            File.Copy (sourceFile, destFile)
