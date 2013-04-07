Imports Microsoft.VisualBasic.CompilerServices

Public Class Form1
    Dim WithEvents VFI As New FFMpeg.FileInfo
    Dim FileWrite As System.IO.StreamWriter

    Dim FileCSV As System.IO.FileInfo

    'Private Conversions As New List(Of ConversionSetting)

    Private Sub Form1_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Dim width = 804 '600 '1280x720
        Dim height = width * 9 / 16

        Conversions.Items.Add(New ConversionSetting With {
                        .Name = "High m4v no resize",
                        .Extension = "m4v",
                        .File = New FFMpeg.FileInfo With {
                            .VideoEncoding = FFMpeg.VideoEncodingEnum.h264_iPod,
                            .Channels = FFMpeg.SoundTypeEnum.Stereo,
                            .AudioBitRate = 128,
                            .AudioEncoding = FFMpeg.AudioEncodingEnum.aac,
                            .BitRate = 15000,
                            .Stretch = False,
                            .Width = 0,
                            .Height = 0
                        }
                    })
        Conversions.Items.Add(New ConversionSetting With {
                        .Name = "High m4v",
                        .Extension = "m4v",
                        .File = New FFMpeg.FileInfo With {
                            .Width = width,
                            .Height = width * 9 / 16,
                            .VideoEncoding = FFMpeg.VideoEncodingEnum.h264_iPod,
                            .Channels = FFMpeg.SoundTypeEnum.Stereo,
                            .AudioBitRate = 128,
                            .AudioEncoding = FFMpeg.AudioEncodingEnum.aac,
                            .BitRate = 15000,
                            .Stretch = False
                        }
                    })
        Conversions.Items.Add(New ConversionSetting With {
                        .Name = "High m4v Stretched",
                        .Extension = "m4v",
                        .File = New FFMpeg.FileInfo With {
                            .Width = width,
                            .Height = width * 9 / 16,
                            .VideoEncoding = FFMpeg.VideoEncodingEnum.h264_iPod,
                            .Channels = FFMpeg.SoundTypeEnum.Stereo,
                            .AudioBitRate = 128,
                            .AudioEncoding = FFMpeg.AudioEncodingEnum.aac,
                            .BitRate = 15000,
                            .Stretch = True
                        }
                    })
        Conversions.Items.Add(New ConversionSetting With {
                        .Name = "iPod m4v",
                        .Extension = "m4v",
                        .File = New FFMpeg.FileInfo With {
                            .Width = width,
                            .Height = width * 9 / 16,
                            .VideoEncoding = FFMpeg.VideoEncodingEnum.h264_iPod,
                            .Channels = FFMpeg.SoundTypeEnum.Stereo,
                            .AudioBitRate = 128,
                            .AudioEncoding = FFMpeg.AudioEncodingEnum.aac,
                            .BitRate = 1500,
                            .Stretch = False
                        }
                    })

        Conversions.Items.Add(New ConversionSetting With {
                        .Name = "flv 1-pass",
                        .Extension = "flv",
                        .File = New FFMpeg.FileInfo With {
                            .Width = width,
                            .Height = width * 9 / 16,
                            .VideoEncoding = FFMpeg.VideoEncodingEnum.flv,
                            .Channels = FFMpeg.SoundTypeEnum.Stereo,
                            .AudioBitRate = 128,
                            .NumberPasses = FFMpeg.PassEnum.One,
                            .QMax = 0,
                            .FrameRate = 24,
                            .BitRate = 1000,
                            .Stretch = False
                        }
                    })

        Conversions.Items.Add(New ConversionSetting With {
                        .Name = "WebM",
                        .Extension = "webm",
                        .File = New FFMpeg.FileInfo With {
                            .Width = width,
                            .Height = width * 9 / 16,
                            .VideoEncoding = FFMpeg.VideoEncodingEnum.WebM,
                            .Channels = FFMpeg.SoundTypeEnum.Stereo,
                            .AudioBitRate = 128,
                            .NumberPasses = FFMpeg.PassEnum.Two,
                            .QMax = 51,
                            .FrameRate = 24,
                            .BitRate = 1000,
                            .Stretch = False
                        }
                    })
        Conversions.Items.Add(New ConversionSetting With {
                        .Name = "Ogg Theora",
                        .Extension = "ogv",
                        .File = New FFMpeg.FileInfo With {
                            .Width = width,
                            .Height = width * 9 / 16,
                            .VideoEncoding = FFMpeg.VideoEncodingEnum.OGG_Theora,
                            .Channels = FFMpeg.SoundTypeEnum.Stereo,
                            .AudioBitRate = 128,
                            .NumberPasses = FFMpeg.PassEnum.Two,
                            .QMax = 51,
                            .FrameRate = 24,
                            .BitRate = 1000,
                            .Stretch = False
                        }
                    })

        Conversions.Items.Add(New ConversionSetting With {
                        .Name = "flv 2-pass",
                        .Extension = "flv",
                        .File = New FFMpeg.FileInfo With {
                            .Width = width,
                            .Height = width * 9 / 16,
                            .VideoEncoding = FFMpeg.VideoEncodingEnum.flv,
                            .Channels = FFMpeg.SoundTypeEnum.Stereo,
                            .AudioBitRate = 128,
                            .NumberPasses = FFMpeg.PassEnum.Two,
                            .QMax = 5,
                            .FrameRate = 24,
                            .BitRate = 1000,
                            .Stretch = False
                        }
                    })
        Conversions.Items.Add(New ConversionSetting With {
                        .Name = "flv 2-pass 4x3",
                        .Extension = "flv",
                        .File = New FFMpeg.FileInfo With {
                            .Width = width,
                            .Height = width * 3 / 4,
                            .VideoEncoding = FFMpeg.VideoEncodingEnum.flv,
                            .Channels = FFMpeg.SoundTypeEnum.Stereo,
                            .AudioBitRate = 128,
                            .NumberPasses = FFMpeg.PassEnum.Two,
                            .QMax = 5,
                            .FrameRate = 24,
                            .BitRate = 1000,
                            .Stretch = False
                        }
                    })
    End Sub

    Private Sub Convert_Click(sender As System.Object, e As System.EventArgs) Handles Convert.Click
        OpenFileDialog1.DefaultExt = "*"
        OpenFileDialog1.Multiselect = True
        OpenFileDialog1.Filter = "*.*|*.*"
        OpenFileDialog1.ShowDialog()
        For Each F In OpenFileDialog1.FileNames
            VFI = New FFMpeg.FileInfo(F)
            Dim NFile = VFI.File.FullName
            ProgressBar1.Maximum = VFI.Duration
            FileCSV = New System.IO.FileInfo(Mid(NFile, 1, InStrRev(NFile, ".") - 1) & ".csv")
            If Not FileCSV.Directory.Exists Then FileCSV.Directory.Create()
            Dim Settings As ConversionSetting = Conversions.SelectedItem
            Dim extra = ""
            If Settings.Extension = VFI.Extension Then
                extra = "_Final"
            End If
            Dim VideoFile = New System.IO.FileInfo(Mid(NFile, 1, InStrRev(NFile, ".") - 1) & extra & "." & Settings.Extension)
            Settings.File.File = VideoFile
            Dim VFI3 = Settings.File

            Try
                VFI.ConvertTo(VFI3)
                If VFI3.IsVideo AndAlso VFI.EncodingState = FFMpeg.EncodingStateEnum.Audio_Only Then
                    Throw New Exception("No video portion converted")
                End If
            Catch ex As Exception
                MessageBox.Show(ex.ToString)
            End Try
            'FileWrite.Close()
            'FileWrite = Nothing
        Next
    End Sub

    Private Sub VFI_EndConversion() Handles VFI.EndConversion
        If FileWrite IsNot Nothing Then
            FileWrite.Close()
            FileWrite = Nothing
        End If
    End Sub

    Private Sub VFI_StartConversion(ByVal TotalPasses As FFMpeg.PassEnum, ByVal CurrentPass As Integer) Handles VFI.StartConversion
        If FileWrite IsNot Nothing Then
            FileWrite.Close()
            FileWrite = Nothing
        End If
        If FileCSV.Exists Then FileCSV.Delete()
        FileWrite = New System.IO.StreamWriter(FileCSV.OpenWrite)

    End Sub

    Private Sub VFI_State(ByVal PS As FFMpeg.ProcessState) Handles VFI.State
        ProgressBar1.Value = PS.Time
        FileWrite.WriteLine(PS.Time & "," & PS.Frame & "," & PS.Q & "," & PS.Bitrate)
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        OpenFileDialog1.DefaultExt = "*"
        OpenFileDialog1.Multiselect = True
        OpenFileDialog1.Filter = "*.*|*.*"
        OpenFileDialog1.ShowDialog()
        For Each F In OpenFileDialog1.FileNames
            VFI = New FFMpeg.FileInfo(F)
            Dim NFile = VFI.File.FullName
            ProgressBar1.Maximum = VFI.Duration
            FileCSV = New System.IO.FileInfo(Mid(NFile, 1, InStrRev(NFile, ".") - 1) & ".csv")
            If Not FileCSV.Directory.Exists Then FileCSV.Directory.Create()

            Dim PodcastVideoFile = New System.IO.FileInfo(Mid(NFile, 1, InStrRev(NFile, ".") - 1) & ".m4v")

            Dim VFI3 As New FFMpeg.FileInfo With {.Width = 804, .Height = 804 * 9 / 16, .VideoEncoding = FFMpeg.VideoEncodingEnum.h264_iPod, .File = PodcastVideoFile, .Channels = FFMpeg.SoundTypeEnum.Stereo, .AudioBitRate = 128, .AudioEncoding = FFMpeg.AudioEncodingEnum.aac, .BitRate = 1500}
            Try
                VFI.ConvertTo(VFI3)
                If VFI.EncodingState = FFMpeg.EncodingStateEnum.Audio_Only Then
                    Throw New Exception("No video portion converted")
                End If
            Catch ex As Exception
                MessageBox.Show(ex.ToString)
            End Try
            'FileWrite.Close()
            'FileWrite = Nothing
        Next
    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        OpenFileDialog1.DefaultExt = "*"
        OpenFileDialog1.Multiselect = True
        OpenFileDialog1.Filter = "*.*|*.*"
        OpenFileDialog1.ShowDialog()
        For Each F In OpenFileDialog1.FileNames
            VFI = New FFMpeg.FileInfo(F)
            Dim NFile = VFI.File.FullName

            'Dim FName = New System.IO.FileInfo(Mid(NFile, 1, InStrRev(NFile, ".") - 1) & " 804 1000 1 pass.csv")
            'If Not FName.Directory.Exists Then FName.Directory.Create()
            'FileWrite = New System.IO.StreamWriter(FName.OpenWrite)

            ''NFile = Mid(NFile, 1, InStrRev(NFile, ".")) & "m4v"

            ''Dim VFI3 As New FFMpeg.FileInfo With {.Width = 804, .Height = 452, .VideoEncoding = FFMpeg.VideoEncodingEnum.h264, .File = New System.IO.FileInfo(NFile), .Channels = FFMpeg.SoundTypeEnum.Stereo, .AudioBitRate = 128, .AudioEncoding = FFMpeg.AudioEncodingEnum.aac}
            ''VFI.ConvertTo(VFI3)

            ''804x452
            'NFile = Mid(NFile, 1, InStrRev(NFile, ".") - 1) & " 804 1000 1 pass.flv"
            'Dim VFI2 As New FFMpeg.FileInfo With {.Width = 804, .Height = 452, .VideoEncoding = FFMpeg.VideoEncodingEnum.flv, .File = New System.IO.FileInfo(NFile), .Channels = FFMpeg.SoundTypeEnum.Stereo, .AudioBitRate = 128, .BitRate = 1000, .NumberPasses = FFMpeg.PassEnum.One, .QMax = 0}
            'VFI.ConvertTo(VFI2)
            'Dim q = 5
            'Dim P = 2
            'Dim Di = 1
            'For q = 5 To 32 Step 5
            'For Di = 0 To 1
            'For P = 1 To 2
            'FileWrite.Close()
            'Dim DeIn = IIf(Di = 1, True, False)
            Dim BName As String = "" '" 1000 kbps - " & IIf(Di = 1, "DeInterlace", "Interlace") & " - " & q & " Q - " & P & " Pass"

            NFile = VFI.File.FullName

            FileCSV = New System.IO.FileInfo(Mid(NFile, 1, InStrRev(NFile, ".") - 1) & " " & BName & ".csv")
            If Not FileCSV.Directory.Exists Then FileCSV.Directory.Create()

            NFile = Mid(NFile, 1, InStrRev(NFile, ".") - 1) & " " & BName & ".m4v"
            'Dim VFI2 = New FFMpeg.FileInfo With {.Width = 804, .Height = 452, .VideoEncoding = FFMpeg.VideoEncodingEnum.flv, .File = New System.IO.FileInfo(NFile), .Channels = FFMpeg.SoundTypeEnum.Stereo, .AudioBitRate = 128, .BitRate = 1000, .QMax = q, .Deinterlace = DeIn, .NumberPasses = P}

            Dim VFI3 As New FFMpeg.FileInfo With {.Width = 640, .Height = 640 * 9 / 16, .VideoEncoding = FFMpeg.VideoEncodingEnum.h264_iPod, .File = New System.IO.FileInfo(NFile), .Channels = FFMpeg.SoundTypeEnum.Stereo, .AudioBitRate = 128, .AudioEncoding = FFMpeg.AudioEncodingEnum.aac, .BitRate = 1500}


            VFI.ConvertTo(VFI3)
            'FileWrite.Close()
            'FileWrite = Nothing


            'Next
            'Next
            'Next
        Next
        'FileWrite.Close()
        'FileWrite = Nothing
        'Debug.WriteLine(fi.PAR)
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        OpenFileDialog1.DefaultExt = "*"
        OpenFileDialog1.Multiselect = True
        OpenFileDialog1.Filter = "*.*|*.*"
        OpenFileDialog1.ShowDialog()

        'Dim Rate() As Integer = {2000, 1000, 500, 300}
        'Dim Width() As Integer = {804, 600, 402, 320} ', 200}
        Dim Rate() As Integer = {1000} ', 900, 800, 500, 300} ' {1600, 1500, 1400, 1300, 1200, 1100, 1000, 900, 800, 500, 300}
        Dim Width() As Integer = {804, 600} ', 600, 402} ', 320} ', 200}
        Dim FrameRate() As Integer = {24} ', 320} ', 200}
        For Each F In OpenFileDialog1.FileNames
            VFI = New FFMpeg.FileInfo(F)
            Dim NFile = VFI.File.FullName
            NFile = Mid(NFile, 1, InStrRev(NFile, ".")) & "m4v"

            Dim DumpFile As New System.IO.FileInfo(Mid(NFile, 1, InStrRev(NFile, ".")) & "csv")
            If DumpFile.Exists Then DumpFile.Delete()
            Dim SWrite = New System.IO.StreamWriter(DumpFile.OpenWrite)
            SWrite.WriteLine("Rate,Width,Frame Rate,Passes,Quality,Length")
            SWrite.Close()
            'Dim VFI3 As New FFMpeg.FileInfo With {.Width = 804, .Height = 452, .VideoEncoding = FFMpeg.VideoEncodingEnum.h264, .File = New System.IO.FileInfo(NFile), .Channels = FFMpeg.SoundTypeEnum.Stereo, .AudioBitRate = 128, .AudioEncoding = FFMpeg.AudioEncodingEnum.aac}
            'VFI.ConvertTo(VFI3)
            For Each R In Rate
                For Each W In Width
                    'For Each FRate In FrameRate
                    Dim FRate = 24
                    NFile = VFI.File.FullName
                    'Dim FName As System.IO.FileInfo
                    'FName = New System.IO.FileInfo(Mid(NFile, 1, InStrRev(NFile, ".") - 1) & "_" & W & "_" & R & "_" & FRate & " T2 1 Pass.csv")
                    'NFile = Mid(NFile, 1, InStrRev(NFile, ".") - 1) & "_" & W & "_" & R & "_" & FRate & " T2 1 Pass.flv"
                    'If Not FName.Directory.Exists Then FName.Directory.Create()
                    'FileWrite = New System.IO.StreamWriter(FName.OpenWrite)


                    Dim VFI2 As FFMpeg.FileInfo
                    'VFI2 = New FFMpeg.FileInfo With {.Width = W, .Height = W * 9 / 16, .VideoEncoding = FFMpeg.VideoEncodingEnum.flv, .File = New System.IO.FileInfo(NFile), .Channels = FFMpeg.SoundTypeEnum.Stereo, .AudioBitRate = 128, .BitRate = R, .QMax = 5, .NumberPasses = FFMpeg.PassEnum.One, .FrameRate = FRate}
                    'VFI.ConvertTo(VFI2)
                    'FileWrite.Close()

                    'VFI2.File.Refresh()
                    'SWrite.WriteLine(R & "," & W & "," & FRate & "," & VFI2.NumberPasses.ToString & "," & VFI2.QMax & "," & VFI2.File.Length)


                    NFile = VFI.File.FullName
                    FileCSV = New System.IO.FileInfo(Mid(NFile, 1, InStrRev(NFile, ".") - 1) & "_" & W & "_" & R & "_" & FRate & " T2 2 Pass.csv")
                    NFile = Mid(NFile, 1, InStrRev(NFile, ".") - 1) & "_" & W & "_" & R & "_" & FRate & " T2 2 Pass.flv"
                    If Not FileCSV.Directory.Exists Then FileCSV.Directory.Create()
                    'FileWrite = New System.IO.StreamWriter(FName.OpenWrite)


                    VFI2 = New FFMpeg.FileInfo With {.Width = W, .Height = Int((W * 9 / 16) / 2 + 0.5) * 2, .VideoEncoding = FFMpeg.VideoEncodingEnum.flv, .File = New System.IO.FileInfo(NFile), .Channels = FFMpeg.SoundTypeEnum.Stereo, .AudioBitRate = 128, .BitRate = R, .QMax = 5, .FrameRate = FRate}
                    VFI.ConvertTo(VFI2)
                    'FileWrite.Close()
                    'FileWrite.Close()
                    'FileWrite = Nothing

                    VFI2.File.Refresh()
                    SWrite = DumpFile.AppendText
                    SWrite.WriteLine(VFI2.BitRate & "," & VFI2.Width & "," & VFI2.FrameRate & "," & VFI2.NumberPasses.ToString & "," & VFI2.QMax & "," & VFI2.File.Length)
                    SWrite.Close()
                    'Next
                Next
            Next
        Next
    End Sub

End Class
