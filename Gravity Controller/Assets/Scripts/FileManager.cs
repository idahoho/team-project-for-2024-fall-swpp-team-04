using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class FileManager
{
	public static bool WriteToFile(string a_FileName, string a_FileContents)
	{
		var fullPath = Path.Combine(Application.persistentDataPath, a_FileName);

		try
		{
			File.WriteAllText(fullPath, a_FileContents);
			return true;
		}
		catch (Exception e)
		{
			Debug.LogError($"Failed to write to {fullPath} with exception {e}");
			return false;
		}
	}

	public static bool LoadFromFile(string a_FileName, out string result)
	{
		var fullPath = Path.Combine(Application.persistentDataPath, a_FileName);

		try
		{
			result = File.ReadAllText(fullPath);
			return true;
		}
		catch (Exception e)
		{
			Debug.LogError($"Failed to read from {fullPath} with exception {e}");
			result = "";
			return false;
		}
	}
}

// Source: https://github.com/UnityTechnologies/UniteNow20-Persistent-Data?tab=MIT-1-ov-file

/*
MIT License

Copyright (c) 2020 Bronson Zgeb

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */