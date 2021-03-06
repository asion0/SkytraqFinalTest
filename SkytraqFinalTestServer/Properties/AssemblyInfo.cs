﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 組件的一般資訊是由下列的屬性集控制。
// 變更這些屬性的值即可修改組件的相關
// 資訊。
[assembly: AssemblyTitle("SkytraqFinalTestServer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SkytraqFinalTestServer")]
[assembly: AssemblyCopyright("Copyright ©  2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 將 ComVisible 設定為 false 會使得這個組件中的型別
// 對 COM 元件而言為不可見。如果您需要從 COM 存取這個組件中
// 的型別，請在該型別上將 ComVisible 屬性設定為 true。
[assembly: ComVisible(false)]

// 下列 GUID 為專案公開 (Expose) 至 COM 時所要使用的 typelib ID
[assembly: Guid("dad1e616-ef20-439f-a7b2-f866ee7ddac9")]

// 組件的版本資訊是由下列四項值構成:
//
//      主要版本
//      次要版本 
//      組建編號
//      修訂編號
//
// 您可以指定所有的值，也可以依照以下的方式，使用 '*' 將組建和修訂編號
// 指定為預設值:
// [assembly: AssemblyVersion("1.0.*")]
// 1.0.0.4 - Add time out for test, and quit process when server exit.
// 1.0.0.5 - 2014/03/18 Add log file for console windows.
// 1.0.0.6 - Add log in FinalTestV8, and adjust time out.
// 1.0.0.7 - Fixed FinalTestV8 no timeout issue when ic reboot.
// 1.0.0.8 - V815 Module support.
// 1.0.0.9 - Update FinalTestV8.exe to V1.0.0.06
// 1.0.0.10 - Update FinalTestV8.exe to V1.0.0.07, support V822 final test.
// 1.0.0.011 - Support V815 read test setting from SiteProfile.ini.
// 1.0.0.012 - Fix V822 IO Test srec GPIO 3 fail issue.
// 1.0.0.013 - Display Module name and prom file in Test UI.
// 1.0.0.014 - New UI and protocol from Hilo.
// 1.0.0.015 - New protocol from Hilo
// 1.0.0.016 - Fixed error respone strings.
// 1.0.0.017 - V816 Support.
// 1.0.0.018 - V815 Modify test module.
// 1.0.0.019 - V815 Modify test module.
// 1.0.0.020 - V815 Modify cold start.
// 1.0.0.021 - V815 Modify test RTC.
// 1.0.0.022 - V822 Modify for IO Testing in ROM Mode.
// 1.0.0.023 - V822 test in 115200 bps.
// 1.0.0.025 - Rebuild for new download loader.
// 1.0.0.026 - Add V828 support.
// 1.0.0.027 - Add V828 support.
// 1.0.0.028 - Fix close port issue.
// 1.0.0.029 - Change download loader for change baud rate.
// 1.0.0.030 - Change log format.
// 1.0.0.031 - Update to FinalTestV8 1.0.0.031.
// 1.0.0.032 - Add wait NMEA after change baud rate.
// 1.0.0.033 - Support clock offset test in V828
// 1.0.0.034 - Modify V828 Test flow.
// 1.0.0.035 - Change error code, modify test clock offset and modify v828download flow.
// 1.0.0.035 - Add V822 TestCRC function.

[assembly: AssemblyVersion("1.0.0.036")]
[assembly: AssemblyFileVersion("1.0.0.036")]
