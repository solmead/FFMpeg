﻿'/*
' * Copyright (C) 2009-2012 Solmead Productions
' *
' * == BEGIN LICENSE ==
' *
' * Licensed under the terms of any of the following licenses at your
' * choice:
' *
' *  - GNU General Public License Version 2 or later (the "GPL")
' *    http://www.gnu.org/licenses/gpl.html
' *
' *  - GNU Lesser General Public License Version 2.1 or later (the "LGPL")
' *    http://www.gnu.org/licenses/lgpl.html
' *
' *  - Mozilla Public License Version 1.1 or later (the "MPL")
' *    http://www.mozilla.org/MPL/MPL-1.1.html
' *
' * == END LICENSE ==
' */
Public Enum EncodingStateEnum
    Not_Encoding
    Audio_Only
    Video_Only
    Video_and_Audio
End Enum
Public Enum SoundTypeEnum
    None = 0
    Mono = 1
    Stereo = 2
End Enum
Public Enum VideoSubEncodingEnum
    None
    yuv420p

End Enum
Public Enum VideoEncodingEnum
    None
    mpeg2video
    mpeg4
    flv
    h264
    h264_iPod
    OGG_Theora
    WebM
End Enum
Public Enum AudioEncodingEnum
    None
    mp3
    ac3
    mp2
    aac
End Enum
Public Enum PassEnum
    One = 1
    Two = 2
End Enum
Friend Class TestRun
    Public Quality As Integer
    Public Passes As PassEnum
    Public BitRate As Integer
    Public MaxQuality As Integer
