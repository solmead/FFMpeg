'/*
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
Class ShellCommand
    Public Event DebugMessage(ByVal Msg As String)
    Public Event ShellMessage(ByVal msg As String)

    Public Sub DebugMsg(ByVal Msg As String)
        RaiseEvent DebugMessage(Msg)
    End Sub

    Public Sub ShellMsg(ByVal Msg As String)
        RaiseEvent ShellMessage(Msg)
    End Sub


    Public Function Shell(ByVal Command As String, ByVal Args As String) As String
        DebugMsg(Command)
        DebugMsg(Args)
        Dim FI As New System.IO.FileInfo(Command)
        Dim p = New System.Diagnostics.Process()
        Dim pStart = New System.Diagnostics.ProcessStartInfo()
        pStart.UseShellExecute = False
        pStart.WindowStyle = ProcessWindowStyle.Normal ' System.Diagnostics.ProcessWindowStyle.Hidden
        pStart.Arguments = Args
        pStart.FileName = Command
        pStart.CreateNoWindow = True ' True
        pStart.RedirectStandardOutput = True
        pStart.RedirectStandardError = True
        pStart.WorkingDirectory = FI.Directory.FullName

        p.StartInfo = pStart

        Dim sTimeOut = 60 * 60 * 2 'System.Configuration.ConfigurationManager.AppSettings("TranscodeTimeoutSeconds")
        Dim timeOut = Integer.Parse(sTimeOut)

        DebugMsg("Starting Process Command:[" & Command & "] Args:[" & Args & "]")
        Dim success = p.Start()
        'p.PriorityClass = ProcessPriorityClass.BelowNormal
        DebugMsg("Process Started")
        Dim SR2 = p.StandardError
        Dim S2 As String = ""
        If (success) Then
            DebugMsg("Process Success")

            Dim elapsedTime = System.Diagnostics.Stopwatch.StartNew()

            DebugMsg("Waiting till process exiting")
            While (Not p.HasExited)
                Dim SS = SR2.ReadLine()
                'DebugMsg(SS)
                ShellMsg(SS)
                S2 = S2 & SS & vbCrLf
                If (elapsedTime.Elapsed.Seconds > timeOut) Then

                    p.Kill()
                    p.Close()
                    p.Dispose()
                    p = Nothing

                    elapsedTime.Stop()
                    elapsedTime = Nothing
                    Exit While
                End If
            End While
            DebugMsg("Process Completed")
        End If
        DebugMsg("Getting Output")

        Dim SR = p.StandardOutput
        Dim S = SR.ReadToEnd

        S2 = S2 & SR2.ReadToEnd
        Return S2
    End Function
End Class
