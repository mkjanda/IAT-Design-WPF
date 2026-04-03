using IAT.Core.Models.Enumerations;
using IAT.Core.Services;
using sun.awt.geom;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace IAT.Core.Models
{
    public partial class AlternationGroup : IPackagePart
    {
        /// <summary>
        /// Gets the URI associated with this instance.
        /// </summary>
        [XmlElement("URI", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public required Uri Uri { get; set; }


        /// <summary>
        /// Gets the unique identifier for the group.
        /// </summary>
        [XmlElement("GroupID", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public required int GroupID { get; init; }

        /// <summary>
        /// Gets the list of group identifiers.
        /// </summary>
        public static List<int> GroupIds { get; } = new();


        /// <summary>
        /// Gets the type of the package item represented by this instance.
        /// </summary>
        [XmlIgnore]
        public PartType PackagePartType => PartType.AlternationGroup;


        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [XmlElement("Id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Guid Id { get; set; } = new();

        /// <summary>
        /// Represents the collection of items that are members of the group.
        /// </summary>
        /// <remarks>This list is read-only and can be used to access or enumerate the group members.
        /// Modifications to the collection itself are not supported.</remarks>
        [XmlIgnore]
        public required List<IContentsItem> GroupMembers { get; init; } = new();
  

        /// <summary>
        /// Removes the specified contents item from the group.
        /// </summary>
        /// <remarks>After removal, the item's association with this group is cleared. If the item is not
        /// a member of the group, no action is taken.</remarks>
        /// <param name="ici">The contents item to remove from the group. Cannot be null.</param>
        public void Remove(IContentsItem ici)
        {
            GroupMembers.Remove(ici);
            ici.AlternationGroup = null;
        }


        /// <summary>
        /// Initializes a new instance of the AlternationGroup class with the specified group members.
        /// </summary>
        /// <remarks>Each item in the group will have its AlternationGroup property set to this instance.
        /// The group is assigned a unique identifier among all alternation groups.</remarks>
        /// <param name="groupMembers">An array of items to include as members of the alternation group. Cannot be null.</param>
        public AlternationGroup(IContentsItem[] groupMembers)
        {
            GroupID = 0;
            while (GroupIds.Contains(GroupID))
                GroupID++;
            GroupIds.Add(GroupID);
            GroupMembers.AddRange(groupMembers);
            foreach (IContentsItem i in GroupMembers)
                i.AlternationGroup = this;
        }

        /// <summary>
        /// Initializes a new instance of the AlternationGroup class with the specified contents items as group members.
        /// </summary>
        /// <remarks>Both items are assigned to the same alternation group and added as group members.
        /// Each group is assigned a unique identifier.</remarks>
        /// <param name="item1">The first contents item to include in the alternation group. Cannot be null.</param>
        /// <param name="item2">The second contents item to include in the alternation group. Cannot be null.</param>
        public AlternationGroup(IContentsItem item1, IContentsItem item2)
        {
            GroupID = 0;
            while (GroupIds.Contains(GroupID))
                GroupID++;
            GroupIds.Add(GroupID);
            item1.AlternationGroup = this;
            item2.AlternationGroup = this;
            GroupMembers.Add(item1);
            GroupMembers.Add(item2);
        }


        /// <summary>
        /// Releases all resources used by the current instance and removes the group from the collection of active
        /// groups.
        /// </summary>
        /// <remarks>Call this method when the group is no longer needed to ensure that all associated
        /// resources are properly released. After calling this method, the group and its members should not be
        /// used.</remarks>
        public void Dispose()
        {
            foreach (IContentsItem i in GroupMembers)
                i.AlternationGroup = null;
            GroupIds.Remove(GroupID);
        }
    }
}

