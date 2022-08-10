# JsonUnityVectors
Fix json net for unity Vectors

This is an edit for the json.net library I downloaded from nuget.
Original:
https://github.com/obarlik/Json.Net

# Installation.
1) Create a JsonNet folder in your project
2) Copy all the contents
3) Use:
using Json.Net;

4) Example:

string jstr = JsonNet.Serialize(new Vector3(13, 66, 66)) 
Vector3 res = JsonNet.Deserialize<Vector3>(jstr);

# Video
https://www.youtube.com/watch?v=JLGhHjlYqjU
