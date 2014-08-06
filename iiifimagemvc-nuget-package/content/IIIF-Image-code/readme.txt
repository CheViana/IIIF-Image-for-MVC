Path to images folder is set in web.config - appSettings, key imagesFolder.
Method that you should call inside your Application_Start() method in the global.asax.cs class:

ImagesRouteConfig.RegisterRoutes(RouteTable.Routes);

Id of image is it's name without format. F.ex. for "myimage.jpg" id is "myimage".
Then full image can be accessed by url: /images/myimage/full/full/0/native.jpg

you can use in html: <img src="/images/myimage/full/full/0/native.jpg"/>

Tile of image: /images/myimage/0,10,100,200/full/0/native.jpg
Scaled by 50% tile of image: /images/myimage/0,10,100,200/pct:50/0/native.jpg
Read iiif.io for full implemented API reference.

If you want your images to be stored not in the folder or to have custom id logic,
you can create derived of ImageProvider class with overriden GetImage method and use it in ImageController.