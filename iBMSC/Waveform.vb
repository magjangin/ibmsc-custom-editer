Imports CSCore.Streams
Imports CSCore.Streams.Effects
Imports CSCore
Imports CSCore.Codecs


Partial Public Class MainWindow

    '----WaveForm Options
    Dim wWavL() As Single
    Dim wWavR() As Single
    Dim wLock As Boolean = True
    Dim wSampleRate As Integer
    Dim wPosition As Double = 0
    Dim wLeft As Integer = 50
    Dim wWidth As Integer = 100
    Dim wPrecision As Integer = 1

    Private Sub BWLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BWLoad.Click
        Dim xDWAV As New OpenFileDialog
        xDWAV.Filter = "Wave files (*.wav, *.ogg)" & "|*.wav;*.ogg"
        xDWAV.DefaultExt = "wav"
        xDWAV.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))

        If xDWAV.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub
        InitPath = ExcludeFileName(xDWAV.FileName)

        Using src = CSCore.Codecs.CodecFactory.Instance.GetCodec(xDWAV.FileName).ToStereo()
            Dim sampleSource = src.ToSampleSource()
            Dim bytesPerSample As Integer = Math.Max(1, src.WaveFormat.BitsPerSample \ 8)
            Dim frameCountLong As Long = (src.Length \ bytesPerSample) \ src.WaveFormat.Channels

            If frameCountLong <= 0 Then
                Erase wWavL
                Erase wWavR
                Throw New InvalidDataException("The selected audio file does not contain readable samples.")
            End If

            If frameCountLong > Integer.MaxValue - 1 Then
                Erase wWavL
                Erase wWavR
                Throw New OutOfMemoryException("The selected audio file is too large to draw as a waveform.")
            End If

            ReDim wWavL(CInt(frameCountLong - 1))
            ReDim wWavR(CInt(frameCountLong - 1))

            Dim buffer(Math.Max(src.WaveFormat.Channels * 4096, src.WaveFormat.Channels) - 1) As Single
            Dim frameIndex As Integer = 0
            Dim samplesRead As Integer

            Do
                samplesRead = sampleSource.Read(buffer, 0, buffer.Length)
                If samplesRead <= 0 Then Exit Do

                Dim sampleIndex As Integer = 0
                While sampleIndex + src.WaveFormat.Channels - 1 < samplesRead AndAlso frameIndex < wWavL.Length
                    wWavL(frameIndex) = buffer(sampleIndex)
                    wWavR(frameIndex) = buffer(sampleIndex + 1)
                    sampleIndex += src.WaveFormat.Channels
                    frameIndex += 1
                End While
            Loop

            If frameIndex = 0 Then
                Erase wWavL
                Erase wWavR
                Throw New InvalidDataException("The selected audio file does not contain readable samples.")
            End If

            If frameIndex < wWavL.Length Then
                ReDim Preserve wWavL(frameIndex - 1)
                ReDim Preserve wWavR(frameIndex - 1)
            End If

            wSampleRate = src.WaveFormat.SampleRate
        End Using
        RefreshPanelAll()

        TWFileName.Text = xDWAV.FileName
        TWFileName.Select(Len(xDWAV.FileName), 0)
    End Sub

    Private Sub BWClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BWClear.Click
        Erase wWavL
        Erase wWavR
        TWFileName.Text = "(" & Strings.None & ")"
        RefreshPanelAll()
    End Sub

    Private Sub BWLock_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BWLock.CheckedChanged
        wLock = BWLock.Checked
        TWPosition.Enabled = Not wLock
        TWPosition2.Enabled = Not wLock
        RefreshPanelAll()
    End Sub
End Class
