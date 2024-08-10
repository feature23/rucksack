using System.Text;
using Xunit.Abstractions;

namespace Rucksack.Tests.Util;

public class TestOutputHelperTextWriterAdapter(ITestOutputHelper output)
    : TextWriter
{
    private string currentLine = "";

    public override void Write(char value)
    {
        if (value == '\n')
        {
            WriteCurrentLine();
        }
        else
        {
            currentLine += value;
        }
    }

    public override Encoding Encoding => Encoding.Default;

    private void WriteCurrentLine()
    {
        output.WriteLine(currentLine);
        currentLine = "";
    }

    protected override void Dispose(bool disposing)
    {
        if (currentLine != "")
        {
            WriteCurrentLine();
        }

        base.Dispose(disposing);
    }
}
