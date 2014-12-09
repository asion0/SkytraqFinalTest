using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;

// 組件的一般資訊是由下列的屬性集控制。
// 變更這些屬性的值即可修改組件的相關
// 資訊。
[assembly: AssemblyTitle("FinalTestV8")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("FinalTestV8")]
[assembly: AssemblyCopyright("Copyright ©  2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 將 ComVisible 設定為 false 會使得這個組件中的型別
// 對 COM 元件而言為不可見。如果您需要從 COM 存取這個組件中
// 的型別，請在該型別上將 ComVisible 屬性設定為 true。
[assembly: ComVisible(false)]

// 下列 GUID 為專案公開 (Expose) 至 COM 時所要使用的 typelib ID
[assembly: Guid("28386710-c315-42f3-9c0d-64c536f084fF")]

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
[assembly: AssemblyVersion("1.0.0.018")]
[assembly: AssemblyFileVersion("1.0.0.018")]

// 1.0.0.01 - first version.
// 1.0.0.02 - Does not save default setting avoid exception.
// 1.0.0.03 - Adjust time out and add message log.
// 1.0.0.05 - V815 test program.
// 1.0.0.06 - Add RTC check and show message.
// 1.0.0.07 - Support V822 Final Test
// 1.0.0.011 - Support V815 read test setting from profile.
// 1.0.0.012 - Fix V822 IO Test srec GPIO 3 fail issue.
// 1.0.0.013 - Display module name and prom file(in V822)
// 1.0.0.017 - Support V816 Final Test
// 1.0.0.018 - Update V815 Final Test, test rtc after SNR test.

[assembly: NeutralResourcesLanguageAttribute("")]
