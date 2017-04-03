# MOBZHash
MOBZHash shows MD5 or SHA hash values for files, and reports files with identical hashes (which are most likely duplicates).
All documentation can be found at [MOBZystems, Home of Tools](https://www.mobzystems.com/Tools/MOBZHash)

In a nutshell:

```
MOBZHash v1.0.1 (64-bit) by MOBZystems - http://www.mobzystems.com/Tools/MOBZHash

Usage:

MOBZHash [-v] [-r] [-d] hash-type [file-spec [file-spec ...]]

hash-type: hash type to use. All .NET hash algorihms are supported
           Examples: MD5, SHA1 (default), SHA256, SHA384, SHA512
file-spec: A file name or wildcard. Defaults to all files in current directory

Switches:
-v: verbose
-r: recursive
-d: find duplicates. Display a list of all files with an identical hash}}

Sample output, showing SHA1 hashes of files in the current directory:

{{5D-BD-3D-03-59-38-DE-C5-B0-7E-E4-78-9A-B1-14-93-6D-2D-E6-68: MOBZHash - Copy.xml
11-F7-84-54-EC-57-63-77-36-F3-EC-EE-23-A7-70-C9-64-3D-FF-3E: MOBZHash.exe
62-16-50-F9-F4-98-E0-7B-0D-BE-00-D2-BE-40-81-E1-2E-8E-EB-4A: MOBZHash.exe.config
71-A9-53-55-10-B0-FD-36-AA-11-1C-C1-80-48-3E-DB-8C-CD-48-BD: MOBZHash.pdb
27-35-2B-A0-E6-80-25-B6-03-38-37-2D-D8-1A-40-D2-B5-B5-5F-21: MOBZHash.vshost.exe
62-16-50-F9-F4-98-E0-7B-0D-BE-00-D2-BE-40-81-E1-2E-8E-EB-4A: MOBZHash.vshost.exe.config
78-20-EE-05-21-0E-10-AA-C4-CD-45-4F-C9-B4-26-F8-0E-6A-07-A2: MOBZHash.vshost.exe.manifest
5D-BD-3D-03-59-38-DE-C5-B0-7E-E4-78-9A-B1-14-93-6D-2D-E6-68: MOBZHash.xml
```

Output showing files with duplicate hashes:

```
SHA1 5D-BD-3D-03-59-38-DE-C5-B0-7E-E4-78-9A-B1-14-93-6D-2D-E6-68: 2 files
- MOBZHash - Copy.xml
- MOBZHash.xml
SHA1 62-16-50-F9-F4-98-E0-7B-0D-BE-00-D2-BE-40-81-E1-2E-8E-EB-4A: 2 files
- MOBZHash.exe.config
- MOBZHash.vshost.exe.config
```
