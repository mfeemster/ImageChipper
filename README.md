# ImageChipper
A library to split large images into smaller ones using the ImageSharp library.

## Example usage

### Split an image into tiles of size 4096x4096 and save each as a bitmap in the form of [row]_[col].bmp

```
using var chipper = new ImageChipper.ImageChipper("C:\\mybigimages\\largeimagefile.jpg", 7000, 7000, true)
chipper.ChipByDimensions("C:\\outputpath", 4096, 4096, ".bmp");
```

### Split an image into 100 tiles and save each as a bitmap in the form of [row]_[col].bmp

```
using var chipper = new ImageChipper.ImageChipper("C:\\mybigimages\\largeimagefile.jpg", 7000, 7000, true)
chipper.ChipByCount("C:\\outputpath", 10, 10, ".bmp");
```

### Use powershell to split an entire folder of images

```
$imageCount = 0

ls "C:\\mybigimages\\*.png" -file | ForEach-Object {
	$outdir = $_.DirectoryName + "\chips"
	$imageindex = $imageCount++
	$prefix = $imageindex.ToString() + "_"
#Uncomment these to debug.
#"Dir: " + $outdir
#"Image index: " + $imageindex
#"Prefix: " + $prefix
	.\ChipImage.exe -i $_.FullName -o $outdir -mm 7000 -v -w 4096 -h 4096 -p $prefix
}
```