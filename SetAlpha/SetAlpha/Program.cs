using System.Drawing;
using System.Drawing.Imaging;

if (args.Length == 2)
{
    string color = "";
    string alpha = "";

    foreach (string item in args)
    {
        if (File.Exists(item) && Path.GetExtension(item).ToLower() == ".png")
        {
            if (Path.GetFileNameWithoutExtension(item).ToLower().EndsWith("_color"))
            {
                color = item;
            }
            else if (Path.GetFileNameWithoutExtension(item).ToLower().EndsWith("_alpha"))
            {
                alpha = item;
            }
        }
    }

    if (color != "" && alpha != "")
    {
        MergeAlpha(color, alpha);
        return;
    }
}

if (args.Length > 0)
{
    foreach (string item in args)
    {
        if (File.Exists(args[0]) && Path.GetExtension(args[0]).ToLower() == ".png")
        {
            SplitAlpha(args[0]);
        }
    }
}
else
{
    Console.WriteLine("Usage:");
    Console.WriteLine("\tSplit:\tSetAlpha \"file.png\"");
    Console.WriteLine("\tMerge:\tSetAlpha \"file_color.png\" \"file_alpha.png\"");
}

void SplitAlpha(string input)
{
    Bitmap source = new Bitmap(input, true);
    if (source.PixelFormat != PixelFormat.Format32bppArgb)
    {
        Console.WriteLine("PNG needs to be 32bppArgb");
        return;
    }

    Bitmap color = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
    Bitmap alpha = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

    BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadWrite, source.PixelFormat);
    BitmapData colorData = color.LockBits(new Rectangle(0, 0, color.Width, color.Height), ImageLockMode.ReadWrite, source.PixelFormat);
    BitmapData alphaData = alpha.LockBits(new Rectangle(0, 0, alpha.Width, alpha.Height), ImageLockMode.ReadWrite, source.PixelFormat);


    int pixelsize = Image.GetPixelFormatSize(source.PixelFormat) / 8;

    unsafe
    {
        for (int y = 0; y < sourceData.Height; y++)
        {
            byte* rowSource = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
            byte* rowColor = (byte*)colorData.Scan0 + (y * colorData.Stride);
            byte* rowAlpha = (byte*)alphaData.Scan0 + (y * alphaData.Stride);

            for (int x = 0; x < sourceData.Width; x++)
            {
                byte r = rowSource[x * pixelsize + 0];
                byte g = rowSource[x * pixelsize + 1];
                byte b = rowSource[x * pixelsize + 2];
                byte a = rowSource[x * pixelsize + 3];


                rowColor[x * pixelsize + 0] = r;
                rowColor[x * pixelsize + 1] = g;
                rowColor[x * pixelsize + 2] = b;
                rowColor[x * pixelsize + 3] = 255;

                rowAlpha[x * pixelsize + 0] = a;
                rowAlpha[x * pixelsize + 1] = a;
                rowAlpha[x * pixelsize + 2] = a;
                rowAlpha[x * pixelsize + 3] = 255;
            }
        }
    }


    source.UnlockBits(sourceData);
    color.UnlockBits(colorData);
    alpha.UnlockBits(alphaData);

    string outputFolder = Path.GetDirectoryName(input) ?? "";
    string fileName = Path.GetFileNameWithoutExtension(input);


    color.Save(Path.Combine(outputFolder, fileName + "_color.png"));
    alpha.Save(Path.Combine(outputFolder, fileName + "_alpha.png"));
}

void MergeAlpha(string colorFile, string alphaFile)
{

    Bitmap color = new Bitmap(colorFile, true);
    if (color.PixelFormat != PixelFormat.Format32bppArgb)
    {
        Console.WriteLine("PNG needs to be 32bppArgb");
        return;
    }

    Bitmap alpha = new Bitmap(alphaFile, true);
    if (alpha.PixelFormat != PixelFormat.Format32bppArgb)
    {
        Console.WriteLine("PNG needs to be 32bppArgb");
        return;
    }

    Bitmap output = new Bitmap(color.Width, color.Height, PixelFormat.Format32bppArgb);

    BitmapData outputData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
    BitmapData colorData = color.LockBits(new Rectangle(0, 0, color.Width, color.Height), ImageLockMode.ReadWrite, color.PixelFormat);
    BitmapData alphaData = alpha.LockBits(new Rectangle(0, 0, alpha.Width, alpha.Height), ImageLockMode.ReadWrite, alpha.PixelFormat);

    int pixelsize = Image.GetPixelFormatSize(color.PixelFormat) / 8;

    unsafe
    {
        for (int y = 0; y < outputData.Height; y++)
        {
            byte* rowOutput = (byte*)outputData.Scan0 + (y * outputData.Stride);
            byte* rowColor = (byte*)colorData.Scan0 + (y * colorData.Stride);
            byte* rowAlpha = (byte*)alphaData.Scan0 + (y * alphaData.Stride);

            for (int x = 0; x < outputData.Width; x++)
            {
                byte r = rowColor[x * pixelsize + 0];
                byte g = rowColor[x * pixelsize + 1];
                byte b = rowColor[x * pixelsize + 2];
                byte a = rowAlpha[x * pixelsize + 0];


                rowOutput[x * pixelsize + 0] = r;
                rowOutput[x * pixelsize + 1] = g;
                rowOutput[x * pixelsize + 2] = b;
                rowOutput[x * pixelsize + 3] = a;
            }
        }
    }


    output.UnlockBits(outputData);
    color.UnlockBits(colorData);
    alpha.UnlockBits(alphaData);

    string outputFolder = Path.GetDirectoryName(colorFile) ?? "";
    string fileName = Path.GetFileNameWithoutExtension(colorFile.Replace("_color", ""));

    string outputPath = Path.Combine(outputFolder, fileName + ".png");

    if (File.Exists(outputPath))
    {
        Console.WriteLine("File " + fileName + ".png exists. Overwrite? Y/N: ");
        if (Console.ReadKey().Key == ConsoleKey.Y)
        {
            output.Save(outputPath);
        }
    }
    else
    {

        output.Save(outputPath);
    }
}