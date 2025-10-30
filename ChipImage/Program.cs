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
			var inputOption = new Option<string>("--input", "-i")
			{
				Description = "The input file to process.",
				Required = true
			};
			rootCommand.Options.Add(inputOption);
			//
			var outputDirOption = new Option<string>("--outdir", "-o")
			{
				DefaultValueFactory = (dummy) => ".",
				Description = "The output directory to save the tiles in. The trailing backslash is optional and will be added if not present."
			};
			rootCommand.Options.Add(outputDirOption);
			//
			var maxMemOption = new Option<int>("--maxmem", "-mm")
			{
				DefaultValueFactory = (dummy) => 5000,
				Description = "The maximum amount of memory to use in megabytes."
			};
			rootCommand.Options.Add(maxMemOption);
			//
			var verboseOption = new Option<bool>("--verbose", "-v")
			{
				DefaultValueFactory = (dummy) => false,
				Description = "Whether to print progress information to the console."
			};
			rootCommand.Options.Add(verboseOption);
			//
			var widthOption = new Option<int>("--width", "-w")
			{
				Description = "The width of the output images, specify in conjunction with height."
			};
			rootCommand.Options.Add(widthOption);
			//
			var heightOption = new Option<int>("--height", "-ht")
			{
				Description = "The height of the output images, specify in conjunction with width."
			};
			rootCommand.Options.Add(heightOption);
			//
			var colsOption = new Option<int>("--columns", "-c")
			{
				Description = "The number of output images in the horizontal direction, specify in conjunction with rows as an alternative to specifying width and height."
			};
			rootCommand.Options.Add(colsOption);
			//
			var rowsOption = new Option<int>("--rows", "-r")
			{
				Description = "The number of output images in the vertical direction, specify in conjunction with columns as an alternative to specifying width and height."
			};
			rootCommand.Options.Add(rowsOption);
			//
			var prefixOption = new Option<string>("--prefix", "-p")
			{
				DefaultValueFactory = (dummy) => "",
				Description = "The prefix to prepend to each output filename."
			};
			rootCommand.Options.Add(prefixOption);
			//
			var suffixOption = new Option<string>("--suffx", "-s")
			{
				DefaultValueFactory = (dummy) => "",
				Description = "The suffix to prepend to each output filename."
			};
			rootCommand.Options.Add(suffixOption);
			//
			var extOption = new Option<string>("--ext", "-e")
			{
				DefaultValueFactory = (dummy) => ".bmp",
				Description = "The image file extension to use including the leading dot."
			};
			rootCommand.Options.Add(extOption);
			//
			rootCommand.SetAction((pr) =>
			{
				var input = pr.GetValue(inputOption) ?? throw new Exception("--input was null, which it should never be.");
				var outDir = pr.GetValue(outputDirOption) ?? throw new Exception("--output was null, which it should never be.");
				var maxMem = pr.GetValue(maxMemOption);
				var verbose = pr.GetValue(verboseOption);
				var width = pr.GetValue(widthOption);
				var height = pr.GetValue(heightOption);
				var columns = pr.GetValue(colsOption);
				var rows = pr.GetValue(rowsOption);
				var prefix = pr.GetValue(prefixOption) ?? throw new Exception("--prefix was null, which it should never be.");
				var suffix = pr.GetValue(suffixOption) ?? throw new Exception("--suffix was null, which it should never be.");
				var ext = pr.GetValue(extOption);

				try
				{
					var whSpecified = width != 0 && height != 0;
					var crSpecified = columns != 0 && rows != 0;
					
					if (verbose)
						Console.WriteLine($"Running with:\n\t{pr}\n");

					if (!whSpecified && !crSpecified)
						throw new Exception("Neither width and height or columns and rows were specified. Provide one pair or the other.");

					using var chipper = new ImageChipper.ImageChipper(input, maxMem, maxMem, verbose)
					{
						Prefix = prefix,
						Suffix = suffix
					};

					if (whSpecified)
						chipper.ChipByDimensions(outDir, width, height, ".bmp");
					else if (crSpecified)
						chipper.ChipByCount(outDir, columns, rows, ".bmp");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to split image:\r\n\t{ex.Message}");
				}
			});
			return rootCommand.Parse(args).Invoke();
		}
	}
}