End Class
Public Class FileInfo
    Public Event State(ByVal PS As ProcessState)
    Public Event StartConversion(ByVal TotalPasses As PassEnum, ByVal CurrentPass As Integer)
    Public Event EndConversion()
    Public Event DebugMessage(ByVal Msg As String)

    Public Shared VideoList() As String = {"FLV", "MOD", "AVI", "MPG", "MPEG", "MOV", "WMV", "VOB", "VRO", "MTS", "QT", "SWF", "MP4", "M4V"}
    Public Shared ImageList() As String = {"JPG", "JPEG", "PNG", "BMP", "TGA", "GIF"}
    Public Shared AudioList() As String = {"MP3", "WAV", "OGG", "AAC", "WMA", "M4A"}


    Public Property File As System.IO.FileInfo
    Public Property Width As Integer = 600
    Public Property Height As Integer = 300
    Public Property BitRate As Integer = 1000

    Public Property Channels As SoundTypeEnum = SoundTypeEnum.Stereo

    Public Property AudioBitRate As Integer = 128

    Public Property Duration As Double = 0
    Public Property HasVideo As Boolean = False
    Public Property HasAudio As Boolean = False

    Public Property VideoEncoding As VideoEncodingEnum = VideoEncodingEnum.flv
    Public Property VideoSubEncoding As VideoSubEncodingEnum = VideoSubEncodingEnum.yuv420p
    Public Property AudioEncoding As AudioEncodingEnum = AudioEncodingEnum.None
    Public Property AudioFrequency As Integer = 44100

    Public Property FrameRate As Double = 24 '29.97
    Public PAR As String = "1:1"
    Public DAR As String = "16:9"
    Public QMax As Single = 5
    Public QMin As Single = 2
    Public NumberPasses As PassEnum = PassEnum.Two
    Public Deinterlace As Boolean = False

    Public MaxQualityFound As Integer = 0

    Public Property Stretch As Boolean = False

    Private WithEvents CMD As New ShellCommand

    Public Sub DebugMsg(ByVal Msg As String)
        RaiseEvent DebugMessage(Msg)
    End Sub

    Public Property FFMpegBaseDirectory As String

    Public Property EncodingState As EncodingStateEnum = EncodingStateEnum.Not_Encoding

    Public ReadOnly Property PAR_Value() As Double
        Get
            Try
                Dim Str As String() = Split(PAR, ":")

                Return Val(Str(0)) / Val(Str(1))
            Catch ex As Exception

            End Try
            Return 1
        End Get
    End Property
    Public ReadOnly Property DAR_Value() As Double
        Get
            Try
                Dim Str As String() = Split(DAR, ":")

                Return Val(Str(0)) / Val(Str(1))
            Catch ex As Exception

            End Try
            Return 16 / 9
        End Get
    End Property
    Public ReadOnly Property WidthAdjusted() As Integer
        Get
            If PAR_Value <= 1 Then
                Return Int(Width * PAR_Value)
            Else
                Return Width
            End If
        End Get
    End Property
    Public ReadOnly Property HeightAdjusted() As Integer
        Get
            If PAR_Value >= 1 Then
                Return Int(Height / PAR_Value)
            Else
                Return Height
            End If
        End Get
    End Property

    Public ReadOnly Property Extension() As String
        Get
            Dim Ext = File.Extension
            Dim FName As String = ""
            If Ext.Length > 0 Then
                FName = File.Name.Substring(0, File.Name.Length - File.Extension.Length)
                Ext = Mid(Ext, 2) ' "FLV"
                'FileName = FName
            End If
            If Ext Is Nothing Then Ext = ""
            Return Ext
        End Get
    End Property

    Public ReadOnly Property IsImage() As Boolean
        Get
            Return ImageList.Contains(Extension.ToUpper)
        End Get
    End Property
    Public ReadOnly Property IsVideo() As Boolean
        Get
            Return VideoList.Contains(Extension.ToUpper)
        End Get
    End Property
    Public ReadOnly Property IsAudio() As Boolean
        Get
            Return AudioList.Contains(Extension.ToUpper)
        End Get
    End Property

    Public Shared Function AudioFormatForEncoding(ByVal AudioEncoding As AudioEncodingEnum) As String
        Select Case AudioEncoding
            Case AudioEncodingEnum.mp3
                Return "libmp3lame"

            Case AudioEncodingEnum.mp2
                Return "mp2"

            Case AudioEncodingEnum.ac3
                Return "ac3"

            Case AudioEncodingEnum.aac
                Return "libfaac"

            Case Else
                Return "libmp3lame"
        End Select
    End Function
    Public Shared Function VideoFormatForEncoding(ByVal VideoEncoding As VideoEncodingEnum) As String
        Select Case VideoEncoding
            Case VideoEncodingEnum.h264
                Return "libx264"


            Case Else
                Return VideoEncoding.ToString
        End Select
    End Function



    Public ReadOnly Property FFMpegLocation() As String
        Get
            If FFMpegBaseDirectory = "" Then
                FFMpegBaseDirectory = My.Application.Info.DirectoryPath & "\ffmpeg\"
            End If
            Return FFMpegBaseDirectory
        End Get
    End Property
    Public ReadOnly Property FLVToolLocation() As String
        Get
            Return FFMpegBaseDirectory
        End Get
    End Property
    Public Sub New()

    End Sub
    Public Sub New(ByVal FileName As String)
        File = New System.IO.FileInfo(FileName)
        RefreshData()
    End Sub
    Public Sub New(ByVal FileInfo As System.IO.FileInfo)
        File = FileInfo
        RefreshData()
    End Sub

    Private Sub RefreshData()
        DebugMsg(FFMpegLocation)
        Dim TempDirectory = New System.IO.DirectoryInfo(File.Directory.FullName & "\Temp")
        If Not TempDirectory.Exists Then TempDirectory.Create()
        Dim FNWE = Left(File.Name, Len(File.Name) - (Len(File.Extension)))
        Dim TempVideoLogFile As New System.IO.FileInfo(TempDirectory.FullName & "\" & FNWE & "_Initial.log")

        If TempVideoLogFile.Exists Then
            TempVideoLogFile.Delete()
        End If
        DebugMsg("Getting Video Info")
        Dim InfoData = RunFFMpeg("-i """ & File.FullName & """")
        DebugMsg("Writing Info Data")
        Dim SW = New System.IO.StreamWriter(TempVideoLogFile.OpenWrite)
        SW.Write(InfoData)
        SW.Close()
        TempVideoLogFile.Refresh()
        DebugMsg("Processing Video Info")
        Dim Lines() As String = Split(InfoData, vbCrLf)

        For Each Line In Lines
            If Line.Contains("Duration:") Then
                Dim a = InfoData.IndexOf("Duration: ")
                a = a + "Duration: ".Length
                Dim b = InfoData.IndexOf(", start:", a)
                Dim Time = InfoData.Substring(a, b - a)
                Dim TS = TimeSpan.Parse(Time)
                Duration = TS.TotalMilliseconds / 1000
            End If
            If Line.Contains("Stream #") Then
                If Line.Contains("Video:") Then
                    Dim a = Line.IndexOf("Video: ")
                    a = a + "Video: ".Length
                    Dim Entries() As String = Split(Line.Substring(a), ",")
                    VideoEncoding = GetEnumData(GetType(VideoEncodingEnum), GetPosition(Entries, 0))
                    VideoSubEncoding = GetEnumData(GetType(VideoSubEncodingEnum), GetPosition(Entries, 1))
                    Dim Extents = GetPosition(Entries, 2)

                    Dim Tarr2() As String = Split(Extents, "x")
                    If Tarr2.Length >= 2 Then
                        Width = Val(Tarr2(0))
                        Height = Val(Tarr2(1))
                    End If
                    If GetPosition(Entries, 3).Contains("kb/s") Then
                        BitRate = Val(GetPosition(Entries, 3))
                    End If
                    If GetPosition(Entries, 3).Contains("tbr") Then
                        FrameRate = Val(GetPosition(Entries, 3))
                    ElseIf GetPosition(Entries, 4).Contains("tbr") Then
                        FrameRate = Val(GetPosition(Entries, 4))
                    End If
                    Try

                        If Extents.Contains("[") AndAlso Extents.Contains("]") Then
                            Dim b = Extents.IndexOf("[")
                            Dim c = Extents.IndexOf("]")

                            Dim PARDAR As String = Extents.Substring(b + 1, (c - 1) - (b + 1) + 1)
                            Dim PD As String() = Split(PARDAR, " ")
                            If PD(0).ToUpper = "PAR" Then
                                PAR = PD(1)
                            ElseIf PD(2).ToUpper = "PAR" Then
                                PAR = PD(3)
                            End If
                            If PD(0).ToUpper = "DAR" Then
                                DAR = PD(1)
                            ElseIf PD(2).ToUpper = "DAR" Then
                                DAR = PD(3)
                            End If
                        End If
                    Catch ex As Exception

                    End Try
                    If GetPosition(Entries, 3).ToUpper.Contains("PAR") Then
                        Dim Stuff = GetPosition(Entries, 3).Trim
                        Dim PD As String() = Split(Stuff, " ")
                        If PD(0).ToUpper = "PAR" Then
                            PAR = PD(1)
                        ElseIf PD(2).ToUpper = "PAR" Then
                            PAR = PD(3)
                        End If
                        If PD(0).ToUpper = "DAR" Then
                            DAR = PD(1)
                        ElseIf PD(2).ToUpper = "DAR" Then
                            DAR = PD(3)
                        End If
                    ElseIf GetPosition(Entries, 4).ToUpper.Contains("PAR") Then
                        Dim Stuff = GetPosition(Entries, 4).Trim
                        Dim PD As String() = Split(Stuff, " ")
                        If PD(0).ToUpper = "PAR" Then
                            PAR = PD(1)
                        ElseIf PD(2).ToUpper = "PAR" Then
                            PAR = PD(3)
                        End If
                        If PD(0).ToUpper = "DAR" Then
                            DAR = PD(1)
                        ElseIf PD(2).ToUpper = "DAR" Then
                            DAR = PD(3)
                        End If
                    End If





                ElseIf Line.Contains("Audio:") Then
                    Dim a = Line.IndexOf("Audio: ")
                    a = a + "Audio: ".Length
                    Dim Entries() As String = Split(Line.Substring(a), ",")
                    AudioEncoding = GetEnumData(GetType(AudioEncodingEnum), GetPosition(Entries, 0))
                    AudioFrequency = Val(GetPosition(Entries, 1))
                    Channels = GetEnumData(GetType(SoundTypeEnum), GetPosition(Entries, 2))
                    AudioBitRate = Val(GetPosition(Entries, 4))

                End If


            End If
        Next

        DebugMsg("Video Info Loaded")

    End Sub
    Private Function GetEnumData(ByVal EnumType As Type, ByVal Value As String) As Integer
        Try
            Return [Enum].Parse(EnumType, Value)
        Catch ex As Exception
            Return 0
        End Try
    End Function
    Private Function GetPosition(ByVal Entries() As String, ByVal Pos As Integer) As String
        If Pos >= Entries.Length Then
            Return ""
        ElseIf Pos < 0 Then
            Return ""
        Else
            Return Entries(Pos)
        End If
    End Function
    'Public Sub GenerateImage(ByVal Width As Integer, ByVal Height As Integer)

    '    Dim ImageFile As New System.IO.FileInfo("")
    '    If ImageFile.Exists Then ImageFile.Delete()

    '    DebugMsg("Grabbing Image")
    '    Dim ImageLogData = RunFFMpeg("-i """ & File.FullName & """ -an -ss 00:00:03 -an -s " & Width & "x" & Height & " -r 1 -vframes 1 -f image2 -y """ & ImageFile.FullName & """")
    '    DebugMsg("Image Grabbed")
    '    ImageFile.Refresh()

    'End Sub
    Public Sub GenerateImage(ByVal ImageFile As System.IO.FileInfo, ByVal Width As Integer, ByVal Height As Integer)
        If ImageFile.Exists Then ImageFile.Delete()

        Dim FNWE = Left(File.Name, Len(File.Name) - (Len(File.Extension))) & "_Image"
        Dim TempDirectory = New System.IO.DirectoryInfo(File.Directory.FullName & "\Archive\" & Now.Year & Now.Month.ToString("00") & Now.Day.ToString("00"))
        If Not TempDirectory.Exists Then TempDirectory.Create()
        Dim TempLogFile As New System.IO.FileInfo(TempDirectory.FullName & "\" & FNWE & ".log")
        If TempLogFile.Exists Then
            TempLogFile.Delete()
        End If

        DebugMsg("Grabbing Image")
        Dim LogData = RunFFMpeg("-i """ & File.FullName & """ -an -ss 00:00:07 -an -s " & Width & "x" & Height & " -r 1 -vframes 1 -f image2 -y """ & ImageFile.FullName & """")

        Dim SW = New System.IO.StreamWriter(TempLogFile.OpenWrite)
        SW.Write(LogData)
        SW.Close()

        DebugMsg("Image Grabbed")
        ImageFile.Refresh()

    End Sub

    Public Sub GenerateAudio(ByVal Audiofile As System.IO.FileInfo, Optional ByVal AudioEncoding As AudioEncodingEnum = AudioEncodingEnum.mp3, Optional ByVal Channels As SoundTypeEnum = SoundTypeEnum.Stereo, Optional ByVal AudioBitRate As Integer = 128, Optional ByVal AudioFrequency As Integer = 44100)
        If Audiofile.Exists Then Audiofile.Delete()

        Dim FNWE = Left(File.Name, Len(File.Name) - (Len(File.Extension))) & "_Audio"
        Dim TempDirectory = New System.IO.DirectoryInfo(File.Directory.FullName & "\Archive\" & Now.Year & Now.Month.ToString("00") & Now.Day.ToString("00"))
        If Not TempDirectory.Exists Then TempDirectory.Create()
        Dim TempLogFile As New System.IO.FileInfo(TempDirectory.FullName & "\" & FNWE & ".log")
        If TempLogFile.Exists Then
            TempLogFile.Delete()
        End If

        DebugMsg("Grabbing Audio")
        Dim LogData = RunFFMpeg("-i """ & File.FullName & """ -vn -acodec " & AudioFormatForEncoding(AudioEncoding) & " -ab " & AudioBitRate & "k -ac " & CInt(Channels) & " -ar " & AudioFrequency & " -y """ & Audiofile.FullName & """")
        Dim SW = New System.IO.StreamWriter(TempLogFile.OpenWrite)
        SW.Write(LogData)
        SW.Close()
        If EncodingState = EncodingStateEnum.Not_Encoding Then
            Throw New Exception("No audio was encoded." + vbCrLf + LogData)
        End If
        DebugMsg("Audio Grabbed")
        Audiofile.Refresh()
    End Sub
    Public Sub ConvertTo(ByVal newfile As FileInfo)
        DebugMsg("Converting Video")
        Dim STime As Date = Now
        Try
            'Dim FInf = newfile.File
            newfile.Duration = Me.Duration
            Dim FNWE = Left(File.Name, Len(File.Name) - (Len(File.Extension)))
            Dim FNWE2 = ""


            Dim TempDirectory = New System.IO.DirectoryInfo(File.Directory.FullName & "\Temp")
            If Not TempDirectory.Exists Then TempDirectory.Create()
            DebugMsg("Setting up file links")
            If newfile.File Is Nothing Then
                newfile.File = New System.IO.FileInfo(File.Directory.FullName & "\" & FNWE & ".flv")
                FNWE2 = FNWE
            Else
                FNWE2 = Left(newfile.File.Name, Len(newfile.File.Name) - (Len(newfile.File.Extension)))
            End If

            Dim TempVideoFile As New System.IO.FileInfo(TempDirectory.FullName & "\" & FNWE & newfile.File.Extension)
            'Dim TempImageFile As New System.IO.FileInfo(TempDirectory.FullName & "\" & FNWE & ".jpg")
            Dim TempVideoLogFile As New System.IO.FileInfo(TempDirectory.FullName & "\" & FNWE2 & "_" & newfile.VideoEncoding.ToString & ".log")
            'Dim TempImageLogFile As New System.IO.FileInfo(TempDirectory.FullName & "\" & FNWE & "-Img.log")


            Dim FinalVideoFile = newfile.File
            'Dim FinalImageFile As New System.IO.FileInfo(newfile.File.Directory.FullName & "\" & FNWE2 & ".jpg")


            If TempVideoFile.Exists Then
                TempVideoFile.Delete()
            End If
            'If TempImageFile.Exists Then
            '    TempImageFile.Delete()
            'End If
            If TempVideoLogFile.Exists Then
                TempVideoLogFile.Delete()
            End If
            'If TempImageLogFile.Exists Then
            '    TempImageLogFile.Delete()
            'End If

            If newfile.Width = 0 AndAlso newfile.Height = 0 Then
                newfile.Width = Width
                newfile.Height = Height
            End If

            Dim AdditionalString As String = ""

            DebugMsg("Currently    : " & Width & "x" & Height)
            DebugMsg("Currently Adj: " & WidthAdjusted & "x" & HeightAdjusted)
            DebugMsg("Going to    : " & newfile.Width & "x" & newfile.Height)
            DebugMsg("Going to Adj: " & newfile.WidthAdjusted & "x" & newfile.HeightAdjusted)
            Dim TopPad = 0
            Dim BottomPad = 0
            Dim LeftPad = 0
            Dim RightPad = 0
            If Not newfile.Stretch Then
                If WidthAdjusted > 0 AndAlso HeightAdjusted > 0 Then

                    If (newfile.WidthAdjusted / WidthAdjusted) > (newfile.HeightAdjusted / HeightAdjusted) Then
                        'Y limited
                        TopPad = 0
                        BottomPad = 0
                        Dim AF = newfile.WidthAdjusted - WidthAdjusted * (newfile.HeightAdjusted / HeightAdjusted)
                        Dim AF2 = CInt(AF / 2)
                        If AF2 Mod 2 > 0 Then
                            AF2 += 1
                        End If
                        LeftPad = AF2
                        RightPad = AF2

                    Else
                        'X Limited
                        LeftPad = 0
                        RightPad = 0
                        Dim AF = newfile.HeightAdjusted - HeightAdjusted * (newfile.WidthAdjusted / WidthAdjusted)
                        Dim AF2 = CInt(AF / 2)
                        If AF2 Mod 2 > 0 Then
                            AF2 += 1
                        End If
                        TopPad = AF2
                        BottomPad = AF2

                    End If
                    If TopPad > 0 OrElse BottomPad > 0 OrElse LeftPad > 0 OrElse RightPad > 0 Then
                        If newfile.VideoEncoding = VideoEncodingEnum.h264_iPod OrElse newfile.VideoEncoding = VideoEncodingEnum.h264 Then
                            AdditionalString = " -padtop " & TopPad & " -padbottom " & BottomPad & " -padleft " & LeftPad & " -padright " & RightPad & " -padcolor 000000"
                        Else
                            AdditionalString = " -vf pad=" & RightPad & ":" & BottomPad & ":" & LeftPad & ":" & TopPad & ":000000"
                        End If
                    End If
                End If
            End If

            DebugMsg("Converting Video")
            Dim LogData As String = ""
            If newfile.VideoEncoding = VideoEncodingEnum.flv Then
                Dim TestList As New List(Of TestRun)
                TestList.Add(New TestRun With {.Quality = newfile.QMax, .Passes = newfile.NumberPasses, .BitRate = newfile.BitRate, .MaxQuality = newfile.QMax + 5})
                If newfile.QMax > 0 Then
                    Dim QT = newfile.QMax
                    For a As Integer = 200 To 1000 Step 200
                        TestList.Add(New TestRun With {.Quality = QT, .Passes = newfile.NumberPasses, .BitRate = newfile.BitRate + a, .MaxQuality = QT + 5})
                    Next
                    For a As Integer = 0 To 1000 Step 200
                        TestList.Add(New TestRun With {.Quality = QT + 5, .Passes = newfile.NumberPasses, .BitRate = newfile.BitRate + a, .MaxQuality = QT + 8})
                        TestList.Add(New TestRun With {.Quality = QT + 10, .Passes = newfile.NumberPasses, .BitRate = newfile.BitRate + a, .MaxQuality = QT + 11})
                    Next

                    TestList = (From TR In TestList Order By TR.MaxQuality ^ 2 + (TR.BitRate / 100) ^ 2 * 0.65 Select TR).ToList

                    TestList.Add(New TestRun With {.Quality = 0, .Passes = PassEnum.Two, .BitRate = newfile.BitRate + 1000, .MaxQuality = 40})
                    TestList.Add(New TestRun With {.Quality = 0, .Passes = PassEnum.One, .BitRate = newfile.BitRate + 1000, .MaxQuality = 40})
                End If


                TempVideoFile.Refresh()
                Dim Cnt As Integer = 0
                DebugMsg("TestList.Count=" & TestList.Count)
                While ((Not TempVideoFile.Exists OrElse TempVideoFile.Length < 1000) AndAlso (Cnt < TestList.Count))
                    DebugMsg("Cnt=" & Cnt & " Q=" & TestList(Cnt).Quality & " Passes=" & TestList(Cnt).Passes.ToString)
                    newfile.QMax = TestList(Cnt).Quality
                    newfile.NumberPasses = TestList(Cnt).Passes
                    newfile.BitRate = TestList(Cnt).BitRate

                    DebugMsg("MaxQuality looking for=" & TestList(Cnt).MaxQuality)
                    MaxQualityFound = 0

                    Dim Arguments As String = "-i """ & File.FullName & """ -r " & newfile.FrameRate & " -f flv " & IIf(Deinterlace, "-deinterlace ", "") & "-ac " & CInt(newfile.Channels) & " -ar " & newfile.AudioFrequency & " -ab " & newfile.AudioBitRate & "k" & IIf(newfile.AudioEncoding <> AudioEncodingEnum.None, " -acodec " & AudioFormatForEncoding(newfile.AudioEncoding), "") & " -b " & newfile.BitRate & "k -s " & (newfile.WidthAdjusted - LeftPad - RightPad) & "x" & (newfile.HeightAdjusted - TopPad - BottomPad) & " -aspect 16:9 "
                    If TestList(Cnt).Quality > 0 Then
                        Arguments = Arguments & "-qmin " & newfile.QMin & " -qmax " & newfile.QMax & " -qcomp 0.7 -g 299.7 -qdiff 4 "
                    End If
                    Arguments = Arguments & (AdditionalString & IIf(IsAudio, " -vn", ""))
                    If newfile.NumberPasses = PassEnum.Two Then
                        MaxQualityFound = 0
                        RaiseEvent StartConversion(PassEnum.Two, 1)
                        LogData = RunFFMpeg("-pass 1 " & Arguments & " -y """ & TempVideoFile.FullName & """")
                        newfile.EncodingState = EncodingState
                        RaiseEvent EndConversion()
                        MaxQualityFound = 0
                        RaiseEvent StartConversion(PassEnum.Two, 2)
                        LogData = RunFFMpeg("-pass 2 " & Arguments & " -y """ & TempVideoFile.FullName & """")
                        newfile.EncodingState = EncodingState
                        RaiseEvent EndConversion()

                        If EncodingState = EncodingStateEnum.Not_Encoding Then
                            'Throw New Exception("Nothing was encoded." + vbCrLf + LogData)
                        End If
                    Else
                        MaxQualityFound = 0
                        RaiseEvent StartConversion(PassEnum.One, 1)
                        LogData = RunFFMpeg(Arguments & " -y """ & TempVideoFile.FullName & """")
                        newfile.EncodingState = EncodingState
                        RaiseEvent EndConversion()

                        If EncodingState = EncodingStateEnum.Not_Encoding Then
                            Throw New Exception("Nothing was encoded." + vbCrLf + LogData)
                        End If
                    End If
                    Cnt += 1
                    TempVideoFile.Refresh()
                    DebugMsg("MaxQualityFound=" & MaxQualityFound)
                    DebugMsg("TempVideoFile.Exists=" & TempVideoFile.Exists)
                    If MaxQualityFound > (TestList(Cnt - 1).MaxQuality) AndAlso TempVideoFile.Exists AndAlso TempVideoFile.Length > 1000 AndAlso Cnt < TestList.Count Then
                        DebugMsg("Video Created but not good enough quality")

                        TempVideoFile.Delete()
                        TempVideoFile.Refresh()
                    End If


                    TempVideoLogFile.Refresh()
                    If TempVideoLogFile.Exists Then TempVideoLogFile.Delete()
                    Dim SW2 = New System.IO.StreamWriter(TempVideoLogFile.OpenWrite)
                    SW2.Write(LogData)
                    SW2.Close()
                    TempVideoLogFile.Refresh()

                    DebugMsg("")
                End While
                DebugMsg("TempVideoFile.Exists=" & TempVideoFile.Exists)
            ElseIf newfile.VideoEncoding = VideoEncodingEnum.h264 Then
                MaxQualityFound = 0
                RaiseEvent StartConversion(PassEnum.One, 1)
                LogData = RunFFMpegOld("-i """ & File.FullName & """ -r " & newfile.FrameRate & " -vcodec libx264 -threads 0 " & IIf(Deinterlace, "-deinterlace ", "") & "-ac " & CInt(newfile.Channels) & " -ar " & newfile.AudioFrequency & " -ab " & newfile.AudioBitRate & "k" & IIf(newfile.AudioEncoding <> AudioEncodingEnum.None, " -acodec " & AudioFormatForEncoding(newfile.AudioEncoding), "") & " -s " & (newfile.WidthAdjusted - LeftPad - RightPad) & "x" & (newfile.HeightAdjusted - TopPad - BottomPad) & " -aspect 16:9 " & AdditionalString & IIf(IsAudio, " -vn", "") & " -level 41 -crf 20 -bufsize 20000k -maxrate 25000k -g 250 -coder 1 -flags +loop -cmp +chroma -partitions +parti4x4+partp8x8+partb8x8 -flags2 +dct8x8+bpyramid -me_method umh -subq 7 -me_range 16 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -rc_eq 'blurCplx^(1-qComp)' -bf 16 -b_strategy 1 -bidir_refine 1 -refs 6 -deblockalpha 0 -deblockbeta 0 -y """ & TempVideoFile.FullName & """")
                newfile.EncodingState = EncodingState
                RaiseEvent EndConversion()

                If EncodingState = EncodingStateEnum.Not_Encoding Then
                    Throw New Exception("Nothing was encoded." + vbCrLf + LogData)
                End If
            ElseIf newfile.VideoEncoding = VideoEncodingEnum.h264_iPod Then
                MaxQualityFound = 0
                RaiseEvent StartConversion(PassEnum.One, 1)
                LogData = RunFFMpegOld("-i """ & File.FullName & """ -r " & newfile.FrameRate & " -vcodec libx264 -threads 0 " & IIf(Deinterlace, "-deinterlace ", "") & "-ac " & CInt(newfile.Channels) & " -ar " & newfile.AudioFrequency & " -ab " & newfile.AudioBitRate & "k" & IIf(newfile.AudioEncoding <> AudioEncodingEnum.None, " -acodec " & AudioFormatForEncoding(newfile.AudioEncoding), "") & " -s " & (newfile.WidthAdjusted - LeftPad - RightPad) & "x" & (newfile.HeightAdjusted - TopPad - BottomPad) & " -aspect " & (newfile.WidthAdjusted) & ":" & (newfile.HeightAdjusted) & " " & AdditionalString & IIf(IsAudio, " -vn", "") & " -vpre """ & FFMpegLocation & "ffpresets\libx264-ipod640.ffpreset"" -b " & newfile.BitRate & "k -bt " & newfile.BitRate & "k -f ipod -y """ & TempVideoFile.FullName & """")

                newfile.EncodingState = EncodingState
                RaiseEvent EndConversion()

                If EncodingState = EncodingStateEnum.Not_Encoding Then
                    Throw New Exception("Nothing was encoded." + vbCrLf + LogData)
                End If
            ElseIf newfile.VideoEncoding = VideoEncodingEnum.OGG_Theora Then
                '-f ogg -vcodec libtheora -b 800k -g 300 -acodec libvorbis -ab 128k

                Dim Arguments As String = "-i """ & File.FullName & """ -threads 0 -g 300 " & "-qmin " & newfile.QMin & " -qmax " & newfile.QMax & IIf(Deinterlace, " -deinterlace ", "") & " -ac " & CInt(newfile.Channels) & " -vcodec libtheora -acodec libvorbis -ab 128k -b 800k -s " & (newfile.WidthAdjusted - LeftPad - RightPad) & "x" & (newfile.HeightAdjusted - TopPad - BottomPad) & " -aspect 16:9 "

                MaxQualityFound = 0
                RaiseEvent StartConversion(PassEnum.Two, 1)
                LogData = RunFFMpeg("-pass 1 " & Arguments & " -y """ & TempVideoFile.FullName & """")
                newfile.EncodingState = EncodingState
                RaiseEvent EndConversion()
                MaxQualityFound = 0
                RaiseEvent StartConversion(PassEnum.Two, 2)
                LogData = RunFFMpeg("-pass 2 " & Arguments & " -y """ & TempVideoFile.FullName & """")
                newfile.EncodingState = EncodingState
                RaiseEvent EndConversion()
                If EncodingState = EncodingStateEnum.Not_Encoding Then
                    Throw New Exception("Nothing was encoded." + vbCrLf + LogData)
                End If


                'MaxQualityFound = 0
                'RaiseEvent StartConversion(PassEnum.One, 1)
                'LogData = CMD.Shell(FFMpegLocation & "ffmpeg2theora.exe", "--videoquality 8 --audioquality 5 --max_size " & (newfile.WidthAdjusted) & "x" & (newfile.HeightAdjusted) & " -o """ & TempVideoFile.FullName & """ """ & File.FullName & """")
                'RaiseEvent EndConversion()

            ElseIf newfile.VideoEncoding = VideoEncodingEnum.WebM Then

                Dim Arguments As String = "-i """ & File.FullName & """ -threads 0 -keyint_min 0 -g 250 -skip_threshold 0 " & "-qmin " & newfile.QMin & " -qmax " & newfile.QMax & IIf(Deinterlace, " -deinterlace ", "") & " -ac " & CInt(newfile.Channels) & " -vcodec libvpx -acodec libvorbis -b 614400 -s " & (newfile.WidthAdjusted - LeftPad - RightPad) & "x" & (newfile.HeightAdjusted - TopPad - BottomPad) & " -aspect 16:9 "

                MaxQualityFound = 0
                RaiseEvent StartConversion(PassEnum.Two, 1)
                LogData = RunFFMpeg("-pass 1 " & Arguments & " -y """ & TempVideoFile.FullName & """")
                newfile.EncodingState = EncodingState
                RaiseEvent EndConversion()
                MaxQualityFound = 0
                RaiseEvent StartConversion(PassEnum.Two, 2)
                LogData = RunFFMpeg("-pass 2 " & Arguments & " -y """ & TempVideoFile.FullName & """")
                newfile.EncodingState = EncodingState
                RaiseEvent EndConversion()

                If EncodingState = EncodingStateEnum.Not_Encoding Then
                    Throw New Exception("Nothing was encoded." + vbCrLf + LogData)
                End If
            End If
            newfile.MaxQualityFound = MaxQualityFound

            DebugMsg("Video Converted")
            TempVideoFile.Refresh()

            If TempVideoLogFile.Exists Then TempVideoLogFile.Delete()
            Dim SW = New System.IO.StreamWriter(TempVideoLogFile.OpenWrite)
            SW.Write(LogData)
            SW.Close()
            TempVideoLogFile.Refresh()

            If newfile.VideoEncoding = VideoEncodingEnum.flv Then
                DebugMsg("Setting Meta Data")
                LogData = RunFLVTool("-Uk """ & TempVideoFile.FullName & """")
            End If



            If Not FinalVideoFile.Directory.Exists Then
                FinalVideoFile.Directory.Create()
            End If



            DebugMsg("Handling Temp Video File")
            Dim AllOK As Boolean = True
            If TempVideoFile.Exists AndAlso TempVideoFile.Length > 1000 Then
                FinalVideoFile.Refresh()
                If FinalVideoFile.Exists Then
                    FinalVideoFile.Delete()
                    FinalVideoFile.Refresh()
                End If
                If Not FinalVideoFile.Exists Then
                    TempVideoFile.MoveTo(FinalVideoFile.FullName)
                Else
                    DebugMsg("FINAL VIDEO FILE still exists")
                End If
            Else
                AllOK = False
            End If


            DebugMsg("Cleaning up directory")
            If AllOK Then
                Dim HisDir = New System.IO.DirectoryInfo(File.Directory.FullName & "\Archive\" & Now.Year & Now.Month.ToString("00") & Now.Day.ToString("00"))
                If Not HisDir.Exists Then HisDir.Create()
                Dim FF As System.IO.FileInfo
                FF = New System.IO.FileInfo(HisDir.FullName & "\" & TempVideoLogFile.Name)
                If FF.Exists Then FF.Delete()
                TempVideoLogFile.MoveTo(FF.FullName)
                'FF = New System.IO.FileInfo(HisDir.FullName & "\" & TempImageLogFile.Name)
                'If FF.Exists Then FF.Delete()
                'TempImageLogFile.MoveTo(FF.FullName)
                'FF = New System.IO.FileInfo(HisDir.FullName & "\" & FromFile.Name)
                'If FF.Exists Then FF.Delete()
                'FromFile.MoveTo(FF.FullName)
            End If

        Catch ex As Exception

            DebugMsg("Error: " & ex.ToString)
            Throw
        End Try

        DebugMsg("Time to convert:" & (Now.Subtract(STime).TotalMilliseconds / 1000))
        Debug.WriteLine(Now.Subtract(STime).TotalMilliseconds / 1000)
    End Sub

    Private Function RunFFMpeg(ByVal Args As String) As String
        EncodingState = EncodingStateEnum.Not_Encoding
        Return CMD.Shell(FFMpegLocation & "ffmpeg.exe", Args)
    End Function
    Private Function RunFFMpegOld(ByVal Args As String) As String
        EncodingState = EncodingStateEnum.Not_Encoding
        Return CMD.Shell(FFMpegLocation & "ffmpeg_old.exe", Args)
    End Function
    Private Function RunFLVTool(ByVal Args As String) As String
        Return CMD.Shell(FLVToolLocation & "FLVTool2.exe", Args)
    End Function

    Private Sub CMD_DebugMessage(ByVal Msg As String) Handles CMD.DebugMessage
        DebugMsg(Msg)
    End Sub

    Private Sub CMD_ShellMessage(ByVal msg As String) Handles CMD.ShellMessage
        Dim Line = msg
        If String.IsNullOrEmpty(Line) Then
            Return
        End If
        Debug.WriteLine(Line)

        'Dim tstr() As String = Split(Line, "=")
        If Line.Contains("time=") Then
            EncodingState = EncodingStateEnum.Audio_Only
            Dim PS As New ProcessState With {
                .Frame = Val(GetValueOnLine(Line, "frame")),
                .FPS = Val(GetValueOnLine(Line, "fps")),
                .Q = Val(GetValueOnLine(Line, "q")),
                .Size = Val(GetValueOnLine(Line, "size")),
                .Time = Val(GetValueOnLine(Line, "time")),
                .Bitrate = Val(GetValueOnLine(Line, "bitrate"))
            }
            If PS.Frame > 0 Then
                EncodingState = EncodingStateEnum.Video_and_Audio
            End If
            If PS.Q > 0 Then
                If MaxQualityFound < PS.Q Then MaxQualityFound = PS.Q
            End If
            RaiseEvent State(PS)
        End If
    End Sub

    Private Function GetValueOnLine(Line As String, Name As String) As String
        Dim pos = Line.ToUpper.IndexOf(Name.ToUpper & "=")
        If (pos >= 0) Then
            Dim Part = Line.Substring(pos + Name.Length + 1)
            Part = Part.Trim()
            Dim EndPos = Part.IndexOf(" ")
            If EndPos < 0 Then
                EndPos = Part.Length
            End If
            Dim Value = Part.Substring(0, EndPos - 1)
            Return Value
        End If
        Return ""
    End Function

End Class
