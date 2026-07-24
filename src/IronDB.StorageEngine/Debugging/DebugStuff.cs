using IronDB.Core.Platform;
using IronDB.StorageEngine.Data.BTrees;
using IronDB.StorageEngine.Data.Fixed;
using IronDB.StorageEngine.Impl;
using System.Diagnostics;
using System.Numerics;

namespace IronDB.StorageEngine.Debugging;

public static class DebugStuff
{
    private const string css = @".css-treeview ul,
.css-treeview li
{
    padding: 0;
    margin: 0;
    list-style: none;
}
 
.css-treeview input
{
    position: absolute;
    opacity: 0;
}
 
.css-treeview
{
    font: normal 11px 'Segoe UI', Arial, Sans-serif;
    -moz-user-select: none;
    -webkit-user-select: none;
    user-select: none;
}
 
.css-treeview a
{
    color: #00f;
    text-decoration: none;
}
 
.css-treeview a:hover
{
    text-decoration: underline;
}
 
.css-treeview input + label + ul
{
    margin: 0 0 0 22px;
}
 
.css-treeview input ~ ul
{
    display: none;
}
 
.css-treeview label,
.css-treeview label::before
{
    cursor: pointer;
}
 
.css-treeview input:disabled + label
{
    cursor: default;
    opacity: .6;
}
 
.css-treeview input:checked:not(:disabled) ~ ul
{
    display: block;
}
 
.css-treeview label,
.css-treeview label::before
{
    background: url('http://experiments.wemakesites.net/pages/css3-treeview/example/icons.png') no-repeat;
}
 
.css-treeview label,
.css-treeview a,
.css-treeview label::before
{
    display: inline-block;
    height: 16px;
    line-height: 16px;
    vertical-align: middle;
}
 
.css-treeview label
{
    background-position: 18px 0;
}
 
.css-treeview label::before
{
    content: '';
    width: 16px;
    margin: 0 22px 0 0;
    vertical-align: middle;
    background-position: 0 -32px;
}
 
.css-treeview input:checked + label::before
{
    background-position: 0 -16px;
}
 
/* webkit adjacent element selector bugfix */
@media screen and (-webkit-min-device-pixel-ratio:0)
{
    .css-treeview 
    {
        -webkit-animation: webkit-adjacent-element-selector-bugfix infinite 1s;
    }
 
    @-webkit-keyframes webkit-adjacent-element-selector-bugfix 
    {
        from 
        { 
            padding: 0;
        } 
        to 
        { 
            padding: 0;
        }
    }
}";

    [Conditional("DEBUG")]
    public static void RenderAndShow_FixedSizeTree<TVal>(LowLevelTransaction tx, FixedSizeTree<TVal> fst)
        where TVal : unmanaged, IBinaryNumber<TVal>, IMinMaxValue<TVal>
    {
        var name = fst.Name;
        var tree = fst.Parent;
        RenderHtmlTreeView(writer =>
        {
            DumpFixedSizeTreeToStreamAsync(tx, fst, writer, name, tree).Wait();
        });
    }

    private static void RenderHtmlTreeView(Action<TextWriter> action)
    {
        if (Debugger.IsAttached == false)
            return;

        var output = Path.GetTempFileName() + ".html";
        using (var file = File.OpenWrite(output))
        using (var sw = new StreamWriter(file))
        {
            sw.WriteLine("<html><head><style>{0}</style></head><body>", css);
            action(sw);
            sw.WriteLine("</body></html>");
            sw.Flush();
        }

        OpenBrowser(output);
    }

    public static void OpenBrowser(string file)
    {
        if (PlatformDetails.RunningOnPosix == false)
        {
            string pathToChromeX86 = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            string pathToChromeX64 = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            string pathToEdge = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            string pathToChrome;

            if (File.Exists(pathToChromeX64))
                pathToChrome = pathToChromeX64;
            else if (File.Exists(pathToChromeX86))
                pathToChrome = pathToChromeX86;
            else if (File.Exists(pathToEdge))
                pathToChrome = pathToEdge;
            else
                throw new InvalidOperationException("Make sure path to chrome.exe is valid");

            var process = new Process
            {
                StartInfo =
                    {
                        FileName = pathToChrome,
                        Arguments = file,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                    }
            };
            process.Start();
            return;
        }

        if (PlatformDetails.RunningOnMacOsx)
        {
            Process.Start("open", file);
        }
        else
        {
            Process.Start("xdg-open", file);
        }
    }

    private static unsafe Task DumpFixedSizeTreeToStreamAsync<TVal>(
        LowLevelTransaction tx, 
        FixedSizeTree<TVal> fst,
        TextWriter writer, 
        Slice name, 
        Tree? tree)
          where TVal : unmanaged, IBinaryNumber<TVal>, IMinMaxValue<TVal>
    {
        throw new NotImplementedException();
    }
}
