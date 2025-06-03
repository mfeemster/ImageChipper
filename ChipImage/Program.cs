using System.CommandLine;

namespace ImageSplitter
{
	/// <summary>
	/// The main program class for the image splitting utility that creates tiles from larger images.
	/// This uses the ImageChipper library to handle the actual image processing, which in turn
	/// uses the ImageSharp library for image manipulation. The reason/benefit of this is that
	/// ImageSharp supports very large images, whereas other libraries such as
	/// Windows GDI+ and the various wrapper libraries around it such as SkiaSharp do not.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Entry point for the ChipImage.
		/// </summary>
		/// <param name="args">Command line arguments:
		/// -i, --input     : Required. The input image file to process
		/// -o, --outdir    : Optional. Output directory for tiles (default: current directory)
		/// -mm, --maxmem   : Optional. Maximum memory usage in MB (default: 5000)
		/// -v, --verbose   : Optional. Enable verbose output (default: false)
		/// -w, --width     : Optional. Width of output tiles (use with height)
		/// -h, --height    : Optional. Height of output tiles (use with width)
		/// -c, --columns   : Optional. Number of horizontal tiles (use with rows)
		/// -r, --rows      : Optional. Number of vertical tiles (use with columns)
		/// -p, --prefix    : Optional. Prefix for output filenames (default: none)
		/// -s, --suffix    : Optional. Suffix for output filenames (default: none)
		/// -e, --ext       : Optional. Output file extension (default: .bmp)</param>
		/// <returns>0 if successful, non-zero if an error occurred</returns>
		public static int Main(string[] args)
		{
			var rootCommand = new RootCommand("ChipImage will create image tiles out of a larger image.");
			//
			var inputOption = new Option<string>(["--input", "-i"], "The input file to process.") { IsRequired = true };
			rootCommand.Add(inputOption);
			//
			var outputDirOption = new Option<string>(["--outdir", "-o"], () => ".", "The output directory to save the tiles in. The trailing backslash is optional and will be added if not present.");
			rootCommand.Add(outputDirOption);
			//
			var maxMemOption = new Option<int>(["--maxmem", "-mm"], () => 5000, "The maximum amount of memory to use in megabytes.");
			rootCommand.Add(maxMemOption);
			//
			var verboseOption = new Option<bool>(["--verbose", "-v"], () => false, "Whether to print progress information to the console.");
			rootCommand.Add(verboseOption);
			//
			var widthOption = new Option<int>(["--width", "-w"], "The width of the output images, specify in conjunction with height.");
			rootCommand.Add(widthOption);
			//
			var heightOption = new Option<int>(["--height", "-h"], "The height of the output images, specify in conjunction with width.");
			rootCommand.Add(heightOption);
			//
			var colsOption = new Option<int>(["--columns", "-c"], "The number of output images in the horizontal direction, specify in conjunction with rows as an alternative to specifying width and height.");
			rootCommand.Add(colsOption);
			//
			var rowsOption = new Option<int>(["--rows", "-r"], "The number of output images in the vertical direction, specify in conjunction with columns as an alternative to specifying width and height.");
			rootCommand.Add(rowsOption);
			//
			var prefixOption = new Option<string>(["--prefix", "-p"], () => "", "The prefix to prepend to each output filename.");
			rootCommand.Add(prefixOption);
			//
			var suffixOption = new Option<string>(["--suffx", "-s"], () => "", "The suffix to prepend to each output filename.");
			rootCommand.Add(suffixOption);
			//
			var extOption = new Option<string>(["--ext", "-e"], () => ".bmp", "The image file extension to use including the leading dot.");
			rootCommand.Add(extOption);
			//
			rootCommand.SetHandler((context) =>
			{
				var input = context.ParseResult.GetValueForOption(inputOption) ?? throw new Exception("--input was null, which it should never be.");
				var outDir = context.ParseResult.GetValueForOption(outputDirOption) ?? throw new Exception("--output was null, which it should never be.");
				var maxMem = context.ParseResult.GetValueForOption(maxMemOption);
				var verbose = context.ParseResult.GetValueForOption(verboseOption);
				var width = context.ParseResult.GetValueForOption(widthOption);
				var height = context.ParseResult.GetValueForOption(heightOption);
				var columns = context.ParseResult.GetValueForOption(colsOption);
				var rows = context.ParseResult.GetValueForOption(rowsOption);
				var prefix = context.ParseResult.GetValueForOption(prefixOption) ?? throw new Exception("--prefix was null, which it should never be.");
				var suffix = context.ParseResult.GetValueForOption(suffixOption) ?? throw new Exception("--suffix was null, which it should never be.");
				var ext = context.ParseResult.GetValueForOption(extOption);

				try
				{
					using var chipper = new ImageChipper.ImageChipper(input, maxMem, maxMem, verbose)
					{
						Prefix = prefix,
						Suffix = suffix
					};

					if (width != 0 && height != 0)
						chipper.ChipByDimensions(outDir, width, height, ".bmp");
					else if (columns != 0 && rows != 0)
						chipper.ChipByCount(outDir, columns, rows, ".bmp");
					else
						throw new Exception("Neither width and height or columns and rows were specified. Provide one pair or the other.");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to split image:\r\n\t{ex.Message}");
				}
			});
			return rootCommand.Invoke(args);
		}
	}
}