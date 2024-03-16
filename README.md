# ImageCropView.MAUI
.NET MAUI replacement for the Xamarin Forms ImageCropView for Xamarin Forms library found here https://github.com/daniel-luberda/DLToolkit.Forms.Controls/tree/master/ImageCropView

This started out as a direct port of the ImageCropView for Xamarin Forms library, but the ImageCropView used FFImageLoading, this is not well supported on .NET MAUI. This implementation uses a GraphicsView Canvas and IImage under the covers so there are no third party packages needed. There were some things about the original implementation I didn't care for, so there are some changes from the original implementation. Hopefully if you are using it a direct replacement the changes won't be too difficult.

NOTE: There is a bug in MAUI on Android currently where PanGestureRecognizer doesn't behave well inside a ScrollView, so make sure you don't have a ScrollView wrapping this control. Issue details here: https://github.com/dotnet/maui/issues/17319





