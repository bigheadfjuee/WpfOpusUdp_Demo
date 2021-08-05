# WpfOpusUdp_Demo
 WPF opus audio udp sender demo

# Reference
## OpusDotNet
https://github.com/mrphil2105/OpusDotNet

使用 專案裡面的 opus.dll 和 OpusDotNet.dll

## Opus.NET
https://github.com/DevJohnC/Opus.NET

專案裡面的 opus.dll 版本較舊了

僅參考 _waveIn_DataAvailable 裡面對即時音訊資料的處理

## 注意事項
● NAudio v2.x 改版較多，此專案使用 v1.10.0

● 舊版 .Net Framework 不支援 UdpClient 的 SendAsync 方法，此專案使用 4.6

● 建置 > 平台目標只能限定為 x64 或 x86，不能用預設的 Any CPU (opus.dll)

因為 _waveIn_DataAvailable 裡用到 pointer
所以要勾選「允許不安全的程式碼」

