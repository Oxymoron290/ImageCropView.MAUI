# ImageCropView.MAUI
.NET MAUI replacement for the Xamarin Forms ImageCropView library found here https://github.com/daniel-luberda/DLToolkit.Forms.Controls/tree/master/ImageCropView

This started out as a direct port of the ImageCropView package, but the ImageCropView package for Xamarin Forms used FFImageLoading, this is not well supported on .NET MAUI. This implementation uses a GraphicsView Canvas and IImage under the covers so there are no third party packages needed. 

NOTE: There is a bug in MAUI on Android currently where PanGestureRecognizer doesn't behave well inside a ScrollView, so make sure you don't have a ScrollView wrapping this control. Issue details here: https://github.com/dotnet/maui/issues/17319





