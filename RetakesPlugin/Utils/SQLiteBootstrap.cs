using System.Runtime.InteropServices;

namespace RetakesPlugin.Utils;

/// <summary>
/// One-time initializer for the SQLite native library.
/// Both GunsDatabase and StatsDatabase delegate here to avoid
/// registering a second DllImportResolver on the same assembly.
/// </summary>
public static class SQLiteBootstrap
{
    private static bool _initialized;
    private static readonly object _lock = new();

    public static void Initialize(string pluginDirectory)
    {
        lock (_lock)
        {
            if (_initialized) return;
            _initialized = true;
        }

        try
        {
            var providerAssembly = typeof(SQLitePCL.SQLite3Provider_e_sqlite3).Assembly;
            NativeLibrary.SetDllImportResolver(providerAssembly, (libraryName, _, _) =>
            {
                if (libraryName != "e_sqlite3") return IntPtr.Zero;

                string[] candidates =
                [
                    Path.Combine(pluginDirectory, "libe_sqlite3.so"),
                    Path.Combine(pluginDirectory, "runtimes", "linux-x64", "native", "libe_sqlite3.so"),
                ];

                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                    {
                        Logger.LogInfo("SQLite", $"Loading native library from {path}");
                        return NativeLibrary.Load(path);
                    }
                }

                Logger.LogWarning("SQLite", "libe_sqlite3.so not found — SQLite persistence will be unavailable.");
                return IntPtr.Zero;
            });

            SQLitePCL.Batteries_V2.Init();
            Logger.LogInfo("SQLite", "Native library initialised successfully");
        }
        catch (Exception ex)
        {
            Logger.LogException("SQLite", ex);
        }
    }
}
