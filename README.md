# RedditSharp for Unity Game Engine.  

[RedditSharp](https://github.com/CrustyJew/RedditSharp) is an open source C#, Reddit API library. The current library is intended for .NET 4.6.1 applications or higher.
This repository modified the old library to be compadible with the [Unity Game Engine](https://unity3d.com/) which runs on a subset of Mono 3.5. This library matches RedditSharp version 1.1.13. 

Changes made include:

 - Adding HtmlAgilityPack.dll, Newtonsoft.Json.dll, and System.Threading.dll.
 
 - Utility classes to handle missing Enum, System.Web, and Tuple features.
 
 - Surrounding all asynchronous fucntions with an #if(_HAS_ASYNC_) directive. 
 
 - Set the ServicePointManager to accept all SSL certificates (since we are only communicating with Reddit servers). 
 
 - Adding a constructor into WebAgent to intialize a CookieContainer. 

 
