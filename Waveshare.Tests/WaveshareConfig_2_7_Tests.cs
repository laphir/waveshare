using System.Diagnostics;
using System.Text;

namespace Waveshare.Tests;

public class WaveshareConfig_2_7_Tests
{
    Waveshare.Config.WaveshareConfig_2_7HATv2 DisplayConfig { get; } = new Waveshare.Config.WaveshareConfig_2_7HATv2();

    [Fact]
    public void TestHardwareReset()
    {
        var commands = DisplayConfig.HardwareReset.Select(a => a.ToString());
        var expected = new string[] {
            "SetGpio Reset 1",
            "Sleep 20",
            "SetGpio Reset 0",
            "Sleep 2",
            "SetGpio Reset 1",
            "Sleep 20",
            "WaitForIdle",
        };

        Assert.Equal(expected, commands);
    }

    [Fact]
    public void TestSoftwareReset()
    {
        var commands = DisplayConfig.SoftwareReset.Select(a => a.ToString());
        var expected = new string[] {
            "SendCommand 0x12",
            "Sleep 10",
            "WaitForIdle",
        };

        Assert.Equal(expected, commands);
    }

    [Fact]
    public void TestSendCommandRawByte()
    {
        var commands = DisplayConfig.SendCommandRawByte(0xFF).Select(a => a.ToString());

        var expected = new string[]
        {
            "SendCommand 0xFF",
        };

        Assert.Equal(expected, commands);
    }

    // TODO: SendCommand() with all enum names.

    [Fact]
    public void TestSendCommandByte()
    {
        var commands = DisplayConfig.SendDataByte(0xFF).Select(a => a.ToString());

        var expected = new string[]
        {
            "SendData 0xFF"
        };

        Assert.Equal(expected, commands);
    }

    [Fact]
    public void TestSetRamXAddressAsFullWidth()
    {
        var commands = DisplayConfig.SetRamXAddressAsFullWidth
            .Select(a => a.ToString());

        var expected = new string[]
        {
            "SendCommand 0x44",
            "SendData 0x00",
            "SendData 0x15",        // 0x15 == 21 == (DisplayWidth) / 8  - 1
        };

        Assert.Equal(expected, commands);
    }

    [Fact]
    public void TestSetRamYAddressAsFullHeight()
    {
        var commands = DisplayConfig.SetRamYAddressAsFullHeight
            .Select(a => a.ToString());

        var expected = new string[]
        {
            "SendCommand 0x45",
            "SendData 0x00",
            "SendData 0x00",
            "SendData 0x07",        // 0x107 == 263 == DisplayHeight - 1
            "SendData 0x01",
        };

        Assert.Equal(expected, commands);
    }

    [Fact]
    public void TestSetRamXCounter()
    {
        var commands = DisplayConfig.SetRamXCounter(33)
            .Select(a => a.ToString());

        var expected = new string[]
        {
            "SendCommand 0x4E",
            "SendData 0x04",        // 0x04 == 4 == 33 / 8
        };

        Assert.Equal(expected, commands);
    }

    [Fact]
    public void TestSetRamYCounter()
    {
        var commands = DisplayConfig.SetRamYCounter(99)
            .Select(a => a.ToString());

        var expected = new string[]
        {
            "SendCommand 0x4F",
            "SendData 0x63",     // 0x63 == 99
            "SendData 0x00",
        };

        Assert.Equal(expected, commands);
    }

    [Fact]
    public void TestSetDataEntryMode()
    {
        var commands = DisplayConfig.SetDataEntryMode(false, true, true)
            .Select(a => a.ToString());

        var expected = new string[]
        {
            "SendCommand 0x11",
            "SendData 0x06",        // 0x06 == 0b110
                                    //             0: X Address Increment
                                    //            1: Y Address Increment
                                    //           1: Follow Y axis
        };

        Assert.Equal(expected, commands);
    }

    // TODO: SetBorderWaveForm
    // TODO: RefreshScreen_F7

    [Fact]
    public void TestDeepSleep()
    {
        var commands = DisplayConfig.TurnOff()
            .Where(a => a is not DebugMessageCommand)
            .Select(a => a.ToString());

        var expected = new string[]
        {
            "SendCommand 0x10",
            "SendData 0x01",
        };

        Assert.Equal(expected, commands);
    }

    //[Fact]
    // This is for understanding EPD_2IN7_V2_4GrayDisplay() logic.
    // This is the investigation result:
    //
    // Input is 2 bit per pixel, 4 pixels per byte, stream.
    // Input color scheme:
    //      White Gray1 Gray2 Black
    //      11    10    01    00
    // 
    // Output uses a different color code. Basically bit-value is negated.
    //      White Gray1 Gray2 Black
    //      00    01    10    11
    // Then store high bit and low bit in a different stream.
    //      0x24 data - low bit
    //      0x26 data - high bit

