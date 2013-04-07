

Public Class ConversionSetting
    Public Name As String = ""
    Public Extension As String = ""
    Public File As FFMpeg.FileInfo

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
