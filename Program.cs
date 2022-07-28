// See https://aka.ms/new-console-template for more information

using System.Text;

public class Program
{
	public static string BaseDirectory = System.Environment.GetEnvironmentVariable("TESTS_DIR")
	                                   ?? AppDomain.CurrentDomain.BaseDirectory;

	private static Random random = new Random();

	public static string RandomString(int length)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}

	public static async Task Main(string[] args)
	{
		Console.WriteLine("test");

		for (int i = 100; i <= 102_400; i += 100)
		{
			await TestFile(i);
		}

		Console.WriteLine("done");
		await Task.Delay(5_000);
		Console.ReadLine();
	}

	public static async Task TestFile(int baseLength)
	{
		Console.WriteLine("[!] testing with len=" + baseLength);
		var dir = "tests";
		var fname = RandomString(8) + ".tmp";
		for (int i = 0; i < 25; ++i)
		{
			int length = baseLength + random.Next(-50, 50);

			await GenerateFile(dir, fname, length);
			await Task.Delay(25);

			var fn = Resolve(dir, fname);
			var bytes = await File.ReadAllBytesAsync(fn);
			if (bytes.Length != length)
			{
				Console.WriteLine($"[!] test failed: fn={fn}, expected={length}, got={bytes.Length} difference={bytes.Length-length}");
				return;
			}
			await Task.Delay(25);
		}
	}

	public static Task GenerateFile(string bucket, string fn, int length)
	{
		string str = RandomString(length);
		return FileWrite(bucket, fn, str);
	}

	public static Task FileWrite(string bucket, string fname, string data)
	{
		var encoder = new UTF8Encoding();
		byte[] bytes = encoder.GetBytes(data);
		return FileWrite(bucket, fname, bytes);
	}

	public static Task FileWrite(string bucket, string fname, byte[] data)
	{
		return Task.Run(async () =>
		{
			await MakeDirectories(bucket, fname, true);
			string path = Resolve(bucket, fname);
			// this.Logger.Debug("LocalFS: write file '{path}' with {size} bytes", path, data.Length);
			return File.WriteAllBytesAsync(path, data);
		});
	}
	public static Task MakeDirectories(string bucket, string fname, bool fromFile = false)
	{
		return Task.Run(() =>
		{
			string path = Resolve(bucket, fname, fromFile);
			if (!Directory.Exists(path)) {
				// this.Logger.Debug("Creating new directory '{Dir}'", path);
				Directory.CreateDirectory(path);
			}
		});
	}
	public static string Resolve(string bucket, string fname, bool directoryFromFile = false)
	{
		string path = Path.Join(BaseDirectory, bucket, fname);
		return directoryFromFile ? Path.GetDirectoryName(path) : path;
	}
}
