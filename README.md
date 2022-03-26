# ImageSorter

This is small tool i use to sort my photos from smartphone.

It accepts input path(s) to files or directories (directories can be
traversed recursively) and destination directory. Input paths can
contain pictures or any other file types, but mostly pictures and a
little bit of videos. Next, for each file, tool

1. Attempts to get creation date
1. Copies file to `destinationDir\year\month\day_hour_minute_seconds.ext`
1. If destination already contains file:

    - If thoose file are the same (comparison done with `SHA512`) --
    do nothing
    - Otherwise, rename source file to
    `originalFileNameWithoutExtension_{numericSuffix}.originalExtension`
    and attempt to copy file again.

## How creation date is obtained

- Try open image file with `SixLabors.ImageSharp` and get date from
  metadata.

  or

- Match filename with `_\d{8}_\d{6}` regex and, in case of match,
  parse date with `_yyyyMMdd_HHmmss` format.

Filenames examples:

```
P_20220210_123843_vHDR_On.jpg
P_20211231_174637_vHDR_On.jpg
V_20210806_164814_vHDR_On.mp4
```

That's how my phone names pictures.

If both ways to get date fails, file is copied to
`destinationDir\UnknownCreationDate` dir (again, in case of
destination already contains same file, check for duplicate and/or copy
under different name).

## Building

```
cd .\src\ImageSorter\

dotnet build ImageSorter.fsproj -c Release -p:Platform=x64

.\bin\x64\Release\net6.0\ImageSorter.exe --help
```

## Usage

```
USAGE: ImageSorter.exe [--help] [--recursive] [--destinationdir <destinationDir>] <paths>...

PATHS:

    <paths>...            One or more paths to files which to be sorted.

OPTIONS:

    --recursive, -r       If path is non empty directory process it recursively.
    --destinationdir <destinationDir>
                          Destination directory to put sorted files in otherwise it is current working directory.
    --help                display this list of options.
```
