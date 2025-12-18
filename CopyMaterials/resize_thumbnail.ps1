# PowerShell script to resize thumbnail.png to 512x512 while maintaining aspect ratio
$thumbnailPath = Join-Path $PSScriptRoot "screenshots\thumbnail.png"

if (-not (Test-Path $thumbnailPath)) {
    Write-Error "thumbnail.png not found at: $thumbnailPath"
    exit 1
}

try {
    # Load System.Drawing assembly
    Add-Type -AssemblyName System.Drawing
    
    # Load the original image
    $originalImage = [System.Drawing.Image]::FromFile($thumbnailPath)
    $originalWidth = $originalImage.Width
    $originalHeight = $originalImage.Height
    
    Write-Host "Original size: ${originalWidth}x${originalHeight}"
    
    # Calculate new size maintaining aspect ratio
    $targetSize = 512
    $ratio = [Math]::Min($targetSize / $originalWidth, $targetSize / $originalHeight)
    $newWidth = [int]($originalWidth * $ratio)
    $newHeight = [int]($originalHeight * $ratio)
    
    # Create a new 512x512 image with white background
    $newImage = New-Object System.Drawing.Bitmap($targetSize, $targetSize)
    $graphics = [System.Drawing.Graphics]::FromImage($newImage)
    
    # Set high quality rendering
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    
    # Fill with white background
    $graphics.Clear([System.Drawing.Color]::White)
    
    # Calculate position to center the image
    $x = ($targetSize - $newWidth) / 2
    $y = ($targetSize - $newHeight) / 2
    
    # Draw the resized image centered
    $graphics.DrawImage($originalImage, $x, $y, $newWidth, $newHeight)
    
    # Save the new image
    $newImage.Save($thumbnailPath, [System.Drawing.Imaging.ImageFormat]::Png)
    
    # Clean up
    $graphics.Dispose()
    $newImage.Dispose()
    $originalImage.Dispose()
    
    Write-Host "Thumbnail resized to 512x512 (maintaining aspect ratio: ${newWidth}x${newHeight} centered)"
    Write-Host "Saved to: $thumbnailPath"
} catch {
    Write-Error "Error resizing thumbnail: $_"
    exit 1
}
