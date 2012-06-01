Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.IO
Imports System.Security.Cryptography

''' <summary>
''' Console application to display hash values for file contents.
''' Also supports finding and displaying files with identical hashes
''' </summary>
''' <remarks>
''' Return values:
''' 
''' 0 - OK
''' 1- Argument error
''' 2 - Run time error
''' </remarks>
Module MOBZHash
  Function Main(arguments() As String) As Integer
    ' Construct a title
    Dim title As String = String.Format(
      "{0} v{1} ({2}-bit) by MOBZystems - http://www.mobzystems.com/Tools/MOBZHash",
      My.Application.Info.Title,
      My.Application.Info.Version.ToString(3),
      IntPtr.Size * 8
    )

    Try

      ' If no arguments specified, print usage
      If arguments.Length = 0 Then
        Console.WriteLine(title)
        Console.WriteLine()
        Console.WriteLine("Usage:")
        Console.WriteLine()
        Console.WriteLine("MOBZHash [-v] [-r] [-d] hash-type [file-spec [file-spec ...]]")
        Console.WriteLine()
        Console.WriteLine("hash-type: hash type to use. All .NET hash algorihms are supported")
        Console.WriteLine("           Examples: MD5, SHA1 (default), SHA256, SHA384, SHA512")
        Console.WriteLine("file-spec: A file name or wildcard. Defaults to all files in current directory")
        Console.WriteLine()
        Console.WriteLine("Switches:")
        Console.WriteLine("-v: verbose")
        Console.WriteLine("-r: recursive")
        Console.WriteLine("-d: find duplicates. Display a list of all files with an identical hash")

        ' No error
        Return 0
      End If

      ' Parse the command line

      ' The first non-switch argument is the hash name
      Dim hashName As String = Nothing
      ' List of file specs
      Dim fileSpecs As New List(Of String)
      ' List of switches
      Dim switches As New List(Of String)

      ' Separate switches from arguments, isolate hash name
      For Each arg As String In arguments
        If arg.StartsWith("-") OrElse arg.StartsWith("/") Then
          switches.Add(arg.Substring(1).ToUpperInvariant())
        ElseIf hashName Is Nothing Then
          hashName = arg.ToUpperInvariant()
        Else
          fileSpecs.Add(arg)
        End If
      Next

      ' Use SHA1 if no hash specified
      If hashName Is Nothing Then
        hashName = "SHA1"
      End If

      ' See if we can create a hash algorithm for the specified hash name
      Dim hasher As HashAlgorithm = Nothing
      Try
        hasher = DirectCast(CryptoConfig.CreateFromName(hashName), HashAlgorithm)
      Catch ex As Exception
        ' We get an exception if the hash exists, but is not a hash algorithm (e.g. RSA)
        Console.Error.WriteLine(
          String.Format("Invalid hash type '{0}'. Valid hash types are MD5, SHA1, " +
                        "SHA256, SHA256, SHA384, SHA512", hashName))
        ' Argument error
        Return 1
      End Try

      ' Did we fail silently? (Hash name is unknown)
      If hasher Is Nothing Then
        Console.Error.WriteLine(String.Format("Uknown hash type '{0}'", hashName))
        ' Argument error
        Return 1
      End If

      ' Parse the switches:

      Dim verbose As Boolean = False
      Dim recursive As Boolean = False
      Dim duplicates As Boolean = False

      For Each switch As String In switches
        Select Case switch
          Case "V"
            verbose = True
          Case "R"
            recursive = True
          Case "D"
            duplicates = True
          Case Else
            Console.Error.WriteLine("Invalid switch '{0}'", switch)
            ' Argument error
            Return 1
        End Select
      Next

      ' Process all file specifications

      ' Provide a default file spec if we didn't supply one
      If fileSpecs.Count = 0 Then
        fileSpecs.Add("*.*")
      End If

      ' Build a dictionary of hashes if we are looking for duplicates
      Dim hashes As Dictionary(Of String, List(Of String)) = Nothing

      If duplicates Then
        hashes = New Dictionary(Of String, List(Of String))
      End If

      If verbose Then
        Console.WriteLine(title)
        Console.WriteLine()
      End If

      ' Loop over the file specs
      For Each fileSpec In fileSpecs
        Dim pathName As String
        Dim fileMask As String

        ' Did we specify a directory?
        If IO.Directory.Exists(fileSpec) Then
          ' Use the name of the directory
          pathName = fileSpec
          ' and a file mask of *.*
          fileMask = "*.*"
        Else
          ' Separate directory from file mask
          pathName = Path.GetDirectoryName(fileSpec)
          fileMask = Path.GetFileName(fileSpec)

          ' Did we spcify a file name only?
          If String.IsNullOrEmpty(pathName) Then
            ' Use the current directory
            pathName = Environment.CurrentDirectory
          End If
        End If

        ' Show some text in verbose mode
        If verbose Then
          Console.WriteLine(
            String.Format(
              "{0} {1} hash for {2}{3}...",
              If(duplicates, "Finding duplicates by", "Computing"),
              hashName,
              fileSpec,
              If(recursive, " and subdirectories", "")
            )
          )
        End If

        ' Find all files corresponding to the file mask in the specified path,
        ' possibly recursive
        Dim dir As New DirectoryInfo(pathName)
        Dim files() As FileInfo = Nothing

        Try
          files = dir.GetFiles(
            fileMask,
            If(recursive, SearchOption.AllDirectories, SearchOption.TopDirectoryOnly)
          )
        Catch ex As Exception
          Console.Error.WriteLine(
            String.Format(
              "Could not access '{0}': {1}",
              pathName,
              ex.Message
            )
          )
        End Try

        If files IsNot Nothing Then
          ' Process all files
          For Each file As FileInfo In files

            ' Skip hidden files
            If (file.Attributes And FileAttributes.Hidden) = 0 Then

              ' Set up the file name to display
              Dim relativeFileName As String = file.FullName.Substring(dir.FullName.Length + 1)

              ' Use the relative name for display purposes if we have a single file spec.
              ' Otherwise, use the full name of the file
              Dim displayName As String = relativeFileName
              If fileSpecs.Count > 1 Then
                displayName = file.FullName
              End If

              Try

                Using stream As Stream = System.IO.File.OpenRead(file.FullName)
                  ' Compute the hash on the stream of the file
                  Dim hash As Byte() = hasher.ComputeHash(stream)
                  ' Convert the hash to a string
                  Dim hashString As String = BitConverter.ToString(hash)

                  ' Are we collecting duplicates?
                  If duplicates Then

                    ' Store the hash and the file name in the hashes
                    Dim fileList As List(Of String) = Nothing

                    ' Do we know this hash?
                    If hashes.TryGetValue(hashString, fileList) Then

                      ' Add this file if it isn't in the list already (case insensitive!)
                      If Not fileList.Contains(
                        displayName,
                        StringComparer.InvariantCultureIgnoreCase
                      ) Then
                        fileList.Add(displayName)
                      End If

                    Else

                      ' New hash - create a new file list
                      fileList = New List(Of String)
                      ' Add this file
                      fileList.Add(displayName)
                      ' Store in hashes
                      hashes.Add(hashString, fileList)

                    End If

                  Else

                    ' Not finding duplicates - simply list hash and file name
                    Console.WriteLine(String.Format("{0}: {1}", hashString, displayName))

                  End If

                End Using

              Catch ex As Exception
                Console.Error.WriteLine(
                  String.Format(
                    "Error computing {0} hash for file '{1}': {2}",
                    hashName,
                    displayName,
                    ex.Message
                  )
                )

              End Try

            End If ' Not hidden

          Next file

        End If ' If no error getting files

      Next fileSpec

      ' We're done processing all file specs.
      ' For duplicates, display all lists containing more than one item:
      If duplicates Then
        ' Loop over all hashes
        For Each hashString As String In hashes.Keys
          ' Get the list of files with this hash
          Dim fileList As List(Of String) = hashes(hashString)
          ' More than one? Display them
          If fileList.Count > 1 Then
            Console.WriteLine("{0} {1}: {2} files", hashName, hashString, fileList.Count)
            For Each filename In fileList
              Console.WriteLine(String.Format("- {0}", filename))
            Next
          End If
        Next
      End If

      ' OK
      Return 0

    Catch ex As Exception
      Console.Error.WriteLine(
        String.Format(
          "Hash: Generic error: {0}",
          ex.Message
        )
      )

      ' Run-time eror
      Return 2
    End Try
  End Function
End Module
