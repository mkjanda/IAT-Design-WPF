using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Models
{
    public class ContentsItemType
    {
        public static readonly ContentsItemType BeforeSurvey = new ContentsItemType("BeforeSurvey", new Func<Uri, IContentsItem>(u => CIAT.SaveFile.GetSurvey(u)));
        public static readonly ContentsItemType AfterSurvey = new ContentsItemType("AfterSurvey", new Func<Uri, IContentsItem>(u => CIAT.SaveFile.GetSurvey(u)));
        public static readonly ContentsItemType IATBlock = new ContentsItemType("IATBlock", new Func<Uri, IContentsItem>(u => CIAT.SaveFile.GetIATBlock(u)));
        public static readonly ContentsItemType InstructionBlock = new ContentsItemType("InstructionBlock", new Func<Uri, IContentsItem>(u => CIAT.SaveFile.GetInstructionBlock(u)));

        private static IEnumerable<ContentsItemType> All = new ContentsItemType[] { BeforeSurvey, AfterSurvey, IATBlock, InstructionBlock };
        private String Name { get; set; }
        public Func<Uri, IContentsItem> Fetch { get; private set; }
        private ContentsItemType(String name, Func<Uri, IContentsItem> create)
        {
            Name = name;
            Fetch = create;
        }
        public new String ToString()
        {
            return Name;
        }
        public static ContentsItemType Parse(String name)
        {
            return All.Where(cit => cit.Name == name).First();
        }
    }
    public interface IContentsItem : IPreviewableItem, IDisposable, IPackagePart
    {
        ContentsItemType Type { get; }
        bool HasAlternateItem { get; }
        
        AlternationGroup? AlternationGroup { get; set; }
        String Name { get; }
        int IndexInContainer { get; }
        int IndexInContents { get; }
        void DeleteFromIAT();
        void AddToIAT(int InsertionNdx);
        List<IPreviewableItem> SubContentsItems { get; }
    }
}
