using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using System.Collections.Concurrent;
using System.IO.Packaging;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.Services
{
    public class SaveFile : IDisposable
    {
        public class SaveFileMetaData : IPackagePart
        {
            [Serializable]
            public class HistoryEntry
            {
                [XmlElement(ElementName = "TimeOpened", Form = XmlSchemaForm.Unqualified)]
                public String TimeOpened { get; set; } = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                [XmlElement(ElementName = "ProductKey", Form = XmlSchemaForm.Unqualified)]
                public String ProductKey { get; set; } = "N/A";
                [XmlElement(ElementName = "ErrorCount", Form = XmlSchemaForm.Unqualified)]
                public int ErrorCount { get; set; } = -1;
                [XmlElement(ElementName = "ErrorsReported", Form = XmlSchemaForm.Unqualified)]
                public int ErrorsReported { get; set; } = -1;
                [XmlElement(ElementName = "SaveFileVersion", Form = XmlSchemaForm.Unqualified)]
                public String Version { get; set; } = null;

                public HistoryEntry()
                {
                    Version = LocalStorageService.Activation[LocalStorageService.Field.Version];
                    ProductKey = LocalStorageService.Activation[LocalStorageService.Field.ProductKey];
                    ErrorCount = ErrorReporter.Errors;
                    ErrorsReported = ErrorReporter.ErrorsReported;
                }

                public HistoryEntry(XElement root)
                {
                    if (root.Element("Timestamp") != null)
                        TimeOpened = root.Element("Timestamp").Value;
                    if (root.Element("Version") != null)
                        Version = root.Element("Version").Value;
                    if (root.Element("ErrorCount") != null)
                        ErrorCount = Convert.ToInt32(root.Element("ErrorCount").Value);
                    if (root.Element("ErrorsReported") != null)
                        ErrorsReported = Convert.ToInt32(root.Element("ErrorsReported").Value);
                    if (root.Element("ProductKey") != null)
                        ProductKey = root.Element("ProductKey").Value;
                }

                public void AddToXml(XElement parent)
                {
                    parent.Add(new XElement("HistoryEntry", new XElement("Timestamp", TimeOpened), new XElement("Version", Version), new XElement("ErrorCount", ErrorCount),
                        new XElement("ErrorsReported", ErrorsReported.ToString()), new XElement("ProductKey", ProductKey)));
                }
            }


            public DateTime TimeOpened { get; private set; } = DateTime.UtcNow;
            public List<HistoryEntry> History { get; private set; } = new List<HistoryEntry>();
            private SaveFile SaveFile { get; set; } = null;
            public CVersion Version { get; private set; } = new CVersion();
            public SaveFileMetaData(SaveFile saveFile)
            {
                Uri = PackUriHelper.CreatePartUri(new Uri(typeof(SaveFileMetaData).ToString() + "/" + typeof(SaveFileMetaData).ToString() + "1.xml", UriKind.Relative));
                saveFile.SavePackage.CreatePart(Uri, MimeType, CompressionOption.Normal);
                UriCounters[typeof(SaveFileMetaData).ToString()] = new List<int>(new int[] { 1 });
                saveFile.CreatePackageLevelRelationship(Uri, typeof(SaveFileMetaData));
                SaveFile = saveFile;
            }

            public SaveFileMetaData(SaveFile saveFile, Uri u)
            {
                SaveFile = saveFile;
                this.Uri = u;
                UriCounters[typeof(SaveFileMetaData).ToString()] = new List<int>(new int[] { 1 });
                Load(SaveFile.SavePackage);
            }


            public void Save()
            {
                XDocument xDoc = new XDocument();
                xDoc.Add(new XElement("MetaData"));
                xDoc.Root.Add(new XElement("IATRelId", IATRelId));
                new HistoryEntry().AddToXml(xDoc.Root);
                foreach (HistoryEntry hist in History)
                    hist.AddToXml(xDoc.Root);
                XElement xElem = new XElement("UriCounters");
                foreach (String t in UriCounters.Keys)
                {
                    XElement uriElem = new XElement("UriCounter", new XAttribute("Type", t.ToString()));
                    foreach (int i in UriCounters[t])
                        uriElem.Add(new XElement("ConsumedValue", i.ToString()));
                    xElem.Add(uriElem);
                }
                xDoc.Root.Add(xElem);
                Stream s = Stream.Synchronized(SaveFile.GetWriteStream(this));
                try
                {
                    xDoc.Save(s);
                }
                finally
                {
                    s.Dispose();
                    SaveFile.ReleaseWriteStreamLock();
                }
            }

            public void Load(Package savePackage)
            {
                Stream s = Stream.Synchronized(SaveFile.GetReadStream(this));
                XDocument xDoc = null;
                try
                {
                    xDoc = XDocument.Load(s);
                    s.Dispose();
                }
                finally
                {
                    SaveFile.ReleaseReadStreamLock();
                }
                IATRelId = xDoc.Root.Element("IATRelId").Value;
                foreach (XElement elem in xDoc.Root.Elements("HistoryEntry"))
                    History.Add(new HistoryEntry(elem));
                Version = new CVersion(History.First().Version);
                foreach (XElement elem in xDoc.Root.Element("UriCounters").Elements())
                {
                    UriCounters[elem.Attribute("Type").Value] = new List<int>();
                    foreach (XElement consumedVal in elem.Elements("ConsumedValue"))
                        UriCounters[elem.Attribute("Type").Value].Add(Convert.ToInt32(consumedVal.Value));
                }
            }

            public void Dispose()
            {
                UriCounters.Clear();
            }
        }

        public static Thread SaveThread { get; private set; } = null;
        private SaveSplash SaveSplash { get; set; } = null;
        public bool IsDisposed { get; private set; } = false;
        public bool IsDisposing { get; private set; } = false;
        private Package SavePackage;
        private readonly CompressionOption Compression = CompressionOption.Normal;
        private CIAT _IAT = null;
        private CIATLayout _Layout = null;
        private FontPreferences _FontPreferences;
        private ConcurrentDictionary<Uri, CIATKey> Keys = new ConcurrentDictionary<Uri, CIATKey>();
        private ConcurrentDictionary<Uri, CIATItem> IATItems = new ConcurrentDictionary<Uri, CIATItem>();
        private ConcurrentDictionary<Uri, CIATBlock> IATBlocks = new ConcurrentDictionary<Uri, CIATBlock>();
        private ConcurrentDictionary<Uri, CInstructionScreen> InstructionScreens = new ConcurrentDictionary<Uri, CInstructionScreen>();
        private ConcurrentDictionary<Uri, CInstructionBlock> InstructionBlocks = new ConcurrentDictionary<Uri, CInstructionBlock>();
        private ConcurrentDictionary<Uri, CSurvey> Surveys = new ConcurrentDictionary<Uri, CSurvey>();
        private ConcurrentDictionary<Uri, AlternationGroup> AlternationGroups = new ConcurrentDictionary<Uri, AlternationGroup>();
        private ConcurrentDictionary<Uri, Images.IImage> IImages = new ConcurrentDictionary<Uri, Images.IImage>();
        private ConcurrentDictionary<Uri, ObservableUri> ObservableUris = new ConcurrentDictionary<Uri, ObservableUri>();
        private ConcurrentDictionary<Uri, CFontFile.FontItem> FontItems = new ConcurrentDictionary<Uri, CFontFile.FontItem>();
        private ConcurrentDictionary<Uri, CSurveyItem> SurveyItems = new ConcurrentDictionary<Uri, CSurveyItem>();
        private readonly ManualResetEvent DisposingEvent = new ManualResetEvent(true), ResizingLayoutEvent = new ManualResetEvent(true);
        private ConcurrentDictionary<Uri, DIBase> DIs = new ConcurrentDictionary<Uri, DIBase>();
        public ActivityLog ActivityLog { get; private set; } = new ActivityLog();

        private int ReadLockCount = 0;
        private bool FrozenForSave { get; set; }
        private System.Threading.Timer CacheTimer = null;
        private const int AutoSaveInterval = 30000;
        private readonly object ReadLocked = new object(), WriteLocked = new object();
        public readonly SaveFileMetaData MetaData;
        private ManualResetEvent PackageStreamOpen = new ManualResetEvent(true);
        public Images.ImageManager ImageManager { get; private set; }
        private readonly object layoutLock = new object();
        private readonly object saveLock = new object();
        public bool IsLoading { get; private set; } = false;
        private readonly ReaderWriterLockSlim ioLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public CVersion Version { get { return MetaData.Version; } }
        public List<SaveFileMetaData.HistoryEntry> History { get { return MetaData.History; } }
        private MemoryStream PackageStream;
        public SaveFile(String fileName, bool compressed, bool hidden)
        {
            ImageManager = new Images.ImageManager();
            ErrorReporter.Errors = 0;
            ErrorReporter.ErrorsReported = 0;
            DisposingEvent.WaitOne();
            FileInfo fi = new FileInfo(fileName);
            fi.Attributes &= ~FileAttributes.ReadOnly;
            bool validSaveFile = false;
            try
            {
                byte[] signedHash = new byte[512];
                /*
                      using (Stream workingFS = File.Open(WorkingSaveFilename, FileMode.Create, FileAccess.ReadWrite))
                              {
                                  using (FileStream saveFS = File.Open(fileName, FileMode.Open, FileAccess.Read))
                                  {
                                      long nBytes = fi.Length - 512;
                                      byte[] buff = new byte[65536];
                                      long nBytesRead = 0;
                                      while (nBytesRead < nBytes)
                                      {
                                          int bytes = saveFS.Read(buff, 0, (nBytes - nBytesRead < 65536) ? (int)(nBytes - nBytesRead) : 65536);
                                          workingFS.Write(buff, 0, bytes);
                                          nBytesRead += bytes;
                                      }
                                      saveFS.Read(signedHash, 0, 512);
                                  }
                              }*/

                /*
                PackageStream = new MemoryStream();
                using (FileStream saveFS = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    long nBytes = fi.Length - 512;
                    byte[] buff = new byte[65536];
                    long nBytesRead = 0;
                    while (nBytesRead < nBytes)
                    {
                        int bytes = saveFS.Read(buff, 0, (nBytes - nBytesRead < 65536) ? (int)(nBytes - nBytesRead) : 65536);
                        PackageStream.Write(buff, 0, bytes);
                        nBytesRead += bytes;
                    }
                    saveFS.Read(signedHash, 0, 512);
                }
                SHA256 sha = SHA256.Create();
                byte[] hash;*/
                /*
                using (Stream workingFS = File.Open(WorkingSaveFilename, FileMode.Open, FileAccess.Read))
                    hash = sha.ComputeHash(workingFS);
                */
                /*
                PackageStream.Seek(0, SeekOrigin.Begin);
                hash = sha.ComputeHash(PackageStream);
                RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
                rsaCrypt.ImportCspBlob(Convert.FromBase64String(Properties.Resources.sig));
                if (rsaCrypt.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), signedHash)) {
                    validSaveFile = true;
                    PackageStream.Seek(0, SeekOrigin.Begin);
                    SavePackage = Package.Open(PackageStream, FileMode.Open, FileAccess.ReadWrite);
                } else {
                    PackageStream.Dispose();
                    PackageStream = new MemoryStream();
 //                   File.Delete(WorkingSaveFilename);
   //                 File.Copy(fileName, WorkingSaveFilename);
                    SavePackage = Package.Open(PackageStream, FileMode.Open, FileAccess.ReadWrite);
                }
                if (!validSaveFile && (Version.CompareTo(new CVersion("1.1.1.29")) > 0) && (Version.CompareTo(new CVersion("1.1.1.43")) < 0))
                    throw new InvalidSaveFileException();                
                */
                PackageStream = new MemoryStream();
                byte[] bytes = new byte[8192];
                int nBytesRead = 0;
                using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    while (nBytesRead < fi.Length)
                    {
                        int nBytes = fs.Read(bytes, 0, bytes.Length);
                        PackageStream.Write(bytes, 0, nBytes);
                        nBytesRead += nBytes;
                    }
                }
                PackageStream.Seek(0, SeekOrigin.Begin);
                SavePackage = Package.Open(PackageStream, FileMode.Open, FileAccess.ReadWrite);
            }
            catch (Exception ex)
            {
                //                if (File.Exists(WorkingSaveFilename))
                //                  File.Delete(WorkingSaveFilename);
                //            File.Copy(fileName, WorkingSaveFilename);
                //          SavePackage = Package.Open(WorkingSaveFilename, FileMode.Open, FileAccess.ReadWrite);
                //        InvalidSaveFileReport.Report();
            }
            MetaData = new SaveFileMetaData(this, GetPackageLevelRelationship(typeof(SaveFileMetaData)).TargetUri);
            if (CVersion.Compare(new CVersion("1.1.1.13"), Version) < 0)
                RebuildSaveFileUris();
        }



        public SaveFile()
        {
            ImageManager = new Images.ImageManager();
            DisposingEvent.WaitOne();
            PackageStream = new MemoryStream();
            SavePackage = Package.Open(PackageStream, FileMode.Create, FileAccess.ReadWrite);
            MetaData = new SaveFileMetaData(this);
        }
        /*
                private String _WorkingSaveFilename = null;
                public String WorkingSaveFilename
                {
                    get
                    {
                        if (_WorkingSaveFilename != null)
                            return _WorkingSaveFilename;
                        int ctr = 0;
                        String appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create) + Path.DirectorySeparatorChar + "IATSoftware" + Path.DirectorySeparatorChar;
                        while (File.Exists(String.Format(appDataDir + Properties.Resources.sWorkingSaveFileName, ++ctr)))
                        {
                            try
                            {
                                File.Delete(String.Format(Properties.Resources.sWorkingSaveFileName, ctr));
                            }
                            catch (Exception) { }
                        }
                        ctr = 0;
                        FileStream fs = null;
                        while (_WorkingSaveFilename == null)
                        {
                            try
                            {
                                _WorkingSaveFilename = String.Format(String.Format(appDataDir + Properties.Resources.sWorkingSaveFileName, ++ctr));
                                fs = File.Open(_WorkingSaveFilename, FileMode.OpenOrCreate, FileAccess.Read);
                            }
                            catch (Exception ex)
                            {
                                _WorkingSaveFilename = null;
                            }
                            finally
                            {
                                if (fs != null)
                                    fs.Close();
                            }
                        }
                        return _WorkingSaveFilename;
                    }
                }
        */
        private void RebuildSaveFileUris()
        {
            ioLock.EnterUpgradeableReadLock();
            Uri u;
            try
            {
                MetaData.UriCounters.Clear();
                Regex objTypeExp = new Regex(@"^/IATClient\.[^/]+/(IATClient\..*?)([1-9][0-9]*).*?(?!=\.rel)");
                PackagePartCollection ppc = SavePackage.GetParts();
                foreach (PackagePart pp in ppc)
                {
                    u = pp.Uri;
                    if (!objTypeExp.IsMatch(u.ToString()))
                        continue;
                    String objType = objTypeExp.Match(u.ToString()).Groups[1].Value;
                    int ndx = Convert.ToInt32(objTypeExp.Match(u.ToString()).Groups[2].Value);
                    if (!MetaData.UriCounters.ContainsKey(objType))
                        MetaData.UriCounters[objType] = new List<int>();
                    MetaData.UriCounters[objType].Add(ndx);
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error upgrading save file Uris", ex));
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public List<CFontFile.FontItem> CheckForMissingFonts()
        {
            List<CFontFile.FontItem> utilizedFonts = new List<CFontFile.FontItem>();
            List<Uri> uris = GetRelationshipsByType(IAT.URI, typeof(CIAT), typeof(CFontFile.FontItem)).Select(pr => pr.TargetUri).ToList();
            foreach (Uri u in uris)
                utilizedFonts.Add(new CFontFile.FontItem(u));
            return utilizedFonts.Where(fi => CFontFile.AvailableFonts.Where(fd => fd.FamilyName == fi.FamilyName).Count() == 0).ToList();
        }
        private readonly object iatLock = new object();
        public CIAT IAT
        {
            get
            {
                if (_IAT != null)
                    return _IAT;
                lock (iatLock)
                {
                    if (MetaData.IATRelId != String.Empty)
                    {
                        _IAT = new CIAT(GetPackageLevelRelationship(MetaData.IATRelId).TargetUri);
                        foreach (Uri u in GetRelationshipsByType(_IAT.URI, typeof(CIAT), typeof(AlternationGroup)).Select(pr => pr.TargetUri))
                            new AlternationGroup(u);
                    }
                    else
                    {
                        try
                        {
                            IsLoading = true;
                            _IAT = new CIAT(GetPackageLevelRelationship(typeof(CIAT)).TargetUri);
                        }
                        catch (ArgumentException ex)
                        {
                            _IAT = new CIAT();
                            foreach (Uri u in GetRelationshipsByType(_IAT.URI, typeof(CIAT), typeof(AlternationGroup)).Select(pr => pr.TargetUri))
                                new AlternationGroup(u);
                            MetaData.IATRelId = CreatePackageLevelRelationship(_IAT.URI, typeof(CIAT));
                        }
                        finally
                        {
                            IsLoading = false;
                        }
                    }
                    return _IAT;
                }
            }
            set
            {
                lock (iatLock)
                {
                    if (value.URI != null)
                    {
                        try
                        {
                            DeleteRelationships(value.URI);
                        }
                        catch (InvalidOperationException) { }
                        MetaData.IATRelId = CreatePackageLevelRelationship(value.URI, typeof(CIAT));
                    }
                    else
                    {
                        try
                        {
                            PackageRelationship pr = SavePackage.GetRelationshipsByType(typeof(CIAT).ToString()).First();
                            value.URI = pr.TargetUri;
                            MetaData.IATRelId = pr.Id;
                        }
                        catch (InvalidOperationException)
                        {
                            value.URI = CreatePart(value.BaseType, value.GetType(), value.MimeType, ".xml");
                            MetaData.IATRelId = CreatePackageLevelRelationship(value.URI, typeof(CIAT));
                        }
                    }
                    _IAT = value;
                }
            }
        }

        public CIATLayout Layout
        {
            get
            {
                if (_Layout != null)
                    return _Layout;
                lock (layoutLock)
                {
                    if (_Layout != null)
                        return _Layout;
                    PackageRelationship pr = GetPackageLevelRelationship(typeof(CIATLayout));
                    if (pr == null)
                    {
                        _Layout = new CIATLayout();
                        _Layout.Activate();
                        _Layout.Save();
                        CreatePackageLevelRelationship(_Layout.URI, typeof(CIATLayout));
                    }
                    else
                    {
                        _Layout = new CIATLayout(pr.TargetUri);
                        _Layout.Activate();
                    }
                    return _Layout;
                }
            }
            set
            {
                if (_Layout == value)
                    return;
                if (_Layout != null)
                    DeletePackageLevelRelationship(_Layout.BaseType);
                lock (layoutLock)
                {
                    if (_Layout != null)
                    {
                        CIATLayout.ErrorMarkObservableUri.Value = _Layout.ErrorMark.URI;
                        CIATLayout.LeftKeyValueOutlineObservableUri.Value = _Layout.LeftKeyValueOutline.URI;
                        CIATLayout.RightKeyValueOutlineObservableUri.Value = _Layout.RightKeyValueOutline.URI;
                        value.Save();
                        CIAT.SaveFile.DeletePart(_Layout.URI);
                        _Layout = value;
                        CreatePackageLevelRelationship(value.URI, typeof(CIATLayout));
                        Task.Run(() => {
                            Monitor.Enter(layoutLock);
                            Monitor.Exit(layoutLock);
                            ResizeToNewLayout();
                        });
                    }
                    else
                    {
                        _Layout = value;
                        CreatePackageLevelRelationship(value.URI, typeof(CIATLayout));
                        value.Activate();
                        value.Save();
                    }
                    _Layout.Save();
                }
            }
        }

        public FontPreferences FontPreferences
        {
            get
            {
                if (_FontPreferences != null)
                    return _FontPreferences;
                ioLock.EnterUpgradeableReadLock();
                try
                {
                    var fpPr = SavePackage.GetRelationshipsByType(typeof(FontPreferences).ToString()).FirstOrDefault();
                    if (fpPr != null)
                        _FontPreferences = new FontPreferences(fpPr.TargetUri);
                    else
                        _FontPreferences = new FontPreferences();
                }
                finally
                {
                    ioLock.ExitUpgradeableReadLock();
                }
                return _FontPreferences;
            }
            set
            {
                if (_FontPreferences != null)
                {
                    ioLock.ExitUpgradeableReadLock();
                    try
                    {
                        SavePackage.DeleteRelationship(SavePackage.GetRelationshipsByType(typeof(FontPreferences).ToString()).First().Id);
                        _FontPreferences = value;
                        _FontPreferences.Save();
                        SavePackage.CreateRelationship(_FontPreferences.URI, TargetMode.Internal, typeof(FontPreferences).ToString());
                    }
                    finally
                    {
                        ioLock.ExitUpgradeableReadLock();
                    }
                }
            }
        }

        private ImageMetaDataDocument _ImageMetaDataDocument = null;
        private readonly object metaDataDocLock = new object();
        public ImageMetaDataDocument ImageMetaDataDocument
        {
            get
            {
                lock (metaDataDocLock)
                {
                    if (_ImageMetaDataDocument != null)
                        return _ImageMetaDataDocument;
                    var pr = GetPackageLevelRelationship(typeof(ImageMetaDataDocument));
                    if (pr != null)
                    {
                        _ImageMetaDataDocument = new ImageMetaDataDocument(pr.TargetUri);
                        return _ImageMetaDataDocument;
                    }
                    _ImageMetaDataDocument = new ImageMetaDataDocument();
                    return _ImageMetaDataDocument;
                }
            }
        }

        public Uri CreatePart(Type baseType, Type objType, String mimeType, String ext)
        {
            ioLock.EnterWriteLock();
            try
            {
                String objTypeName = objType.ToString();
                if (!MetaData.UriCounters.ContainsKey(objTypeName))
                    MetaData.UriCounters[objTypeName] = new List<int>();
                else if (objType.Equals(typeof(DINull)))
                    return PackUriHelper.CreatePartUri(new Uri(baseType.ToString() + "/" + objType.ToString() + "1" + ext, UriKind.Relative));
                MetaData.UriCounters[objTypeName].Sort();
                List<int> preCtrs = MetaData.UriCounters[objTypeName].TakeWhile((elem, ndx) => elem == ndx + 1).ToList();
                List<int> postCtrs = MetaData.UriCounters[objTypeName].SkipWhile((elem, ndx) => elem == ndx + 1).ToList();
                Uri uri = PackUriHelper.CreatePartUri(new Uri(baseType.ToString() + "/" + objType.ToString() + ((preCtrs.Count == 0) ? 1 : (preCtrs.Max() + 1)).ToString() + ext, UriKind.Relative));
                MetaData.UriCounters[objTypeName].Clear();
                MetaData.UriCounters[objTypeName].AddRange(preCtrs);
                MetaData.UriCounters[objTypeName].Add((preCtrs.Count == 0) ? 1 : (preCtrs.Max() + 1));
                MetaData.UriCounters[objTypeName].AddRange(postCtrs);
                return SavePackage.CreatePart(uri, mimeType).Uri;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public Uri CreatePart(Uri u, String mimeType)
        {
            ioLock.EnterWriteLock();
            try
            {
                return SavePackage.CreatePart(u, mimeType, Compression).Uri;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        private PackagePart GetPart(IPackagePart p)
        {
            if (p.Uri != null)
            {
                ioLock.EnterUpgradeableReadLock();
                try
                {
                    return SavePackage.GetPart(p.Uri);
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    ioLock.ExitUpgradeableReadLock();
                }
            }
            return null;
        }

        public PackagePart GetPart(Uri uri)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(uri);
            }
            catch (InvalidOperationException ex)
            {
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public String GetTypeName(Uri uri)
        {
            PackagePart part = GetPart(uri);
            return (new Regex("\\+(.*)")).Match(part.ContentType).Groups[1].Value;
        }

        public String GetMimeType(Uri uri)
        {
            PackagePart part = GetPart(uri);
            return part.ContentType;
        }

        public bool PartExists(Uri u)
        {
            ioLock.EnterUpgradeableReadLock();
            bool b = SavePackage.PartExists(u);
            ioLock.ExitUpgradeableReadLock();
            return b;
        }

        public PackageRelationship GetRelationship(IPackagePart p, String rId)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(p.Uri).GetRelationship(rId);
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }


        public String GetRelationship(IPackagePart src, IPackagePart target)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                PackagePart p = SavePackage.GetPart(src.Uri);
                return p.GetRelationships().Where(pr => pr.TargetUri.Equals(target.Uri)).Select(pr => pr.Id).First();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public String GetRelationship(Uri srcUri, Uri targetUri)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(srcUri).GetRelationships().Where(pr => pr.TargetUri.Equals(targetUri)).Select(pr => pr.Id).FirstOrDefault();
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public PackageRelationship GetRelationship(Uri srcUri, String rId)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(srcUri).GetRelationship(rId);
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public String GetRelationship(IPackagePart src, Uri targetUri)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                PackagePart p = SavePackage.GetPart(src.Uri);
                return p.GetRelationships().Where(pr => pr.TargetUri.Equals(targetUri)).Select(pr => pr.Id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public PackageRelationship GetPackageLevelRelationship(String rId)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetRelationship(rId);
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public PackageRelationship GetPackageLevelRelationship(Type targetType)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetRelationshipsByType(targetType.ToString()).FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }


        public void DeleteRelationships(Uri srcUri)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart part = SavePackage.GetPart(srcUri);
                List<String> rids = part.GetRelationships().Select(pr => pr.Id).ToList();
                foreach (String rid in rids)
                    part.DeleteRelationship(rid);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public void DeleteRelationshipsByType(Uri srcUri, Type srcType, Type targetType)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart part = SavePackage.GetPart(srcUri);
                List<String> rids = part.GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString())).Select(rel => rel.Id).ToList();
                foreach (String rid in rids)
                    part.DeleteRelationship(rid);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public String CreatePackageLevelRelationship(Uri targetUri, Type targetType)
        {
            ioLock.EnterWriteLock();
            try
            {
                if (SavePackage.GetRelationshipsByType(targetType.ToString()).Count() > 0)
                    throw new InvalidOperationException("A package level relationship of that type already exists");
                return SavePackage.CreateRelationship(targetUri, TargetMode.Internal, targetType.ToString()).Id;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public String CreateRelationship(Type srcType, Type targetType, Uri srcUri, Uri targetUri)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart srcPart = SavePackage.GetPart(srcUri);
                return srcPart.CreateRelationship(targetUri, TargetMode.Internal, String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString())).Id;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public String CreateRelationship(Type srcType, Type targetType, Uri srcUri, Uri targetUri, String additionalData)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart srcPart = SavePackage.GetPart(srcUri);
                return srcPart.CreateRelationship(targetUri, TargetMode.Internal, String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString()) + "+" + additionalData).Id;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public List<PackageRelationship> GetRelationshipsByType(Uri srcUri, Type srcType, Type targetType)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(srcUri).GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString())).ToList();
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public List<PackageRelationship> GetRelationshipsByType(Uri srcUri, Type srcType, Type targetType, String additionalData)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetPart(srcUri).GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString()) + "+" + additionalData).ToList();
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public void DeleteRelationshipsByType(Uri srcUri, Type srcType, Type targetType, String additionalData)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart pp = GetPart(srcUri);
                List<PackageRelationship> prList = pp.GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, srcType.ToString(), targetType.ToString()) + "+" + additionalData).ToList();
                foreach (String rId in prList.Select(pr => pr.Id))
                    pp.DeleteRelationship(rId);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public void DeletePackageLevelRelationship(Type relType)
        {
            ioLock.EnterWriteLock();
            try
            {
                SavePackage.DeleteRelationship(SavePackage.GetRelationshipsByType(relType.ToString()).First().Id);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public String DeleteRelationship(Uri srcUri, Uri targetUri)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart part = SavePackage.GetPart(srcUri);
                String rId = part.GetRelationships().Where(pr => pr.TargetUri.Equals(targetUri)).First().Id;
                part.DeleteRelationship(rId);
                return rId;
            }
            catch (Exception)
            {
                return String.Empty;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public String DeleteRelationship(Uri URI, String rid)
        {
            ioLock.EnterWriteLock();
            try
            {
                PackagePart p = GetPart(URI);
                GetPart(URI).DeleteRelationship(rid);
                return rid;
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        private ManualResetEventSlim ReadStreamFree = new ManualResetEventSlim(true), WriteStreamFree = new ManualResetEventSlim(true);

        public Stream GetReadStream(IPackagePart p)
        {
            PackagePart part = GetPart(p);
            ioLock.EnterUpgradeableReadLock();
            return Stream.Synchronized(part.GetStream(FileMode.Open, FileAccess.Read));
        }

        public Stream GetWriteStream(IPackagePart p)
        {
            ioLock.EnterWriteLock();
            PackagePart part = GetPart(p);
            Stream s = Stream.Synchronized(part.GetStream(FileMode.Create, FileAccess.Write));
            return s;
        }

        public void ReleaseReadStreamLock()
        {
            ioLock.ExitUpgradeableReadLock();
        }

        public void ReleaseWriteStreamLock()
        {
            ioLock.ExitWriteLock();
        }

        public Uri Register(DIBase di)
        {
            Uri u = di.Uri;
            if (u == null)
                u = CreatePart(di.BaseType, di.GetType(), di.MimeType, ".xml");
            if (!DIs.TryAdd(u, di))
                return u;
            return u;
        }

        public void Replace(DIBase newDI, DIBase oldDI)
        {
            ioLock.EnterWriteLock();
            try
            {
                List<PackageRelationship> oldRels = GetPart(newDI).GetRelationships().ToList();
                foreach (PackageRelationship rel in oldRels)
                {
                    GetPart(newDI).DeleteRelationship(rel.Id);
                }
                List<PackageRelationship> deletedRels = GetPart(oldDI).GetRelationships().ToList();
                foreach (PackageRelationship rel in deletedRels)
                {
                    GetPart(oldDI).DeleteRelationship(rel.Id);
                }
                if (!DIs.TryRemove(oldDI.Uri, out DIBase dummy))
                    throw new ArgumentException("Attempt to replace a display item not in the dictionary.");
                DeletePart(oldDI.Uri);
                newDI.Uri = oldDI.Uri;
                CreatePart(typeof(DIBase), newDI.GetType(), newDI.MimeType, ".xml");
                DIs.TryAdd(newDI.Uri, newDI);
            }
            finally
            {
                ioLock.ExitWriteLock();
            }
        }

        public Uri Register(CIATKey key)
        {
            Uri u = key.URI;
            if (u == null)
                u = CreatePart(key.BaseType, key.GetType(), key.MimeType, ".xml");
            if (!Keys.TryAdd(u, key))
                return u;
            return u;
        }

        public Uri Register(CIATItem i)
        {
            Uri u = i.URI;
            if (u == null)
                u = CreatePart(i.BaseType, i.GetType(), i.MimeType, ".xml");
            if (!IATItems.TryAdd(u, i))
                return u;
            return u;
        }

        public Uri Register(CIATBlock b)
        {
            Uri u = b.URI;
            if (u == null)
                u = CreatePart(b.BaseType, b.GetType(), b.MimeType, ".xml");
            if (!IATBlocks.TryAdd(u, b))
                return u;
            return u;
        }

        public Uri Register(CInstructionScreen b)
        {
            Uri u = b.URI;
            if (u == null)
                u = CreatePart(b.BaseType, b.GetType(), b.MimeType, ".xml");
            if (!InstructionScreens.TryAdd(u, b))
                return u;
            return u;
        }

        public Uri Register(CInstructionBlock b)
        {
            Uri u = b.URI;
            if (u == null)
                u = CreatePart(b.BaseType, b.GetType(), b.MimeType, ".xml");
            if (!InstructionBlocks.TryAdd(u, b))
                return u;
            return u;
        }

        public Uri Register(CSurvey s)
        {
            Uri u = s.URI;
            if (u == null)
                u = CreatePart(s.BaseType, s.GetType(), s.MimeType, ".xml");
            if (!Surveys.TryAdd(u, s))
                return u;
            return u;
        }

        public Uri Register(AlternationGroup g)
        {
            Uri u = g.URI;
            if (u == null)
                u = CreatePart(g.BaseType, g.GetType(), g.MimeType, ".xml");
            if (!AlternationGroups.TryAdd(u, g))
                return u;
            return u;
        }

        public Uri Register(Images.IImage ii)
        {
            Uri u = ii.URI;
            if (u == null)
                throw new NullReferenceException();
            if (!IImages.TryAdd(u, ii))
                return u;
            return u;
        }

        public Uri Register(ObservableUri ou)
        {
            Uri u = ou.URI;
            if (u == null)
                u = CreatePart(ou.BaseType, ou.GetType(), ou.MimeType, ".xml");
            if (!ObservableUris.TryAdd(u, ou))
                return u;
            return u;
        }

        public Uri Register(CFontFile.FontItem fi)
        {
            Uri u = fi.URI;
            if (u == null)
                u = CreatePart(typeof(CFontFile.FontItem), fi.GetType(), fi.MimeType, ".xml");
            if (!FontItems.TryAdd(u, fi))
                return u;
            return u;
        }

        public Uri Register(CSurveyItem si)
        {
            Uri u = si.URI;
            if (u == null)
                u = CreatePart(typeof(CSurveyItem), si.GetType(), si.MimeType, ".xml");
            SurveyItems.TryAdd(u, si);
            return u;
        }

        public void DeletePart(Uri uri)
        {
            ioLock.EnterWriteLock();
            try
            {
                Regex exp = new Regex(@"IATClient\..+/(IATClient\..*?)([1-9][0-9]*)");
                String objTypeName = exp.Match(uri.ToString()).Groups[1].Value;
                int ndx = Convert.ToInt32(exp.Match(uri.ToString()).Groups[2].Value);
                MetaData.UriCounters[objTypeName].Remove(ndx);
                Keys.TryRemove(uri, out CIATKey key);
                DIs.TryRemove(uri, out DIBase di);
                IATItems.TryRemove(uri, out CIATItem i);
                IATBlocks.TryRemove(uri, out CIATBlock b);
                InstructionScreens.TryRemove(uri, out CInstructionScreen iScrn);
                InstructionBlocks.TryRemove(uri, out CInstructionBlock iBlock);
                IImages.TryRemove(uri, out Images.IImage ii);
                Surveys.TryRemove(uri, out CSurvey s);
                ObservableUris.TryRemove(uri, out ObservableUri oUri);
                FontItems.TryRemove(uri, out CFontFile.FontItem fi);
                SurveyItems.TryRemove(uri, out CSurveyItem si);
                DeleteRelationships(uri);
            }
            finally
            {
                SavePackage.DeletePart(uri);
                ioLock.ExitWriteLock();
            }
        }

        public List<Uri> GetPartsOfType(String contentType)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                return SavePackage.GetParts().Where(p => p.ContentType == contentType).Select(p => p.Uri).ToList();
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public List<Uri> GetAllIATKeyUris()
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                PackagePart pp = GetPart(IAT);
                return pp.GetRelationshipsByType(String.Format(Properties.Resources.sRelationshipTemplate, typeof(CIAT).ToString(), typeof(CIATKey).ToString())).Select(pr => pr.TargetUri).ToList();
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public List<DIPreview> GetPreviews()
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                var previews = new List<DIPreview>();
                foreach (var part in SavePackage.GetParts().Where(p => p.ContentType == "text/xml+display-item+" + DIType.Preview.ToString()))
                    previews.Add(GetDI(part.Uri) as DIPreview);
                return previews;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public void ResizeToNewLayout()
        {
            ResizingLayoutEvent.Reset();
            List<DIBase> dis = new List<DIBase>();
            List<DIPreview> composites = new List<DIPreview>();
            ioLock.EnterUpgradeableReadLock();
            try
            {
                Regex exp = new Regex(@"^text/xml\+display\-item\+(.*)");
                List<PackagePart> parts = SavePackage.GetParts().Where(p => exp.IsMatch(p.ContentType)).ToList();
                foreach (var p in parts)
                {
                    if (exp.Match(p.ContentType).Groups[1].Value != DIType.SurveyImage.ToString())
                    {
                        DIType diType = DIType.FromString(exp.Match(p.ContentType).Groups[1].Value);
                        if (diType == DIType.Preview)
                            composites.Add(GetDI(p.Uri) as DIPreview);
                        else if (diType != DIType.Null)
                            dis.Add(GetDI(p.Uri));
                    }
                }
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
            //            var genDiUris = dis.AsQueryable().Where(di => di != null).Select(di => di.URI).Distinct<Uri>().Select(u => CIAT.SaveFile.GetDI(u)).Cast<DIBase>();
            ImageManager.InvalidateImageBags(false);
            dis = dis.Distinct().Where(d => d != null).ToList();
            foreach (var c in composites)
                c.SuspendLayout();
            /*
            foreach (var di in dis.Where(d => d.Type == DIType.DualKey))
                (di as DIDualKey).SuspendLayout();
            List<WaitHandle> responseKeyWaiters = new List<WaitHandle>();
            foreach (var di in dis.Where(d => d.Type == DIType.DualKey).Cast<DIDualKey>())
            {
                foreach (var u in di.GetComponentUris())
                {
                    responseKeyWaiters.Add(CIAT.SaveFile.GetDI(u).InvalidationWaitHandle);
                    (CIAT.SaveFile.GetDI(u) as IResponseKeyDI).SuspendLayout();
                }
                di.InvalidationSuspended = true;
            }
            */
            var keys = GetAllIATKeyUris().Select(u => GetIATKey(u)).ToList();
            if (keys.Where(k => k.KeyType == IATKeyType.DualKey).Count() > 0)
            {
                var simpleKeyValueUris = new List<Uri>();
                var dualKeys = keys.Where(k => k.KeyType == IATKeyType.DualKey).Cast<CIATDualKey>().ToList();
                foreach (var dk in dualKeys)
                    dk.SuspendLayout();
                foreach (var k in keys.Where(k => k.KeyType != IATKeyType.SimpleKey).ToList())
                {
                    simpleKeyValueUris.Add(k.LeftValueUri);
                    simpleKeyValueUris.Add(k.RightValueUri);
                }
                WaitHandle[] waits = simpleKeyValueUris.Select(u => CIAT.SaveFile.GetDI(u).InvalidationEvent.WaitHandle).ToArray();
                foreach (var di in simpleKeyValueUris.Distinct().Select(u => CIAT.SaveFile.GetDI(u)))
                    di.ScheduleInvalidationSync();
                WaitHandle.WaitAll(waits);
                foreach (var dk in dualKeys)
                {
                    dk.ResumeLayout(false);
                    dk.GenerateKeyValues();
                    dk.SuspendLayout();
                }
                var dualKeyUris = keys.Where(k => k.KeyType != IATKeyType.DualKey).Select(k => k.LeftValueUri).ToList();
                dualKeyUris.AddRange(keys.Where(k => k.KeyType != IATKeyType.DualKey).Select(k => k.LeftValueUri).ToList());
                foreach (var di in dis.Where(di => !simpleKeyValueUris.Contains(di.Uri) && !dualKeyUris.Contains(di.Uri)).ToList())
                {
                    if (di.IsGenerated)
                        di.ResumeLayout(false);
                    di.ScheduleInvalidation();
                }
            }
            else
            {
                foreach (var di in dis)
                {
                    if (di.IsGenerated)
                        (di as DIGenerated).ResumeLayout(false);
                    di.ScheduleInvalidation();
                }
            }
            /*
            WaitHandle.WaitAll(responseKeyWaiters.Distinct().ToArray());
            foreach (var di in dis.Where(d => d.Type == DIType.DualKey).Cast<DIDualKey>())
                di.InvalidationSuspended = false;
            ResizingLayoutEvent.Set();*/
        }


        public DIBase GetDI(Uri URI)
        {
            ioLock.EnterUpgradeableReadLock();
            try
            {
                String mimeType = String.Empty;
                if (DIs.TryGetValue(URI, out DIBase val))
                    return val;
                if (!SavePackage.PartExists(URI))
                    return null;
                PackagePart part = SavePackage.GetPart(URI);
                mimeType = part.ContentType;
                Regex exp = new Regex(@"display\-item\+(.*)$");
                String dispItemType = exp.Match(mimeType).Groups[1].Value;
                DIType type;
                if (dispItemType == "Composite")
                    type = DIType.Preview;
                else
                    type = DIType.FromString(dispItemType);
                if (type == DIType.LambdaGenerated)
                    return null;
                DIBase di = type.Create(URI);
                DIs.TryAdd(URI, di);
                return di;
                return null;
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }


        public CIATKey GetIATKey(Uri uri)
        {
            bool bWasWriting = ioLock.IsWriteLockHeld;
            bool bWasReading = ioLock.IsReadLockHeld;
            ioLock.EnterUpgradeableReadLock();
            try
            {
                if (Keys.ContainsKey(uri))
                    return Keys[uri];
                if (!PartExists(uri))
                    throw new KeyNotFoundException("The supplied uri does not exist in the package file");
                PackagePart part = SavePackage.GetPart(uri);
                String mimeType = part.ContentType;
                Regex exp = new Regex("\\+(.*)$");
                Keys[uri] = IATKeyType.FromTypeName(exp.Match(mimeType).Groups[1].Value).Create(uri);
                return Keys[uri];
            }
            finally
            {
                ioLock.ExitUpgradeableReadLock();
            }
        }

        public CIATItem GetIATItem(Uri uri)
        {
            if (IATItems.ContainsKey(uri))
                return IATItems[uri];
            if (!PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file");
            IATItems[uri] = new CIATItem(uri);
            return IATItems[uri];
        }

        public CIATBlock GetIATBlock(Uri uri)
        {
            if (IATBlocks.ContainsKey(uri))
                return IATBlocks[uri];
            if (!PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file");
            IATBlocks[uri] = new CIATBlock(IAT, uri);
            return IATBlocks[uri];
        }

        public CInstructionScreen GetInstructionScreen(Uri uri)
        {
            if (InstructionScreens.ContainsKey(uri))
                return InstructionScreens[uri];
            if (!PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file");
            PackagePart part = GetPart(uri);
            String mimeType = part.ContentType;
            Regex exp = new Regex("\\+(.*)");
            InstructionScreens[uri] = InstructionScreenType.FromTypeName(exp.Match(mimeType).Groups[1].Value).Create(uri);
            return InstructionScreens[uri];
        }

        public CInstructionBlock GetInstructionBlock(Uri uri)
        {
            if (InstructionBlocks.ContainsKey(uri))
                return InstructionBlocks[uri];
            if (!PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file");
            InstructionBlocks[uri] = new CInstructionBlock(IAT, uri);
            return InstructionBlocks[uri];
        }

        public CSurvey GetSurvey(Uri uri)
        {
            if (Surveys.ContainsKey(uri))
                return Surveys[uri];
            if (!PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            Surveys[uri] = new CSurvey(uri);
            return Surveys[uri];
        }

        public AlternationGroup GetAlternationGroup(Uri uri)
        {
            if (AlternationGroups.ContainsKey(uri))
                return AlternationGroups[uri];
            if (PartExists(uri))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            AlternationGroups[uri] = new AlternationGroup(uri);
            return AlternationGroups[uri];
        }

        public ObservableUri GetObservableUri(Uri u)
        {
            if (ObservableUris.ContainsKey(u))
                return ObservableUris[u];
            if (!PartExists(u))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            ObservableUris[u] = new ObservableUri(u);
            return ObservableUris[u];
        }

        public CFontFile.FontItem GetFontItem(Uri u)
        {
            if (FontItems.ContainsKey(u))
                return FontItems[u];
            if (!PartExists(u))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            FontItems[u] = new CFontFile.FontItem(u);
            return FontItems[u];
        }

        public CSurveyItem GetSurveyItem(Uri u)
        {
            if (SurveyItems.ContainsKey(u))
                return SurveyItems[u];
            if (!PartExists(u))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            PackagePart pp = SavePackage.GetPart(u);
            Regex exp = new Regex(@"text/xml\+(.+)");
            SurveyItemType t = SurveyItemType.FromTypeName(exp.Match(pp.ContentType).Groups[1].Value);
            if (t == SurveyItemType.Caption)
                SurveyItems[u] = new CSurveyCaption(u);
            else if (t == SurveyItemType.SurveyImage)
                SurveyItems[u] = new CSurveyItemImage(u);
            else
                SurveyItems[u] = new CSurveyItem(u);
            return SurveyItems[u];
        }


        public Images.IImage GetIImage(Uri u)
        {
            if (IImages.ContainsKey(u))
                return IImages[u];
            if (!PartExists(u))
                throw new KeyNotFoundException("The supplied uri does not exist in the package file.");
            IImages[u] = new Images.ImageManager.Image(u);
            return IImages[u];
        }

        private readonly ManualResetEventSlim SaveEvent = new ManualResetEventSlim(true);
        public void Save(String filename)
        {
            Package package = null;
            ThreadStart thStart = new ThreadStart(new Action(() =>
            {
                SaveSplash spl = new SaveSplash();
                spl.Show();
                ImageMetaDataDocument.CleanPackageForSave();
                Flush(false);
                ioLock.EnterWriteLock();
                try
                {
                    SavePackage.Close();
                    //                    if ((WorkingSaveFilename != filename) && (File.Exists(filename)))
                    File.Delete(filename);
                    using (var stream = File.Open(filename, FileMode.OpenOrCreate))
                        PackageStream.WriteTo(stream);
                    SavePackage = Package.Open(PackageStream, FileMode.Open, FileAccess.ReadWrite);

                    /*                    File.Copy(WorkingSaveFilename, filename);
                                        SavePackage = Package.Open(PackageStream, FileMode.Open, FileAccess.ReadWrite);
                                        package = Package.Open(filename, FileMode.Open, FileAccess.ReadWrite);
                                        byte[] data = new byte[32];
                                        Stream s = pPart.GetStream(FileMode.Create, FileAccess.ReadWrite);
                                        s.Write(data, 0, 32);
                                        package.DeletePart(pPart.Uri);
                                        package.Close();

                                        RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
                                        rsaCrypt.ImportCspBlob(Convert.FromBase64String(Properties.Resources.sig));
                                        SHA256 sha = SHA256.Create();
                                        byte[] hash = null;
                                        using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                                            hash = sha.ComputeHash(fs);
                                        byte[] signedHash = rsaCrypt.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
                                        using (FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write))
                                            fs.Write(signedHash, 0, signedHash.Length);
                      */
                }
                finally
                {
                    ioLock.ExitWriteLock();
                    spl.Close();
                    SaveEvent.Set();
                }
            }));
            SaveEvent.Reset();
            SaveThread = new Thread(thStart);
            SaveThread.SetApartmentState(ApartmentState.STA);
            SaveThread.Start();
        }

        public bool Flush(bool optional)
        {
            DisposingEvent.WaitOne();
            ResizingLayoutEvent.WaitOne(1000);
            if (IsDisposed)
                return false;
            CIAT iat = IAT;
            FrozenForSave = true;
            int ctr = 0;
            List<Uri> uris = Keys.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetIATKey(u)?.Save();
            }
            uris = IATItems.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetIATItem(u)?.Save();
            }
            uris = InstructionScreens.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetInstructionScreen(u)?.Save();
            }
            uris = IATBlocks.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetIATBlock(u)?.Save();
            }
            uris = InstructionBlocks.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetInstructionBlock(u)?.Save();
            }
            uris = Surveys.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetSurvey(u)?.Save();
            }
            uris = AlternationGroups.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetAlternationGroup(u)?.Save();
            }
            uris = DIs.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetDI(u)?.Save();
            }
            uris = ObservableUris.Keys.ToList();
            foreach (Uri u in uris)
            {
                GetObservableUri(u).Save();
            }
            FontPreferences.Save();
            ImageMetaDataDocument.Save();
            Layout.Save();
            iat.Save();
            MetaData.Save();
            //            ImageManager.FlushCache();
            FrozenForSave = false;
            return true;
        }

        static public String RecoveryFilePath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create) + Path.DirectorySeparatorChar +
                    "IATSoftware" + Path.DirectorySeparatorChar + "Recovery.iat";
            }
        }

        public void Dispose()
        {
            if (IsDisposing || IsDisposed)
                return;
            IsDisposing = true;
            try
            {
                //                ActivityLog.LogEvent(ActivityLog.EventType.Delete, IAT.URI);
                ImageManager.Dispose();
                /*              lock (saveLock)
                              {
                                  foreach (var k in Keys.Values.Where(key => key is CIATDualKey))
                                      k.Dispose();
                                  foreach (var k in Keys.Values.Where(key => key is CIATReversedKey))
                                      k.Dispose();
                                  foreach (var k in Keys.Values.Where(key => key is CIATKey))
                                      k.Dispose();
                                  foreach (var b in IATBlocks.Values)
                                      b.Dispose();
                                  foreach (var item in IATItems.Values)
                                      item.Dispose();
                                  foreach (var ib in InstructionBlocks.Values)
                                      ib.Dispose();
                                  foreach (var s in Surveys.Values)
                                      s.Dispose();
                                  foreach (var i in IImages.Values)
                                      i.Dispose();
                                  foreach (var ou in ObservableUris.Values)
                                      ou.Dispose();
                              }
                              DisposingEvent.Reset();*/
            }
            finally
            {
                IsDisposed = true;
                SavePackage.Close();
                try
                {
                    PackageStream.Dispose();
                }
                catch (Exception ex)
                {
                }
            }
            DisposingEvent.Set();
        }
    }
}

{
        private Package? _package;
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<Uri, IATBlock> _blockCache = new();
        private readonly Dictionary<Uri, IPackagePart> _partCache = new();

        public async Task<CIAT> LoadPackageAsync(string packagePath)
        {
            
            _lock.EnterWriteLock();
            try
            {
                _package?.Close();
                _package = Package.Open(packagePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var ciat = new CIAT(); // your root model
                // load any root relationships here if needed
                return ciat;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IATBlock GetBlock(Uri uri)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_blockCache.TryGetValue(uri, out var block))
                    return block;

                _lock.EnterWriteLock();
                try
                {
                    if (!_package.PartExists(uri))
                        throw new KeyNotFoundException($"Part not found: {uri}");

                    block = new IATBlock(uri); // lightweight constructor – NO side effects
                    _blockCache[uri] = block;
                    return block;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public Stream GetReadStream(IPackagePart part)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                var packagePart = _package.GetPart(part.Uri);
                return Stream.Synchronized(packagePart.GetStream(FileMode.Open, FileAccess.Read));
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void SaveChanges()
        {
            _lock.EnterWriteLock();
            try
            {
                _package?.Flush();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            _package?.Close();
            _lock.Dispose();
        }
    }
}