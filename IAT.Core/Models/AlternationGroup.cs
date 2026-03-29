using sun.awt.geom;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace IAT.Core.Models
{
    [XmlRoot("AlternationGroup")]
    public class AlternationGroup : IPackagePart
    {

        public Uri URI { get; set; }
        public String MimeType { get { return "text/xml+" + typeof(AlternationGroup).ToString(); } }
        public Type BaseType { get { return typeof(AlternationGroup); } }
        private static readonly List<int> GroupIds = new List<int>();
        public int GroupID { get; private set; } = 0;
        public readonly List<IContentsItem> GroupMembers = new List<IContentsItem>();
        private List<String> rGroupMemberIds = new List<string>();

        private static IEnumerable<AlternationGroup> AlternationGroups
        {
            get
            {
                return CIAT.SaveFile.GetRelationshipsByType(CIAT.SaveFile.IAT.URI, typeof(CIAT), typeof(AlternationGroup)).Select(pr => CIAT.SaveFile.GetAlternationGroup(pr.TargetUri));
            }
        }

        public void Remove(IContentsItem ici)
        {
            CIAT.SaveFile.DeleteRelationship(this.URI, ici.URI);
        }

        public AlternationGroup(IContentsItem[] groupMembers)
        {
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            CIAT.SaveFile.Register(this);
            CIAT.SaveFile.CreateRelationship(typeof(CIAT), BaseType, CIAT.SaveFile.IAT.URI, URI);
            GroupID = 0;
            while (GroupIds.Contains(GroupID))
                GroupID++;
            GroupIds.Add(GroupID);
            GroupMembers.AddRange(groupMembers);
            foreach (IContentsItem i in GroupMembers)
            {
                i.AlternationGroup = this;
                rGroupMemberIds.Add(CIAT.SaveFile.CreateRelationship(BaseType, i.BaseType, this.URI, i.URI));
            }
        }

        public AlternationGroup(IContentsItem item1, IContentsItem item2)
        {
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            CIAT.SaveFile.Register(this);
            CIAT.SaveFile.CreateRelationship(typeof(CIAT), BaseType, CIAT.SaveFile.IAT.URI, URI);
            GroupID = 0;
            while (GroupIds.Contains(GroupID))
                GroupID++;
            GroupIds.Add(GroupID);
            item1.AlternationGroup = this;
            item2.AlternationGroup = this;
            GroupMembers.Add(item1);
            GroupMembers.Add(item2);
            rGroupMemberIds.Add(CIAT.SaveFile.CreateRelationship(BaseType, item1.BaseType, URI, item1.URI));
            rGroupMemberIds.Add(CIAT.SaveFile.CreateRelationship(BaseType, item2.BaseType, URI, item2.URI));
        }

        public AlternationGroup(Uri u)
        {
            URI = u;
            CIAT.SaveFile.Register(this);
            Load();
        }

        public bool Contains(IContentsItem i)
        {
            return GroupMembers.Contains(i);
        }

        public void Dispose()
        {
            foreach (IContentsItem i in GroupMembers)
                i.AlternationGroup = null;
            CIAT.SaveFile.DeleteRelationship(CIAT.SaveFile.IAT.URI, this.URI);
            GroupIds.Remove(GroupID);
        }

        public int AlternationPriority
        {
            get
            {
                if ((GroupMembers[0].Type != ContentsItemType.BeforeSurvey) && (GroupMembers[0].Type != ContentsItemType.AfterSurvey))
                    return -1;
                return AlternationGroups.Where((group) => (group.GroupID < GroupID)).Where((group) => (group.AlternationPriority != -1)).Count();
            }
        }

        public bool IsValid()
        {
            return true;
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("AlternationGroup", new XAttribute("GroupID", GroupID.ToString())));
            foreach (String rId in rGroupMemberIds)
                xDoc.Root.Add(new XElement("rMemberId", rId));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public void Load()
        {
            XDocument xDoc = null;
            Stream s = Stream.Synchronized(CIAT.SaveFile.GetReadStream(this));
            try
            {
                xDoc = XDocument.Load(s);
                s.Dispose();
            }
            finally
            {
                CIAT.SaveFile.ReleaseReadStreamLock();
            }
            GroupID = Convert.ToInt32(xDoc.Root.Attribute("GroupID").Value);
            GroupIds.Add(GroupID);
            foreach (XElement elem in xDoc.Root.Elements("rMemberId"))
            {
                rGroupMemberIds.Add(elem.Value);
                Uri targetUri = CIAT.SaveFile.GetRelationship(this, elem.Value).TargetUri;
                String sType = CIAT.SaveFile.GetTypeName(targetUri);
                IContentsItem item = null;
                if (sType == typeof(CIATBlock).ToString())
                    item = CIAT.SaveFile.GetIATBlock(targetUri);
                else if (sType == typeof(CInstructionBlock).ToString())
                    item = CIAT.SaveFile.GetInstructionBlock(targetUri);
                else if (sType == typeof(CSurvey).ToString())
                    item = CIAT.SaveFile.GetSurvey(targetUri);
                else
                    throw new Exception("Unrecognized alternation group member type.");
                item.AlternationGroup = this;
                GroupMembers.Add(item);
            }
        }
        /*
                static public void WriteToXml(XmlTextWriter writer)
                {
                    writer.WriteStartElement("AlternationGroups");
                    writer.WriteAttributeString("NumAlternationGroups", AlternationGroups.Keys.Count.ToString());
                    writer.WriteElementString("PrefixSelfAlternatingSurveys", PrefixSelfAlternatingSurveys.ToString());
                    foreach (AlternationGroup g in AlternationGroups.Values)
                    {
                        writer.WriteStartElement("AlternationGroup");
                        writer.WriteElementString("GroupID", g.GroupID.ToString());
                        writer.WriteStartElement("GroupMembers");
                        writer.WriteAttributeString("NumGroupMembers", g.GroupMembers.Count.ToString());
                        foreach (IContentsItem i in g.GroupMembers)
                            writer.WriteElementString("GroupMember", i.Name);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                public static void CreateFromXml(XmlNode node, CIAT iat)
                {
                    int nGroups = Convert.ToInt32(node.Attributes["NumAlternationGroups"].Value);
                    PrefixSelfAlternatingSurveys = Convert.ToBoolean(node.ChildNodes[0].InnerText);
                    List<IContentsItem> items = new List<IContentsItem>();
                    for (int ctr1 = 0; ctr1 < nGroups; ctr1++)
                    {
                        int nID = Convert.ToInt32(node.ChildNodes[ctr1 + 1].ChildNodes[0].InnerText);
                        int nMembers = Convert.ToInt32(node.ChildNodes[ctr1 + 1].ChildNodes[1].Attributes["NumGroupMembers"].Value);
                        for (int ctr2 = 0; ctr2 < nMembers; ctr2++)
                        {
                            String name = node.ChildNodes[ctr1 + 1].ChildNodes[1].ChildNodes[ctr2].InnerText;
                            foreach (IContentsItem ci in iat.Contents)
                            {
                                if (ci.Name == name)
                                {
                                    items.Add(ci);
                                    break;
                                }
                            }
                        }
                        AlternationGroup g = new AlternationGroup(items.ToArray());
                        g._GroupID = nID;
                        items.Clear();
                    }
                }

                static public ICollection<AlternationGroup> Groups
                {
                    get
                    {
                        return AlternationGroups.Values;
                    }
                }
                */
    }
}

