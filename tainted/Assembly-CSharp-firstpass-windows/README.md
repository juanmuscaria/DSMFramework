# How to patch Assembly-CSharp-firstpass.dll

* You will need to decompile the dll source using some kind of tool (I used rider's built in decompiler).
* Drop all source here
* Add the following code to Steamworks.SteamAPI Init method
```
Debug.logger.Log("Trying to load Harmony and ModLoader.");
try
{
    string managedPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    Assembly harmony = Assembly.LoadFrom(Path.Combine(managedPath, "0Harmony.dll"));
    Assembly modLoader = Assembly.LoadFrom(Path.Combine(managedPath, "ModLoader.dll"));
    Type modLoaderType = modLoader.GetType("ModLoader.Loader", true);
    MethodInfo startMethod = modLoaderType.GetMethod("Start", new Type[0]);
    startMethod.Invoke(null, new object[0]);
}
catch (Exception e)
{
    Debug.LogError("Unable to start modloader!");
    Debug.LogException(e);
}
```
* Compile it.
* Can't compile because `(LayerMask) -1` is invalid for some reason.
* Remove it ignoring the consequences.
* Try compiling again.
* Now it compiled, replace the original file and drop the modloader and harmony dll together with it.