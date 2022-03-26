module ImageSorter.CreationDateHelpers

open SixLabors.ImageSharp
open SixLabors.ImageSharp.Metadata.Profiles.Exif
open System
open System.IO
open System.Text.RegularExpressions

let private tryGetExifValue (image: Image) exifTag =
    image.Metadata |> Option.ofObj
    |> Option.bind (fun m -> m.ExifProfile |> Option.ofObj)
    |> Option.bind (fun m -> m.GetValue exifTag |> Option.ofObj)
    |> Option.bind (fun m -> m.Value |> Option.ofObj)

let private tryGetCreationDateFromImage (path: string) =
    try
        use image = Image.Load(path)

        [
            ExifTag.DateTime
            ExifTag.DateTimeOriginal
            ExifTag.DateTimeDigitized
        ]
        |> List.map (tryGetExifValue image)
        |> List.tryPick id
        |> Option.map (fun date -> DateTime.ParseExact (date, "yyyy\\:MM\\:dd HH\\:mm\\:ss", System.Globalization.CultureInfo.InvariantCulture))
    with
    | :? UnknownImageFormatException -> None


let private dateTimeRegex = Regex (@"_\d{8}_\d{6}", RegexOptions.Compiled)

let private tryGetCreationTimeFromFilename (path: string) =
    let filename = Path.GetFileName path
    let rmatch = dateTimeRegex.Match filename

    match rmatch.Success with
    | true -> Some <| DateTime.ParseExact (rmatch.Value, "_yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture)
    | false -> None

let tryGetCreationDate path =
    seq {
        yield tryGetCreationDateFromImage path
        yield tryGetCreationTimeFromFilename path
    }
    |> Seq.tryPick id
