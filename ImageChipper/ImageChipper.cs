using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageChipper
{
	/// <summary>
	/// Provides functionality to split (chip) an image into smaller tiles and save or enumerate them.
	/// Because the underlying image processing is done using ImageSharp, this can handle very large images efficiently.
	/// </summary>
	public class ImageChipper : IDisposable
	{
		private bool disposed;
		private Image<Rgba32> image;

		/// <summary>
		/// Gets the filename of the loaded image.
		/// </summary>
		public string Filename { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use hexadecimal numbering for output filenames.
		/// </summary>
		public bool Hex { get; set; }

		/// <summary>
		/// Gets or sets the prefix to use for output filenames.
		/// </summary>
		public string Prefix { get; set; } = "";

		/// <summary>
		/// Gets or sets the suffix to use for output filenames.
		/// </summary>
		public string Suffix { get; set; } = "";

		/// <summary>
		/// Gets or sets a value indicating whether to output logging to the console.
		/// </summary>
		public bool Verbose { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageChipper"/> class and loads the specified image.
		/// </summary>
		/// <param name="filename">The path to the image file to load.</param>
		/// <param name="maxPoolMB">Optional. The maximum memory pool size in megabytes for the allocator.</param>
		/// <param name="maxAllocMB">Optional. The maximum allocation size in megabytes for the allocator.</param>
		/// <param name="verbose">Optional. Whether to enable logging to the console.</param>
		public ImageChipper(string filename, int? maxPoolMB = null, int? maxAllocMB = null, bool verbose = false)
		{
			Filename = filename;
			Verbose = verbose;

			if (maxPoolMB != null || maxAllocMB != null)
			{
				Configuration.Default.MemoryAllocator = MemoryAllocator.Create(new MemoryAllocatorOptions()
				{
					MaximumPoolSizeMegabytes = maxPoolMB,
					AllocationLimitMegabytes = maxAllocMB
				});
			}

			Log($"Loading original image: {filename}");
			image = SixLabors.ImageSharp.Image.Load<Rgba32>(Filename);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="ImageChipper"/> class.
		/// </summary>
		~ImageChipper()
		{
			Dispose(disposing: false);
		}

		/// <summary>
		/// Splits the image into a grid of tiles by specifying the number of columns and rows, and saves them to the output directory.
		/// </summary>
		/// <param name="outputDirectory">The directory to save the tiles.</param>
		/// <param name="tilesX">The number of columns.</param>
		/// <param name="tilesY">The number of rows.</param>
		/// <param name="extension">The file extension for the output images.</param>
		public void ChipByCount(string outputDirectory, int tilesX, int tilesY, string extension)
		{
			var tileWidth = (int)Math.Ceiling((double)image.Width / tilesX);
			var tileHeight = (int)Math.Ceiling((double)image.Height / tilesY);
			Chip(tilesX, tilesY, tileWidth, tileHeight, outputDirectory, extension);
		}

		/// <summary>
		/// Splits the image into tiles of the specified dimensions and saves them to the output directory.
		/// </summary>
		/// <param name="outputDirectory">The directory to save the tiles.</param>
		/// <param name="tileWidth">The width of each tile.</param>
		/// <param name="tileHeight">The height of each tile.</param>
		/// <param name="extension">The file extension for the output images.</param>
		public void ChipByDimensions(string outputDirectory, int tileWidth, int tileHeight, string extension)
		{
			var tilesX = (int)Math.Ceiling((double)image.Width / tileWidth);
			var tilesY = (int)Math.Ceiling((double)image.Height / tileHeight);
			Chip(tilesX, tilesY, tileWidth, tileHeight, outputDirectory, extension);
		}

		/// <summary>
		/// Enumerates the image tiles by specifying the number of columns and rows.
		/// </summary>
		/// <param name="tilesX">The number of columns.</param>
		/// <param name="tilesY">The number of rows.</param>
		/// <returns>An enumerable of <see cref="Image{Rgba32}"/> tiles.</returns>
		public IEnumerable<Image<Rgba32>> GetChipsByCount(int tilesX, int tilesY)
		{
			var tileWidth = (int)Math.Ceiling((double)image.Width / tilesX);
			var tileHeight = (int)Math.Ceiling((double)image.Height / tilesY);

			foreach (var image in GetChips(tilesX, tilesY, tileWidth, tileHeight))
				yield return image;
		}

		/// <summary>
		/// Enumerates the image tiles by specifying the dimensions of each tile.
		/// </summary>
		/// <param name="tileWidth">The width of each tile.</param>
		/// <param name="tileHeight">The height of each tile.</param>
		/// <returns>An enumerable of <see cref="Image{Rgba32}"/> tiles.</returns>
		public IEnumerable<Image<Rgba32>> GetChipsByDimension(int tileWidth, int tileHeight)
		{
			var tilesX = (int)Math.Ceiling((double)image.Width / tileWidth);
			var tilesY = (int)Math.Ceiling((double)image.Height / tileHeight);

			foreach (var image in GetChips(tilesX, tilesY, tileWidth, tileHeight))
				yield return image;
		}

		/// <summary>
		/// Disposes the resources used by the <see cref="ImageChipper"/> instance.
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="ImageChipper"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected void Dispose(bool disposing)
		{
			if (!disposed)
			{
				image?.Dispose();
				disposed = true;
			}
		}

		/// <summary>
		/// Extracts a rectangular region from the source image.
		/// </summary>
		/// <param name="sourceImage">The source image.</param>
		/// <param name="sourceArea">The rectangle area to extract.</param>
		/// <returns>A new <see cref="Image{Rgba32}"/> containing the extracted region.</returns>
		private static Image<Rgba32> Extract(Image<Rgba32> sourceImage, SixLabors.ImageSharp.Rectangle sourceArea)
		{
			Image<Rgba32> targetImage = new (sourceArea.Width, sourceArea.Height);
			sourceImage.ProcessPixelRows(targetImage, (sourceAccessor, targetAccessor) =>
			{
				for (int i = 0; i < sourceArea.Height; i++)
				{
					Span<Rgba32> sourceRow = sourceAccessor.GetRowSpan(sourceArea.Y + i);
					Span<Rgba32> targetRow = targetAccessor.GetRowSpan(i);
					sourceRow.Slice(sourceArea.X, sourceArea.Width).CopyTo(targetRow);
				}
			});
			return targetImage;
		}

		/// <summary>
		/// Splits the image into tiles and saves them to the specified output directory.
		/// </summary>
		/// <param name="tilesX">The number of columns.</param>
		/// <param name="tilesY">The number of rows.</param>
		/// <param name="tileWidth">The width of each tile.</param>
		/// <param name="tileHeight">The height of each tile.</param>
		/// <param name="outputDirectory">The directory to save the tiles.</param>
		/// <param name="extension">The file extension for the output images. Default: .bmp</param>
		private void Chip(int tilesX, int tilesY, int tileWidth, int tileHeight, string outputDirectory, string extension = ".bmp")
		{
			var pad = (int)Math.Max(Math.Ceiling(Math.Log(tilesX, 10)), Math.Ceiling(Math.Log(tilesY, 10)));

			if (extension.Length == 0)
				extension = ".bmp";
			else if (extension[0] != '.')
				extension = '.' + extension;

			Log($"Chipping original into {tilesY} rows, {tilesX} columns for a total of {tilesX * tilesY} images with dimensions {tileWidth}x{tileHeight}.");

			if (!System.IO.Path.EndsInDirectorySeparator(outputDirectory))
				outputDirectory += System.IO.Path.DirectorySeparatorChar;

			if (!System.IO.Path.Exists(outputDirectory))
				System.IO.Directory.CreateDirectory(outputDirectory);

			for (var y = 0; y < tilesY; y++)
			{
				for (var x = 0; x < tilesX; x++)
				{
					var xstr = x.ToString($"{(Hex ? "X" : "D")}{pad}");
					var ystr = y.ToString($"{(Hex ? "X" : "D")}{pad}");
					var filename = $"{Prefix}{ystr}_{xstr}{Suffix}{extension}";
					var rectX = x * tileWidth;
					var rectY = y * tileHeight;
					var maxW = Math.Min(image.Width - rectX, tileWidth);
					var maxH = Math.Min(image.Height - rectY, tileHeight);
					var sourceRect = new SixLabors.ImageSharp.Rectangle(rectX, rectY, maxW, maxH);
					Log($"\r\nChipping row {ystr}, col {xstr} to of size {maxW}x{maxH} to {filename}");
					using var cropped = Extract(image, sourceRect);
					Log($"\tSaving image");
					cropped.Save(Path.Combine(outputDirectory, filename));
				}
			}
		}

		/// <summary>
		/// Enumerates the image tiles as <see cref="Image{Rgba32}"/> objects.
		/// </summary>
		/// <param name="tilesX">The number of columns.</param>
		/// <param name="tilesY">The number of rows.</param>
		/// <param name="tileWidth">The width of each tile.</param>
		/// <param name="tileHeight">The height of each tile.</param>
		/// <returns>An enumerable of <see cref="Image{Rgba32}"/> tiles.</returns>
		private IEnumerable<Image<Rgba32>> GetChips(int tilesX, int tilesY, int tileWidth, int tileHeight)
		{
			for (var y = 0; y < tilesY; y++)
			{
				for (var x = 0; x < tilesX; x++)
				{
					var rectX = x * tileWidth;
					var rectY = y * tileHeight;
					var maxW = Math.Min(image.Width - rectX, tileWidth);
					var maxH = Math.Min(image.Height - rectY, tileHeight);
					var sourceRect = new SixLabors.ImageSharp.Rectangle(rectX, rectY, maxW, maxH);
					using var cropped = Extract(image, sourceRect);
					yield return cropped;
				}
			}
		}

		/// <summary>
		/// Writes a message to the console if verbose logging is enabled.
		/// </summary>
		/// <param name="message">The message to log.</param>
		private void Log(string message)
		{
			if (Verbose)
				Console.WriteLine(message);
		}
	}
}