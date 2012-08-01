/*
 *  Copyright 2012 Tamme Schichler <tammeschichler@googlemail.com>
 * 
 *  This file is part of TQ.Texture.Extract.
 *
 *  TQ.Texture.Extract is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  TQ.Texture.Extract is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with TQ.Texture.Extract.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Linq;
using TQ.Texture.DDSLib;

namespace TQ.Texture.Extract
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No files or directories specified.");
                return;
            }

            foreach (
                string file in args
                    .Where(Directory.Exists)
                    .SelectMany(arg => Directory.GetFiles(arg, "*.tex", SearchOption.AllDirectories)))
            {
                ExtractFile(file);
            }

            foreach (string file in args
                .Where(arg => arg.EndsWith(".tex", StringComparison.Ordinal))
                .Where(File.Exists))
            {
                ExtractFile(file);
            }
        }

        private static void ExtractFile(string file)
        {
            Console.WriteLine(file);
            byte[] data = File.ReadAllBytes(file);
            Texture texture = Texture.ParseTexture(data);

            for (int i = 0; i < texture.Frames.Length; i++)
            {
                string destframe = string.Format("{0}{1}.dds", file, texture.Frames.Length > 1 ? "." + i : "");

                DDS frame = DDS.Parse(texture.Frames[i]);
                if (frame.Variant == DDSVariant.Reversed)
                {
                    frame.Reverse();
                }

                frame.FixHeader();

                string destdir = Path.GetDirectoryName(destframe);
                if (destdir == null)
                {
                    Console.WriteLine("  Error extracting frame {0}: Destination directory no found.", i);
                    continue;
                }
                if (!Directory.Exists(destdir))
                {
                    Directory.CreateDirectory(destdir);
                }

                File.WriteAllBytes(destframe, frame.GetBytes());
            }
        }
    }
}