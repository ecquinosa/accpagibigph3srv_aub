
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace accpagibigph3srv
{
    class FileCompression
    {
                
        //public static bool Compress(string directoryPath, ref string strZipFile)
        public static bool Compress(string directoryPath, string repo, ref string strZipFile)
        {
            try
            {
                // Depending on the directory this could be very large and would require more attention
                // in a commercial package.
                string[] filenames = Directory.GetFiles(directoryPath);

                //strZipFile = directoryPath + Convert.ToString(".zip");
                strZipFile = repo + Convert.ToString(".zip");

                // 'using' statements guarantee the stream is closed properly which is a big source
                // of problems otherwise.  Its exception safe as well which is great.
                using (ZipOutputStream s = new ZipOutputStream(File.Create(strZipFile)))
                {
                    s.SetLevel(9);
                    // 0 - store only to 9 - means best compression
                    byte[] buffer = new byte[4096];

                    foreach (string file_1 in filenames)
                    {

                        // Using GetFileName makes the result compatible with XP
                        // as the resulting path is not absolute.
                        ZipEntry entry = new ZipEntry(Path.GetFileName(file_1));

                        // Setup the entry data as required.

                        // Crc and size are handled by the library for seakable streams
                        // so no need to do them here.

                        // Could also use the last write time or similar for the file.
                        entry.DateTime = Utilities.SystemDate;
                        s.PutNextEntry(entry);

                        using (FileStream fs = File.OpenRead(file_1))
                        {

                            // Using a fixed size buffer here makes no noticeable difference for output
                            // but keeps a lid on memory usage.
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            }
                            while (sourceBytes > 0);
                        }
                    }

                    // Finish/Close arent needed strictly as the using statement does this automatically

                    // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                    // the created file would be invalid.
                    s.Finish();

                    // Close is important to wrap things up and unlock the file.
                    s.Close();

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Utilities.SaveToErrorLog(Utilities.TimeStamp() + String.Format("Failed to compress {0}", Utilities.SessionReference()))
                return false;
            }
        }

        public bool addFolderToZip(ZipFile f, string root, string folder)
        {
            try
            {
                string relative = folder.Substring(root.Length);
                if (relative.Length > 0)
                    f.AddDirectory(relative);
                foreach (string file in Directory.GetFiles(folder))
                {
                    relative = file.Substring(root.Length);
                    f.Add(file, relative);
                }                

                return true;
            }
            catch (Exception ex)
            {
                //_ErrorMessage = ex.Message;
                return false;
            }
        }

        //public int UpdateExistingZip(string strZipFile, string strDirectory, ref System.Text.StringBuilder sb)
        //{
        //    int intError = 0;

        //    ZipFile zipFile = new ZipFile(strZipFile);

        //    // Must call BeginUpdate to start, and CommitUpdate at the end.
        //    zipFile.BeginUpdate();
            
        //    try
        //    {
        //        foreach (string strFile in Directory.GetFiles(strDirectory))
        //            zipFile.Add(strFile, Path.GetFileName(strFile));
        //    }
        //    catch (Exception ex)
        //    {
        //        intError += 1;
        //        sb.AppendLine(string.Format("UpdateExistingZip(): Runtime error encountered {0}", ex.Message));
        //    }

        //    // Continue calling .Add until finished.

        //    // Both CommitUpdate and Close must be called.
        //    zipFile.CommitUpdate();
        //    zipFile.Close();

        //    return intError;
        //}

        //public bool ExtractZipFile(string strZipFile, string strDirectory)
        //{
        //    ZipFile zf;
        //    FileStream fs = File.OpenRead(strZipFile);
        //    try
        //    {
        //        zf = new ZipFile(fs);                

        //        int intCntr = 1;

        //        foreach (ZipEntry zipEntry in zf)
        //        {
        //            if (!zipEntry.IsFile)
        //                // Ignore directories
        //                continue;
        //            String entryFileName = zipEntry.Name;                    

        //            if (entryFileName.Contains("0120160114H3ID168004"))
        //                Console.WriteLine("TEST");

        //            byte[] buffer = new byte[4096];
        //            // 4K is optimum
        //            Stream zipStream = zf.GetInputStream(zipEntry);

        //            // Manipulate the output filename here as desired.
        //            String fullZipToPath = Path.Combine(strDirectory, entryFileName);
        //            string directoryName = Path.GetDirectoryName(fullZipToPath);
        //            if (directoryName.Length > 0)
        //                Directory.CreateDirectory(directoryName);

        //            // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
        //            // of the file, but does not waste memory.
        //            // The "using" will close the stream even if an exception occurs.
        //            using (FileStream streamWriter = File.Create(fullZipToPath))
        //            {
        //                StreamUtils.Copy(zipStream, streamWriter, buffer);
        //            }

        //            intCntr += 1;
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        //_ErrorMessage = ex.Message;
        //        return false;
        //    }
        //    finally
        //    {
        //        if (zf != null)
        //        {
        //            zf.IsStreamOwner = true;
        //            // Makes close also shut the underlying stream
        //            // Ensure we release resources
        //            // fs.Close()
        //            zf.Close();
        //            // fs = Nothing
        //            zf = null/* TODO Change to default(_) if this is not a reference type */;
        //        }
        //    }
        //}




    }
}