    private void Test4GrayDisplay()
    {
        byte[] data = new byte[2];
        //          0001 1011
        //      AA: 1010 1010
        //      CC: 1100 1100
        data[0] = (0 << 6) | (1 << 4) | (2 << 2) | 3;
        data[1] = (0 << 6) | (1 << 4) | (2 << 2) | 3;
        //for (int i = 0; i < data.Length; i++)
        //{
        //    data[i] = (byte)(i);
        //}

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < data.Length / 2; i++)
        {             //5808*4  46464
            int temp3 = 0;
            for (int j = 0; j < 2; j++)
            {
                // 1100 : C0 --> 0
                // 1000 : 80 --> 1
                // 0100 : 40 --> 0
                // 0000 : 00 --> 1
                int temp1 = data[i * 2 + j];
                for (int k = 0; k < 2; k++)
                {
                    int temp2 = temp1 & 0xC0;
                    if (temp2 == 0xC0)
                        temp3 |= 0x00;
                    else if (temp2 == 0x00)
                        temp3 |= 0x01;
                    else if (temp2 == 0x80)
                        temp3 |= 0x01;
                    else //0x40
                        temp3 |= 0x00;
                    temp3 <<= 1;

                    temp1 <<= 2;
                    temp2 = temp1 & 0xC0;
                    if (temp2 == 0xC0)
                        temp3 |= 0x00;
                    else if (temp2 == 0x00)
                        temp3 |= 0x01;
                    else if (temp2 == 0x80)
                        temp3 |= 0x01;
                    else    //0x40
                        temp3 |= 0x00;
                    if (j != 1 || k != 1)
                        temp3 <<= 1;

                    temp1 <<= 2;
                }

            }

            sb.AppendFormat("{0:X2} ", temp3);
        }
        sb.AppendLine();

        for (int i = 0; i < data.Length / 2; i++)
        {             //5808*4  46464
            int temp3 = 0;
            for (int j = 0; j < 2; j++)
            {
                int temp1 = data[i * 2 + j];
                for (int k = 0; k < 2; k++)
                {
                    int temp2 = temp1 & 0xC0;
                    if (temp2 == 0xC0)
                        temp3 |= 0x00;//white
                    else if (temp2 == 0x00)
                        temp3 |= 0x01;  //black
                    else if (temp2 == 0x80)
                        temp3 |= 0x00;  //gray1 ** different from above
                    else //0x40
                        temp3 |= 0x01;  //gray2 ** different from above
                    temp3 <<= 1;

                    temp1 <<= 2;
                    temp2 = temp1 & 0xC0;
                    if (temp2 == 0xC0)  //white
                        temp3 |= 0x00;
                    else if (temp2 == 0x00) //black
                        temp3 |= 0x01;
                    else if (temp2 == 0x80)
                        temp3 |= 0x00;  //gray1 ** different from above
                    else    //0x40
                        temp3 |= 0x01;  //gray2 ** different from above
                    if (j != 1 || k != 1)
                        temp3 <<= 1;

                    temp1 <<= 2;
                }
            }
            sb.AppendFormat("{0:X2} ", temp3);
        }

        Debug.WriteLine(sb.ToString());
    }

    static readonly byte[] LUT_DATA = new byte[159]
    {
        0x40,   0x48,   0x80,   0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x8,    0x48,   0x10,   0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x2,    0x48,   0x4,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x20,   0x48,   0x1,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0xA,    0x19,   0x0,    0x3,    0x8,    0x0,    0x0,
        0x14,   0x1,    0x0,    0x14,   0x1,    0x0,    0x3,
        0xA,    0x3,    0x0,    0x8,    0x19,   0x0,    0x0,
        0x1,    0x0,    0x0,    0x0,    0x0,    0x0,    0x1,
        0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
        0x22,   0x22,   0x22,   0x22,   0x22,   0x22,   0x0,    0x0,    0x0,
        //      154     155     156             158
        0x22,   0x17,   0x41,   0x0,    0x32,   0x1C
    };

    [Fact]
    public void TestLutReference()
    {
        Assert.Equal(0x1C, LUT_DATA[158]);      // VCom
        Assert.Equal(0x22, LUT_DATA[153]);      // EOPQ
        Assert.Equal(0x17, LUT_DATA[154]);      // VGH
        Assert.Equal(0x41, LUT_DATA[155]);      // VSH1
        Assert.Equal(0x00, LUT_DATA[156]);      // VSH2
        Assert.Equal(0x32, LUT_DATA[157]);      // VSL
    }

}
