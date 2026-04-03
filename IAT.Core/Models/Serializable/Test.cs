using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Models.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace IAT.Core.Models.Serializable
{
    public class Test : IDisposable, IPackagePart
    {
        /// <summary>
        /// Gets or sets the Uniform Resource Identifier (URI) associated with this instance.
        /// </summary>
        [XmlElement("Uri", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public Uri? Uri{ get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Represents the part type for the package, set to IAT.
        /// </summary>
        [XmlIgnore]
        public readonly PartType PackagePartType = PartType.IAT;

        /// <summary>
        /// Gets or sets the type of token the test taker is identified byrepresented by this instance.
        /// </summary>
        [XmlElement("TokenType", Form = XmlSchemaForm.Unqualified)]
        public TokenType TokenType { get; set; }

        /// <summary>
        /// Gets or sets the name of the token used for authentication or identification purposes.
        /// </summary>
        [XmlElement("TokenName", Form = XmlSchemaForm.Unqualified)]
        public String TokenName { get; set; }


        /// <summary>
        /// Provides access to the collection of alternation groups, indexed by their unique integer identifiers.
        /// </summary>
        /// <remarks>This dictionary is read-only and should not be replaced. Modifications to the
        /// collection itself, such as adding or removing groups, can be performed through the dictionary's methods. The
        /// property is ignored during XML serialization due to the XmlIgnore attribute.</remarks>
        [XmlIgnore]
        public readonly Dictionary<int, AlternationGroup> AlternationGroups = new Dictionary<int, AlternationGroup>();

        /// <summary>
        /// Gets or sets the collection of alternation group identifiers associated with this instance.
        /// </summary>
        /// <remarks>Modifying this property updates the set of alternation groups maintained by the
        /// instance. Adding an identifier creates a new alternation group if it does not already exist. Removing an
        /// identifier deletes the corresponding alternation group and its associated resources.</remarks>
        [XmlArray("AlternationGroupIds", Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("AlternationGroupId", Form = XmlSchemaForm.Unqualified)]
        public List<Guid> AlternationGroupIds { get; } = [];
/*        {
            get
            {
                return AlternationGroups.Keys.ToList();
            }
            set
            {
                if (value != null)
                {
                    foreach (int id in value)
                    {
                        if (!AlternationGroups.ContainsKey(id))
                            AlternationGroups.Add(id, new AlternationGroup() { Uri = CIAT.SaveFile.CreatePart(BaseType, typeof(AlternationGroup), "text/xml+" + typeof(AlternationGroup).ToString(), ".xml") });
                    }
                    List<int> keysToRemove = new List<int>();
                    foreach (int key in AlternationGroups.Keys)
                        if (!value.Contains(key))
                            keysToRemove.Add(key);
                    foreach (int key in keysToRemove)
                    {
                        CIAT.SaveFile.DeletePart(AlternationGroups[key].Uri);
                        AlternationGroups.Remove(key);
                    }
                }
            }
        }
*/
        private CUniqueResponse _UniqueResponse = null;
        public ContentsList Contents { get; private set; } = new ContentsList();
        public String Name { get; set; }
        public String LeftResponseChar { get; set; }
        public String RightResponseChar { get; set; }
        public String RedirectionURL { get; set; }
        private Uri FixationCrossURI { get; set; } = null;

        public enum ESelfAlternationType { none, prepended, postpended, rotated };
        public ESelfAlternationType AlternationType { get; set; }
        static public EventDispatcher.ApplicationEventDispatcher Dispatcher = new EventDispatcher.ApplicationEventDispatcher();
        private List<Uri> _InstructionBlocks { get; set; } = new List<Uri>();
        private List<Uri> _Blocks { get; set; } = new List<Uri>();
        private List<Uri> _BeforeSurvey { get; set; } = new List<Uri>();
        private List<Uri> _AfterSurvey { get; set; } = new List<Uri>();

        public bool Is7Block { get { return _Blocks.Count == 7; } }
        static public Images.ImageManager ImageManager
        {
            get
            {
                return SaveFile.ImageManager;
            }
        }

        public IList<Block> Blocks
        {
            get
            {
                return _Blocks.Select(u => CIAT.SaveFile.GetIATBlock(u)).ToList().AsReadOnly();
            }
        }
        public IList<CInstructionBlock> InstructionBlocks
        {
            get
            {
                return _InstructionBlocks.Select(u => CIAT.SaveFile.GetInstructionBlock(u)).ToList().AsReadOnly();
            }
        }
        public IList<CSurvey> BeforeSurvey
        {
            get
            {
                return _BeforeSurvey.Select(u => CIAT.SaveFile.GetSurvey(u)).ToList().AsReadOnly();
            }
        }
        public IList<CSurvey> AfterSurvey
        {
            get
            {
                return _AfterSurvey.Select(u => CIAT.SaveFile.GetSurvey(u)).ToList().AsReadOnly();
            }
        }
        private readonly object UniqueResponseLock = new object();
        public CUniqueResponse UniqueResponse
        {
            get
            {
                lock (UniqueResponseLock)
                {
                    if (_UniqueResponse == null)
                    {
                        var pr = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CUniqueResponse)).FirstOrDefault();
                        if (pr?.TargetUri != null)
                            _UniqueResponse = new CUniqueResponse(pr.TargetUri);
                        else
                        {
                            _UniqueResponse = new CUniqueResponse();
                            CIAT.SaveFile.CreateRelationship(BaseType, typeof(CUniqueResponse), this.URI, _UniqueResponse.URI);
                        }
                        return _UniqueResponse;
                    }
                    else if (_UniqueResponse.IsDisposed)
                    {
                        _UniqueResponse = new CUniqueResponse();
                        CIAT.SaveFile.CreateRelationship(BaseType, typeof(CUniqueResponse), URI, _UniqueResponse.URI);
                    }
                    return _UniqueResponse;
                }
            }
            set
            {
                lock (UniqueResponseLock)
                {
                    if (_UniqueResponse != null)
                        _UniqueResponse.Dispose();
                    _UniqueResponse = value;
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(CUniqueResponse), URI, _UniqueResponse.URI);
                }
            }
        }


        public void MoveContentsItem(IContentsItem ci, int diff)
        {
            if (diff == 0)
                return;
            if (!Contents.Contains(ci))
                throw new InvalidOperationException();
            int ndx = Contents.IndexOf(ci);
            if (ndx + diff < 0)
                return;
            if (ndx + diff >= Contents.Count)
                return;
            if ((ci.Type == ContentsItemType.BeforeSurvey) && (ndx + diff >= BeforeSurvey.Count))
            {
                (ci as CSurvey).Ordinality = CSurvey.EOrdinality.After;
                int newNdx = (_AfterSurvey.Count == 0) ? (Contents.Count) : (ndx + diff + (AfterSurvey[0].IndexInContents - 1));
                _AfterSurvey.Insert((ndx + diff) - BeforeSurvey.Count, ci.URI);
                _BeforeSurvey.Remove(ci.URI);
                Contents.Remove(ci);
                Contents.Insert(newNdx - 1, ci);
            }
            else if ((ci.Type == ContentsItemType.AfterSurvey) && (ndx + diff - AfterSurvey[0].IndexInContents < 0))
            {
                (ci as CSurvey).Ordinality = CSurvey.EOrdinality.Before;
                int newNdx = (_BeforeSurvey.Count == 0) ? 0 : (ndx + diff - AfterSurvey[0].IndexInContents + BeforeSurvey.Count + 1);
                _BeforeSurvey.Insert((ndx + diff) - AfterSurvey[0].IndexInContents + BeforeSurvey.Count + 1, ci.URI);
                _AfterSurvey.Remove(ci.URI);
                Contents.Remove(ci);
                Contents.Insert(newNdx, ci);
            }
            else if (((ci.Type != ContentsItemType.IATBlock) && (ci.Type != ContentsItemType.InstructionBlock)) ||
                ((diff + ndx <= Contents.Where(c => (c.Type == ContentsItemType.InstructionBlock) ||
                (c.Type == ContentsItemType.IATBlock)).Select(c => c.IndexInContents).Max())
                && (diff + ndx >= _BeforeSurvey.Count)))
            {
                Contents.Remove(ci);
                if (ndx + diff < 0)
                    return;
                else if (ndx - 1 + diff > Contents.Count)
                    Contents.Add(ci);
                else if (diff < 0)
                    Contents.Insert(ndx + diff, ci);
                else
                    Contents.Insert(ndx + diff, ci);
                int ndxInContainer = Contents.Where(c => !c.URI.Equals(ci.URI)).Where(c => c.Type == ci.Type).Where(c => c.IndexInContents <= ndx).Count();
                if (ci.Type == ContentsItemType.IATBlock)
                {
                    _Blocks.Remove(ci.URI);
                    _Blocks.Insert(ndxInContainer, ci.URI);
                }
                if (ci.Type == ContentsItemType.InstructionBlock)
                {
                    _InstructionBlocks.Remove(ci.URI);
                    _InstructionBlocks.Insert(ndxInContainer, ci.URI);
                }
                if (ci.Type == ContentsItemType.BeforeSurvey)
                {
                    _BeforeSurvey.Remove(ci.URI);
                    _BeforeSurvey.Insert(ndxInContainer, ci.URI);
                }
                if (ci.Type == ContentsItemType.AfterSurvey)
                {
                    _AfterSurvey.Remove(ci.URI);
                    _AfterSurvey.Insert(ndxInContainer, ci.URI);
                }
            }
        }
        public void ReplaceSurvey(CSurvey newSurvey, CSurvey oldSurvey)
        {
            if (_BeforeSurvey.Contains(oldSurvey.URI))
            {
                int ndx = _BeforeSurvey.IndexOf(oldSurvey.URI);
                _BeforeSurvey.RemoveAt(ndx);
                _BeforeSurvey.Insert(ndx, newSurvey.URI);
            }
            if (_AfterSurvey.Contains(oldSurvey.URI))
            {
                int ndx = _AfterSurvey.IndexOf(oldSurvey.URI);
                _AfterSurvey.RemoveAt(ndx);
                _AfterSurvey.Insert(ndx, newSurvey.URI);
            }
        }

        public void DeleteContentsItem(IContentsItem ci)
        {
            if (ci.Type == ContentsItemType.BeforeSurvey)
                _BeforeSurvey.Remove(ci.URI);
            else if (ci.Type == ContentsItemType.AfterSurvey)
                _AfterSurvey.Remove(ci.URI);
            else if (ci.Type == ContentsItemType.IATBlock)
                _Blocks.Remove(ci.URI);
            else if (ci.Type == ContentsItemType.InstructionBlock)
                _InstructionBlocks.Remove(ci.URI);
            Contents.Remove(ci);
            CIAT.SaveFile.DeleteRelationship(URI, ci.URI);
            ci.Dispose();
        }
        public void AddIATBlock(Block b)
        {
            _Blocks.Add(b.URI);
            Contents.Add(b);
            CIAT.SaveFile.CreateRelationship(this.BaseType, b.BaseType, this.URI, b.URI);
        }
        public void InsertIATBlock(Block b, int contentsNdx)
        {
            int blockNdx = 0;
            for (int ctr = 0; ctr < contentsNdx; ctr++)
                if (Contents[ctr].Type == ContentsItemType.IATBlock)
                    blockNdx++;
            _Blocks.Insert(blockNdx, b.URI);
            Contents.Insert(contentsNdx, b);
            CIAT.SaveFile.CreateRelationship(this.BaseType, b.BaseType, this.URI, b.URI);
        }
        public void DeleteIATBlock(Block b)
        {
            _Blocks.Remove(b.URI);
            Contents.Remove(b);
            CIAT.SaveFile.DeleteRelationship(this.URI, b.URI);
            b.Dispose();
        }
        public void AddInstructionBlock(CInstructionBlock b)
        {
            _InstructionBlocks.Add(b.URI);
            Contents.Add(b);
            CIAT.SaveFile.CreateRelationship(BaseType, b.BaseType, URI, b.URI);
        }
        public void InsertInstructionBlock(CInstructionBlock b, int contentsNdx)
        {
            int blockNum = 0;
            for (int ctr = 0; ctr < contentsNdx; ctr++)
                if (Contents[ctr].Type == ContentsItemType.InstructionBlock)
                    blockNum++;
            _InstructionBlocks.Insert(blockNum, b.URI);
            Contents.Insert(contentsNdx, b);
        }
        public void DeleteInstructionBlock(CInstructionBlock b)
        {
            Contents.Remove(b);
            _InstructionBlocks.Remove(b.URI);
            CIAT.SaveFile.DeleteRelationship(URI, b.URI);
            b.Dispose();
        }
        public void AddBeforeSurvey(CSurvey s)
        {
            _BeforeSurvey.Add(s.URI);
            CIAT.SaveFile.CreateRelationship(BaseType, s.BaseType, this.URI, s.URI);
        }
        public void InsertBeforeSurvey(CSurvey s, int contentsNdx)
        {
            int blockNum = 0;
            for (int ctr = 0; ctr < contentsNdx; ctr++)
                if (Contents[ctr].Type == ContentsItemType.BeforeSurvey)
                    blockNum++;
            _BeforeSurvey.Insert(blockNum, s.URI);
            Contents.Insert(contentsNdx, s);
        }
        public void DeleteBeforeSurvey(CSurvey survey)
        {
            Contents.Remove(survey);
            _BeforeSurvey.Remove(survey.URI);
            CIAT.SaveFile.DeleteRelationship(this.URI, survey.URI);
            survey.Dispose();
        }
        public void CreateAfterSurvey(CSurvey s)
        {
            _AfterSurvey.Add(s.URI);
            Contents.Add(s);
            CIAT.SaveFile.CreateRelationship(this.BaseType, s.BaseType, this.URI, s.URI);
        }
        public void InsertAfterSurvey(CSurvey s, int contentsNdx)
        {
            int blockNum = 0;
            for (int ctr = 0; ctr < contentsNdx; ctr++)
                if (Contents[ctr].Type == ContentsItemType.AfterSurvey)
                    blockNum++;
            _AfterSurvey.Insert(blockNum, s.URI);
            Contents.Insert(contentsNdx, s);
        }
        public void DeleteAfterSurvey(CSurvey survey)
        {
            Contents.Remove(survey);
            _AfterSurvey.Remove(survey.URI);
            CIAT.SaveFile.DeleteRelationship(this.URI, survey.URI);
            survey.Dispose();
        }
        private readonly object SurveyIndiciesLock = new object();
        private SurveyIndicies _SurveyIndicies = null;
        public SurveyIndicies SurveyIndicies
        {
            get
            {
                lock (SurveyIndiciesLock)
                {
                    if (_SurveyIndicies == null)
                    {
                        Uri indiciesUri = CIAT.SaveFile.GetRelationshipsByType(URI, typeof(CIAT), typeof(SurveyIndicies)).Select(pr => pr.TargetUri).FirstOrDefault();
                        if (indiciesUri == null)
                        {
                            _SurveyIndicies = new SurveyIndicies();
                            CIAT.SaveFile.CreateRelationship(BaseType, typeof(SurveyIndicies), URI, _SurveyIndicies.URI);
                        }
                        else
                            _SurveyIndicies = new SurveyIndicies(indiciesUri);
                    }
                    return _SurveyIndicies;
                }
            }
        }

        /*
        public void ReplaceBeforeSurvey(int ndx, CSurvey survey)
        {
            CSurvey old = BeforeSurvey[ndx];
            CIAT.SaveFile.DeleteRelationship(this.URI, old.URI);
            old.Dispose();
            _BeforeSurvey[ndx] = survey.URI;
            Contents[ndx] = survey;
            CIAT.SaveFile.CreateRelationship(BaseType, survey.BaseType, URI, survey.URI);
        }
        public void ReplaceAfterSurvey(int ndx, CSurvey survey)
        {
            CSurvey old = AfterSurvey[ndx];
            CIAT.SaveFile.DeleteRelationship(this.URI, old.URI);
            old.Dispose();
            _AfterSurvey[ndx] = survey.URI;
            Contents[ndx] = survey;
            CIAT.SaveFile.CreateRelationship(BaseType, survey.BaseType, URI, survey.URI);
        }
        */

        public int GetNumItems()
        {
            int nItems = 0;
            for (int ctr = 0; ctr < Blocks.Count; ctr++)
                nItems += Blocks[ctr].NumItems;
            for (int ctr = 0; ctr < InstructionBlocks.Count; ctr++)
                nItems += InstructionBlocks[ctr].NumScreens;
            return nItems;
        }
        public int NumNonDualKeyBlocks
        {
            get
            {
                return Blocks.Where(b => (b.Key != null)).Where(b => b.Key.KeyType != IATKeyType.DualKey).Count();
            }
        }

        public Test()
        {
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            TokenType = ETokenType.NONE;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }

        public Test(Uri uri)
        {
            this.URI = uri;
            Load();
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }

        public void Dispose()
        {
            _BeforeSurvey.Clear();
            _AfterSurvey.Clear();
            _Blocks.Clear();
            _InstructionBlocks.Clear();
            //            CDynamicSpecifier.ClearSpecifierDictionary();
            Contents.Clear();
            Name = String.Empty;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("IAT", new XElement("Name", Name)));
            if (FixationCrossURI != null)
            {
                String crossRelId = CIAT.SaveFile.GetRelationship(this, FixationCrossURI);
                xDoc.Root.Add(new XElement("FixationCross", crossRelId));
            }
            if (_UniqueResponse != null)
            {
                String uniqueRId = CIAT.SaveFile.GetRelationship(this, UniqueResponse);
                xDoc.Root.Add(new XElement("UniqueResponseRelId", uniqueRId));
                UniqueResponse.Save();
            }
            XElement xElem = new XElement("Contents");
            foreach (IContentsItem iItem in Contents)
            {
                String rId = CIAT.SaveFile.GetRelationship(this, iItem);
                if (iItem.Type == ContentsItemType.BeforeSurvey)
                    xElem.Add(new XElement(ContentsItemType.BeforeSurvey.ToString(), rId));
                else if (iItem.Type == ContentsItemType.AfterSurvey)
                    xElem.Add(new XElement(ContentsItemType.AfterSurvey.ToString(), rId));
                else if (iItem.Type == ContentsItemType.IATBlock)
                    xElem.Add(new XElement(ContentsItemType.IATBlock.ToString(), rId));
                else if (iItem.Type == ContentsItemType.InstructionBlock)
                    xElem.Add(new XElement(ContentsItemType.InstructionBlock.ToString(), rId));
            }
            xDoc.Root.Add(xElem);
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
            foreach (PackageRelationship pr in CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CFontFile.FontItem)).ToList())
                CIAT.SaveFile.GetFontItem(pr.TargetUri).Dispose();
            List<CFontFile.FontItem> fontItems = UtilizedFonts;
            if (fontItems.Count > 0)
                foreach (var fi in fontItems)
                    fi.Save();
            SurveyIndicies.Save();
        }

        public void Load()
        {
            Stream s = SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Name = xDoc.Root.Element("Name").Value;
            if (xDoc.Root.Element("FixationCross") != null)
                FixationCrossURI = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("FixationCross").Value).TargetUri;
            foreach (XElement xElem in xDoc.Root.Element("Contents").Elements())
            {
                String rId = xElem.Value;
                Uri itemUri = SaveFile.GetRelationship(this, xElem.Value).TargetUri;

                if (ContentsItemType.Parse(xElem.Name.LocalName) == ContentsItemType.BeforeSurvey)
                {
                    _BeforeSurvey.Add(itemUri);
                    Contents.Add(ContentsItemType.BeforeSurvey, itemUri);
                }
                if (ContentsItemType.Parse(xElem.Name.LocalName) == ContentsItemType.AfterSurvey)
                {
                    _AfterSurvey.Add(itemUri);
                    Contents.Add(ContentsItemType.AfterSurvey, itemUri);
                }

                if (ContentsItemType.Parse(xElem.Name.LocalName) == ContentsItemType.IATBlock)
                {
                    _Blocks.Add(itemUri);
                    Contents.Add(ContentsItemType.IATBlock, itemUri);
                }
                if (ContentsItemType.Parse(xElem.Name.LocalName) == ContentsItemType.InstructionBlock)
                {
                    _InstructionBlocks.Add(itemUri);
                    Contents.Add(ContentsItemType.InstructionBlock, itemUri);
                }
            }
        }

        /// <summary>
        /// Calculates the number of distinct items in the IAT
        /// </summary>
        /// <returns>the number of distinct items in the IAT</returns>
        public int CountItems()
        {
            int nItems = 0;
            for (int ctr = 0; ctr < Blocks.Count; ctr++)
                nItems += Blocks[ctr].NumItems;

            return nItems;
        }

        /// <summary>
        /// Returns the 1-based block number of the given item in the IAT
        /// </summary>
        /// <param name="ItemNum">the 1-based index of the item</param>
        /// <returns>the 1-based index of the block that contins the item</returns>
        public int GetBlockNumber(int ItemNum)
        {
            int ctr1 = 0;
            int BlockCtr = 1;
            int ItemCtr = 1;
            for (ctr1 = 0; ctr1 < Contents.Count; ctr1++)
            {
                if (Contents[ctr1].Type == ContentsItemType.IATBlock)
                {
                    if (ItemCtr + ((Block)Contents[ctr1]).NumItems > ItemNum)
                        return BlockCtr;
                    BlockCtr++;
                    ItemCtr += ((Block)Contents[ctr1]).NumItems;
                }
            }
            return -1;
        }

        public void MakeAfterSurvey(CSurvey s)
        {
            if (!BeforeSurvey.Contains(s))
                return;
            _BeforeSurvey.Remove(s.URI);
            s.Ordinality = CSurvey.EOrdinality.After;
            _AfterSurvey.Insert(0, s.URI);

        }

        public void MakeBeforeSurvey(CSurvey s)
        {
            if (!AfterSurvey.Contains(s))
                return;
            _AfterSurvey.Remove(s.URI);
            s.Ordinality = CSurvey.EOrdinality.Before;
            _BeforeSurvey.Add(s.URI);
        }
        /*
                public int GetBlockIndex(Block block)
                {
                    int ctr = 0;
                    int nBlockCtr = 0;
                    while (ctr < Contents.Count)
                    {
                        IContentsItem cItem = Contents[ctr++];
                        if (cItem.Type == ContentsItemType.IATBlock)
                        {
                            nBlockCtr++;
                            if (block == (Block)cItem)
                                return nBlockCtr;
                        }
                    }
                    return -1;
                }

                public int GetInstructionBlockIndex(CInstructionBlock block)
                {
                    int ctr = 0;
                    int nBlockCtr = 0;
                    while (ctr < Contents.Count)
                    {
                        IContentsItem cItem = Contents[ctr++];
                        if (cItem.Type == ContentsItemType.InstructionBlock)
                        {
                            nBlockCtr++;
                            if (block == (CInstructionBlock)cItem)
                                return nBlockCtr;
                        }
                    }
                    return -1;
                }
        */
        private ConfigFile.ERandomizationType _RandomizationType = ConfigFile.ERandomizationType.None;
        public ConfigFile.ERandomizationType RandomizationType
        {
            get
            {
                return ConfigFile.ERandomizationType.SetNumberOfPresentations;
            }
        }

        public ConfigFile.ERandomizationType GetRandomizationType()
        {
            return RandomizationType;
        }

        public List<CFontFile.FontItem> UtilizedFonts
        {
            get
            {
                List<CFontFile.FontItem> fontItems = new List<CFontFile.FontItem>();
                List<DIIatBlockInstructions> blockInstructions = new List<DIIatBlockInstructions>();
                foreach (Block b in Blocks)
                {
                    if (b.IndexInContainer < 2)
                        fontItems.AddRange(b.UtilizedStimuliFonts);
                    blockInstructions.Add(CIAT.SaveFile.GetDI(b.InstructionsUri) as DIIatBlockInstructions);
                }
                var textInstructions = blockInstructions.Select((tdi, ndx) => new { ndx = ndx + 1, tdi });
                var textInstructionFonts = from ff in textInstructions.Select(tdi => tdi.tdi).Select(tdi => tdi.PhraseFontFamily).Distinct()
                                           select new { familyName = ff, indicies = textInstructions.Where(tdi => tdi.tdi.PhraseFontFamily == ff).Select(instructions => instructions.ndx) };
                foreach (var txtInstruction in textInstructionFonts)
                {
                    fontItems.Add(new CFontFile.FontItem(txtInstruction.familyName, "is used by instructions in IAT Blocks #{0}", txtInstruction.indicies,
                        textInstructions.Where(i => txtInstruction.indicies.Contains(i.ndx)).Select(i => i.tdi as DIText)));
                }
                foreach (CInstructionBlock instrBlock in InstructionBlocks)
                    fontItems.AddRange(instrBlock.UtilizedFontFamilies);
                fontItems.AddRange(CIATKey.UtilizedFontFamilies);

                return fontItems;
            }
        }
    }
}